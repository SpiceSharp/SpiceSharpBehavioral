using SpiceSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A parser that uses the Shunting-Yard algorithm for parsing an expression.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="Parameterized" />
    /// <seealso cref="IParser{T}" />
    public partial class Parser<T> : Parameterized, IParser<T>, IParameterized<IParserParameters<T>>
    {
        private string _input;
        private int _index;
        private bool _expectInfixPostfix;
        private readonly Stack<Operators> _operators = new Stack<Operators>();
        private readonly Stack<T> _values = new Stack<T>();
        private readonly Stack<FunctionDeclaration> _functions = new Stack<FunctionDeclaration>();

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public IParserParameters<T> Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser{T}"/> class.
        /// </summary>
        public Parser(IParserParameters<T> parameters)
        {
            Parameters = parameters.ThrowIfNull(nameof(parameters));
        }

        /// <summary>
        /// Parses an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// The result of the parsed expression.
        /// </returns>
        /// <exception cref="ParserException">Thrown when the expression is invalid.</exception>
        public T Parse(string expression)
        {
            _index = 0;
            _input = expression.ThrowIfNull(nameof(expression));
            _expectInfixPostfix = false;
            _operators.Clear();
            _values.Clear();

            while (_index < _input.Length)
            {
                // Skip spaces
                while (_index < _input.Length && _input[_index] == ' ')
                    _index++;
                if (_index >= _input.Length)
                    break;

                // Parse binary operators
                if (_expectInfixPostfix)
                {
                    _expectInfixPostfix = false;
                    ReadBinaryOperator();
                }
                else
                {
                    _expectInfixPostfix = true;
                    ReadUnaryOperator();
                }
            }

            while (_operators.Count > 0)
                ExecuteOperator(_operators.Pop());
            if (_values.Count != 1)
                throw new ParserException("Invalid expression", _input, _index);
            return _values.Pop();
        }

        /// <summary>
        /// Reads a binary operator.
        /// </summary>
        private void ReadBinaryOperator()
        {
            var c = _input[_index];
            switch (c)
            {
                case '+': PushOperator(Operators.Addition); _index++; return;
                case '-': PushOperator(Operators.Subtraction); _index++; return;
                case '*': PushOperator(Operators.Multiplication); _index++; return;
                case '/': PushOperator(Operators.Division); _index++; return;
                case '%': PushOperator(Operators.Modulo); _index++; return;
                case '?': PushOperator(Operators.Ternary); _index++; return;
                case ':': PushOperator(Operators.TernarySeparator); _index++; return;
                case '^': PushOperator(Operators.Power); _index++; return;
                case '<':
                    if (NextChar() == '=')
                    {
                        PushOperator(Operators.LessOrEqual);
                        _index += 2;
                    }
                    else
                    {
                        PushOperator(Operators.Less);
                        _index++;
                    }
                    return;
                case '>':
                    if (NextChar() == '=')
                    {
                        PushOperator(Operators.GreaterOrEqual);
                        _index += 2;
                    }
                    else
                    {
                        PushOperator(Operators.Greater);
                        _index++;
                    }
                    return;
                case '=':
                    if (NextChar() == '=')
                    {
                        PushOperator(Operators.Equal);
                        _index += 2;
                    }
                    else
                        throw new Exception("Invalid operator");
                    return;
                case '!':
                    if (NextChar() == '=')
                    {
                        PushOperator(Operators.NotEqual);
                        _index += 2;
                    }
                    else
                        throw new Exception("Invalid operator");
                    return;
                case '&':
                    if (NextChar() == '&')
                    {
                        PushOperator(Operators.And);
                        _index += 2;
                    }
                    else
                        throw new Exception("Invalid operator");
                    return;
                case '|':
                    if (NextChar() == '|')
                    {
                        PushOperator(Operators.Or);
                        _index += 2;
                    }
                    else
                        throw new Exception("Invalid operator");
                    return;
                case ',':
                    PushOperator(Operators.Argument);
                    _operators.Pop(); // Pop the argument off again
                    var top = _operators.Count > 0 ? _operators.Peek() : throw new Exception("Invalid argument");
                    if (top == Operators.Function)
                        _functions.Peek().Arguments++;
                    else
                        throw new Exception("Invalid argument");
                    _index++;
                    break;
                case ')':
                    PushOperator(Operators.RightBracket);
                    _operators.Pop(); // Pop the right bracket off again
                    switch (_operators.Count > 0 ? _operators.Peek() : throw new Exception("Invalid bracket"))
                    {
                        case Operators.LeftBracket:
                            _operators.Pop();
                            break;
                        case Operators.Function:
                            _functions.Peek().Arguments++; // We expected a binary operator, so this comes after an argument
                            ExecuteOperator(_operators.Pop());
                            break;
                        default:
                            throw new Exception("Invalid bracket");
                    }
                    _index++;
                    _expectInfixPostfix = true;
                    return;
                default:
                    throw new ParserException("Unrecognized operator", _input, _index);
            }
        }

        /// <summary>
        /// Reads a unary operator.
        /// </summary>
        private void ReadUnaryOperator()
        {
            char c = _input[_index];
            double value;

            // Do parameters
            char nc;
            switch (c)
            {
                case '+': PushOperator(Operators.Positive); _expectInfixPostfix = false; _index++; return;
                case '-':
                    nc = NextChar();
                    if (nc >= '0' && nc <= '9')
                    {
                        _index++;
                        value = -ParseDouble();
                        nc = NextChar();
                        if (nc >= 'a' && nc <= 'z' || nc >= 'A' && nc <= 'Z')
                        {
                            string unit = NextWord();
                            _index += unit.Length;
                            _values.Push(Parameters.ValueFactory.CreateValue(value, unit));
                        }
                        else
                            _values.Push(Parameters.ValueFactory.CreateValue(value, ""));
                    }
                    else
                    {
                        PushOperator(Operators.Negative);
                        _expectInfixPostfix = false;
                        _index++;
                    }
                    return;
                case '!': PushOperator(Operators.Not); _index++; _expectInfixPostfix = false; return;
                case '(': _operators.Push(Operators.LeftBracket); _expectInfixPostfix = false; _index++; return;
                case ')':
                    PushOperator(Operators.RightBracket);
                    _operators.Pop(); // Pop the right bracket off again
                    var top = _operators.Count > 0 ? _operators.Peek() : throw new Exception("Invalid bracket");
                    if (top == Operators.LeftBracket)
                        _operators.Pop();
                    else if (top == Operators.Function)
                    {
                        _functions.Peek().Arguments++;
                        ExecuteOperator(_operators.Pop()); // Execute the function
                    }
                    else
                        throw new Exception("Invalid bracket");
                    _index++;
                    return;
                case var mc when mc >= 'a' && mc <= 'z' || mc >= 'A' && mc <= 'Z' || mc == '_':
                    var name = NextWord();
                    if (NextChar(name.Length) == '(')
                    {
                        _index += name.Length + 1;
                        _functions.Push(new FunctionDeclaration(name));
                        PushOperator(Operators.Function);
                        _expectInfixPostfix = false;
                        return;
                    }
                    else
                    {
                        _index += name.Length;
                        _values.Push(Parameters.ValueFactory.CreateVariable(name));
                        return;
                    }
                case var mc when mc >= '0' && mc <= '9':
                    value = ParseDouble();
                    nc = NextChar();
                    if (nc >= 'a' && nc <= 'z' || nc >= 'A' && nc <= 'Z')
                    {
                        string unit = NextWord();
                        _index += unit.Length;
                        _values.Push(Parameters.ValueFactory.CreateValue(value, unit));
                    }
                    else
                        _values.Push(Parameters.ValueFactory.CreateValue(value, ""));
                    return;
            }
            throw new ParserException("Invalid operator", _input, _index);
        }

        /// <summary>
        /// Parses a double value.
        /// </summary>
        /// <returns>The value.</returns>
        private double ParseDouble()
        {
            double value = 0.0;

            // Read integer part
            while (_index < _input.Length && (_input[_index] >= '0' && _input[_index] <= '9'))
                value = (value * 10.0) + (_input[_index++] - '0');

            // Read decimal part
            if (_index < _input.Length && (_input[_index] == '.'))
            {
                _index++;
                double mult = 1.0;
                while (_index < _input.Length && (_input[_index] >= '0' && _input[_index] <= '9'))
                {
                    value = (value * 10.0) + (_input[_index++] - '0');
                    mult *= 10.0;
                }
                value /= mult;
            }

            if (_index < _input.Length)
            {
                // Scientific notation
                if (_input[_index] == 'e' || _input[_index] == 'E')
                {
                    _index++;
                    var exponent = 0;
                    var neg = false;
                    if (_index < _input.Length && (_input[_index] == '+' || _input[_index] == '-'))
                    {
                        if (_input[_index] == '-')
                            neg = true;
                        _index++;
                    }

                    // Get the exponent
                    while (_index < _input.Length && (_input[_index] >= '0' && _input[_index] <= '9'))
                        exponent = (exponent * 10) + (_input[_index++] - '0');

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
                else
                {
                    // Spice modifiers
                    switch (_input[_index])
                    {
                        case 't':
                        case 'T': value *= 1.0e12; _index++; break;
                        case 'g':
                        case 'G': value *= 1.0e9; _index++; break;
                        case 'x':
                        case 'X': value *= 1.0e6; _index++; break;
                        case 'k':
                        case 'K': value *= 1.0e3; _index++; break;
                        case 'u':
                        case 'µ':
                        case 'U': value /= 1.0e6; _index++; break;
                        case 'n':
                        case 'N': value /= 1.0e9; _index++; break;
                        case 'p':
                        case 'P': value /= 1.0e12; _index++; break;
                        case 'f':
                        case 'F': value /= 1.0e15; _index++; break;
                        case 'm':
                        case 'M':
                            if (_index + 2 < _input.Length &&
                                (_input[_index + 1] == 'e' || _input[_index + 1] == 'E') &&
                                (_input[_index + 2] == 'g' || _input[_index + 2] == 'G'))
                            {
                                value *= 1.0e6;
                                _index += 3;
                            }
                            else if (_index + 2 < _input.Length &&
                                (_input[_index + 1] == 'i' || _input[_index + 1] == 'I') &&
                                (_input[_index + 2] == 'l' || _input[_index + 2] == 'L'))
                            {
                                value *= 25.4e-6;
                                _index += 3;
                            }
                            else
                            {
                                value /= 1.0e3;
                                _index++;
                            }
                            break;
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Look ahead for the next character
        /// </summary>
        /// <returns>The character at the next position</returns>
        private char NextChar(int offset = 1)
        {
            if (_index + offset < _input.Length)
                return _input[_index + offset];
            return '\0';
        }

        /// <summary>
        /// Looks ahead for a word. Assumes the first character has already been matched!
        /// </summary>
        /// <returns></returns>
        private string NextWord()
        {
            var index = _index;
            var sb = new StringBuilder(32);
            while (_input[index] >= '0' && _input[index] <= '9' ||
                   _input[index] >= 'a' && _input[index] <= 'z' ||
                   _input[index] >= 'A' && _input[index] <= 'Z' ||
                   _input[index] == '_')
            {
                sb.Append(_input[index]);
                index++;
                if (index >= _input.Length)
                    return sb.ToString();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Pushes the operator.
        /// </summary>
        /// <param name="op">The op.</param>
        private void PushOperator(Operators op)
        {
            while (_operators.Count > 0 && HasPrecedence(_operators.Peek(), op))
                ExecuteOperator(_operators.Pop());
            _operators.Push(op);
        }

        /// <summary>
        /// Executes the operator.
        /// </summary>
        /// <param name="op">The operator.</param>
        private void ExecuteOperator(Operators op)
        {
            T a, b, c;
            switch (op)
            {
                case Operators.Positive:
                    break;
                case Operators.Negative:
                    a = _values.Pop();
                    _values.Push(Parameters.Arithmetic.Negate(a));
                    break;
                case Operators.TernarySeparator:
                    if (_operators.Count == 0 || _operators.Pop() != Operators.Ternary)
                        throw new Exception("Invalid ternary operator");
                    c = _values.Pop();
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(Parameters.Conditional.IfThenElse(a, b, c));
                    break;
                case Operators.Addition:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(Parameters.Arithmetic.Add(a, b));
                    break;
                case Operators.Subtraction:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(Parameters.Arithmetic.Subtract(a, b));
                    break;
                case Operators.Multiplication:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(Parameters.Arithmetic.Multiply(a, b));
                    break;
                case Operators.Division:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(Parameters.Arithmetic.Divide(a, b));
                    break;
                case Operators.Power:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(Parameters.Arithmetic.Pow(a, b));
                    break;
                case Operators.Modulo:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(Parameters.Arithmetic.Modulo(a, b));
                    break;
                case Operators.Not:
                    a = _values.Pop();
                    _values.Push(Parameters.Conditional.Not(a));
                    break;
                case Operators.And:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(Parameters.Conditional.And(a, b));
                    break;
                case Operators.Or:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(Parameters.Conditional.Or(a, b));
                    break;
                case Operators.Equal:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(Parameters.Relational.Equal(a, b));
                    break;
                case Operators.NotEqual:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(Parameters.Relational.NotEqual(a, b));
                    break;
                case Operators.Greater:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(Parameters.Relational.GreaterThan(a, b));
                    break;
                case Operators.Less:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(Parameters.Relational.LessThan(a, b));
                    break;
                case Operators.GreaterOrEqual:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(Parameters.Relational.GreaterOrEqual(a, b));
                    break;
                case Operators.LessOrEqual:
                    b = _values.Pop();
                    a = _values.Pop();
                    _values.Push(Parameters.Relational.LessOrEqual(a, b));
                    break;
                case Operators.Function:
                    var function = _functions.Pop();
                    var arguments = new T[function.Arguments];
                    for (var i = function.Arguments - 1; i >= 0; i--)
                        arguments[i] = _values.Pop();
                    _values.Push(Parameters.ValueFactory.CreateFunction(function.Name, arguments));
                    break;
                default:
                    throw new Exception("Unrecognized operator");
            }
        }
    }
}
