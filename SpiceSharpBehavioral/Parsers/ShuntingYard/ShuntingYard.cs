using SpiceSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpBehavioral.Parsers.ShuntingYard
{
    /// <summary>
    /// Describes an <see cref="IParserState{T}"/> that uses the Shunting-Yard algorithm to be able to resolve.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public partial class ShuntingYard<T> : IParserState<T>
    {
        private const int _initialStackSize = 16;
        private const int _allocateWordSize = 16;

        private readonly IShuntingYardOperators<T> _definition;
        private readonly Stack<Operators> _operators = new Stack<Operators>(_initialStackSize);
        private readonly Stack<T> _values = new Stack<T>(_initialStackSize);
        private readonly Stack<FunctionDeclaration> _functions = new Stack<FunctionDeclaration>(_initialStackSize);
        private bool _expectInfixPostfix = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShuntingYard{T}"/> class.
        /// </summary>
        /// <param name="definition">The definition.</param>
        public ShuntingYard(IShuntingYardOperators<T> definition)
        {
            _definition = definition.ThrowIfNull(nameof(definition));
        }

        /// <summary>
        /// Finish up and return the final result.
        /// </summary>
        /// <returns>
        /// The final result.
        /// </returns>
        /// <exception cref="ParserException">Thrown if the expression turned out to be invalid.</exception>
        public T Execute()
        {
            while (_operators.Count > 0)
                ApplyOperator(_operators.Pop());
            if (_values.Count != 1)
                throw new ParserException("Invalid expression");
            return _values.Pop();
        }

        /// <summary>
        /// Reads a token.
        /// </summary>
        /// <param name="reader">The lexer.</param>
        public void ParseToken(IReader reader)
        {
            // Skip spaces
            while (reader.Peek() == ' ')
                reader.Read();
            if (reader.Peek() == '\0')
                return;

            // Parse operators
            if (_expectInfixPostfix)
            {
                _expectInfixPostfix = false;
                ReadBinaryOperator(reader);
            }
            else
            {
                _expectInfixPostfix = true;
                ReadUnaryOperator(reader);
            }
        }

        /// <summary>
        /// Pushes the operator.
        /// </summary>
        /// <param name="operator">The operator.</param>
        private void PushOperator(Operators @operator)
        {
            while (_operators.Count > 0 && HasPrecedence(_operators.Peek(), @operator))
                ApplyOperator(_operators.Pop());
            _operators.Push(@operator);
        }

        /// <summary>
        /// Reads the binary operator.
        /// </summary>
        /// <param name="reader">The lexer.</param>
        private void ReadBinaryOperator(IReader reader)
        {
            switch (reader.Read())
            {
                case '+': PushOperator(Operators.Addition); break;
                case '-': PushOperator(Operators.Subtraction); break;
                case '*': PushOperator(Operators.Multiplication); break;
                case '/': PushOperator(Operators.Division); break;
                case '^': PushOperator(Operators.Power); break;
                case '%': PushOperator(Operators.Modulo); break;

                case '=':
                    if (reader.Read() != '=')
                        throw new ParserException("Unexpected character", reader.Index);
                    PushOperator(Operators.Equal);
                    break;
                case '!':
                    if (reader.Read() != '=')
                        throw new ParserException("Unexpected character", reader.Index);
                    PushOperator(Operators.NotEqual);
                    break;
                case '<':
                    if (reader.Peek() == '=')
                    {
                        reader.Read();
                        PushOperator(Operators.LessOrEqual);
                    }
                    else
                        PushOperator(Operators.Less);
                    break;
                case '>':
                    if (reader.Peek() == '=')
                    {
                        reader.Read();
                        PushOperator(Operators.GreaterOrEqual);
                    }
                    else
                        PushOperator(Operators.Greater);
                    break;
                case '?':
                    PushOperator(Operators.Ternary);
                    break;
                case ':':
                    PushOperator(Operators.TernarySeparator);
                    break;

                case ')':
                    PushOperator(Operators.RightBracket);
                    _operators.Pop();
                    if (_operators.Count == 0)
                        throw new ParserException("Invalid closing bracket");
                    var next = _operators.Pop();
                    if (next == Operators.Function)
                        _functions.Peek().Arguments++;
                    else if (next != Operators.LeftBracket)
                        throw new ParserException("Invalid closing bracket");
                    ApplyOperator(next);
                    _expectInfixPostfix = true;
                    break;
                case ',':
                    PushOperator(Operators.Argument);
                    _operators.Pop();
                    if (_operators.Count == 0 || _operators.Peek() != Operators.Function)
                        throw new ParserException("Invalid argument");
                    _functions.Peek().Arguments++;
                    break;

                default:
                    throw new ParserException("Unrecognized binary operator", reader.Index);
            }
        }

        /// <summary>
        /// Reads the unary operator.
        /// </summary>
        /// <param name="reader">The lexer.</param>
        private void ReadUnaryOperator(IReader reader)
        {
            switch (reader.Read())
            {
                case '+': PushOperator(Operators.Positive); _expectInfixPostfix = false; break;
                case '-': PushOperator(Operators.Negative); _expectInfixPostfix = false; break;
                case '!': PushOperator(Operators.Not); _expectInfixPostfix = false; break;
                case ')': PushOperator(Operators.RightBracket); break;
                case '(': PushOperator(Operators.LeftBracket); _expectInfixPostfix = false; break;
                case var mc when mc >= 'a' && mc <= 'z' || mc >= 'A' && mc <= 'Z' || mc == '_':
                    // Read the variable or function name
                    var sb = new StringBuilder(_allocateWordSize);
                    sb.Append(mc);
                    while (char.IsLetterOrDigit(reader.Peek()) || reader.Peek() == '_')
                        sb.Append(reader.Read());
                    if (reader.Peek() == '(')
                    {
                        reader.Read();
                        _operators.Push(Operators.Function);
                        _functions.Push(new FunctionDeclaration(sb.ToString(), reader.Index));
                        _expectInfixPostfix = false;
                    }
                    else
                        _values.Push(_definition.CreateVariable(sb.ToString()));
                    break;
                case var mc when mc >= '0' && mc <= '9' || mc == '.':
                    _values.Push(_definition.CreateValue(ReadDouble(mc, reader)));
                    break;

                default:
                    throw new ParserException("Unrecognized unary operator", reader.Index);
            }
        }

        /// <summary>
        /// Applies the operator on the stack.
        /// </summary>
        /// <param name="operator">The operator.</param>
        private void ApplyOperator(Operators @operator)
        {
            T a, b, c;
            switch (@operator)
            {
                case Operators.Positive:
                    a = _values.Pop();
                    _values.Push(_definition.UnaryPlus(a));
                    break;
                case Operators.Negative:
                    a = _values.Pop();
                    _values.Push(_definition.UnaryMinus(a));
                    break;
                case Operators.Not:
                    a = _values.Pop();
                    _values.Push(_definition.Not(a));
                    break;

                case Operators.Addition:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(_definition.Add(a, b));
                    break;
                case Operators.Subtraction:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(_definition.Subtract(a, b));
                    break;
                case Operators.Multiplication:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(_definition.Multiply(a, b));
                    break;
                case Operators.Division:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(_definition.Divide(a, b));
                    break;
                case Operators.Power:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(_definition.Pow(a, b));
                    break;
                case Operators.Modulo:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(_definition.Modulo(a, b));
                    break;

                case Operators.Equal:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(_definition.Equal(a, b));
                    break;
                case Operators.NotEqual:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(_definition.NotEqual(a, b));
                    break;
                case Operators.Greater:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(_definition.Greater(a, b));
                    break;
                case Operators.Less:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(_definition.Less(a, b));
                    break;
                case Operators.GreaterOrEqual:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(_definition.GreaterOrEqual(a, b));
                    break;
                case Operators.LessOrEqual:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(_definition.LessOrEqual(a, b));
                    break;

                case Operators.TernarySeparator:
                    c = _values.Pop();
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(_definition.IfThenElse(a, b, c));

                    // The next ternary operator should be removed
                    if (_operators.Count == 0 || _operators.Peek() != Operators.Ternary)
                        throw new ParserException("Invalid ternary operator");
                    _operators.Pop();
                    break;

                case Operators.LeftBracket:
                case Operators.RightBracket:
                case Operators.Argument:
                    break;

                case Operators.Function:
                    var fd = _functions.Pop();
                    T[] arguments = new T[fd.Arguments];
                    for (var i = fd.Arguments - 1; i >= 0; i--)
                        arguments[i] = _values.Pop();
                    _values.Push(_definition.CreateFunction(fd.Name, arguments));
                    break;

                default:
                    throw new Exception("Unimplemented operator");
            }
        }

        /// <summary>
        /// Parse a double value.
        /// </summary>
        /// <param name="leading">The already parsed digit.</param>
        /// <param name="reader">The reader.</param>
        /// <returns>The parsed result.</returns>
        protected virtual double ReadDouble(char leading, IReader reader)
        {
            double value = leading - '0';

            // Read integer part
            while (char.IsDigit(reader.Peek()))
                value = (value * 10.0) + (reader.Read() - '0');

            // Read decimal part
            if (reader.Peek() == '.')
            {
                reader.Read();
                double mult = 1.0;
                while (char.IsDigit(reader.Peek()))
                {
                    value = (value * 10.0) + (reader.Read() - '0');
                    mult *= 10.0;
                }

                value /= mult;
            }

            // Read scientific input
            if (reader.Peek() == 'e' || reader.Peek() == 'E')
            {
                reader.Read();
                int exponent = 0;
                bool neg = false;
                if (reader.Peek() == '+' || reader.Peek() == '-')
                {
                    if (reader.Read() == '-')
                        neg = true;
                }

                // Get the exponent
                while (char.IsDigit(reader.Peek()))
                    exponent = (exponent * 10) + (reader.Read() - '0');

                // Integer exponentation
                var mult = 1.0;
                var b = 10.0;
                while (exponent != 0)
                {
                    if ((exponent & 0x01) == 0x01)
                        mult *= b;

                    b *= b;
                    exponent >>= 1;
                }

                if (neg)
                    value /= mult;
                else
                    value *= mult;
            }
            // Spice modifiers/units
            else if (char.IsLetter(reader.Peek()))
            {
                // Find all trailing letters
                var sb = new StringBuilder(_allocateWordSize);
                sb.Append(reader.Read());
                while (char.IsLetter(reader.Peek()))
                    sb.Append(reader.Read());
                string units = sb.ToString();

                switch (units[0])
                {
                    case 't':
                    case 'T': value *= 1.0e12; break;
                    case 'g':
                    case 'G': value *= 1.0e9;break;
                    case 'x':
                    case 'X': value *= 1.0e6; break;
                    case 'k':
                    case 'K': value *= 1.0e3; break;
                    case 'u':
                    case 'µ':
                    case 'U': value /= 1.0e6; break;
                    case 'n':
                    case 'N': value /= 1.0e9; break;
                    case 'p':
                    case 'P': value /= 1.0e12; break;
                    case 'f':
                    case 'F': value /= 1.0e15; break;
                    case 'm':
                    case 'M':
                        if (units.Length > 3)
                        {
                            if ((units[1] == 'e' || units[1] == 'E') && (units[2] == 'g' || units[2] == 'G'))
                                value *= 1.0e6;
                            else if ((units[1] == 'i' || units[1] == 'I') && (units[2] == 'l' || units[2] == 'L'))
                                value *= 25.4e-6;
                            else
                                value *= 1e-3;
                        }
                        else
                            value *= 1e-3;
                        break;
                }
            }
            return value;
        }
    }
}
