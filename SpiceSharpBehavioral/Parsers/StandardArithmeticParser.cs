using System.Text;
using SpiceSharpBehavioral.Parsers.Operators;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A class capable of applying the Shunting Yard algorithm for standard arithmetic rules.
    /// This means it has precedence and associativity built in for the unary operator positive,
    /// negative and not. It describes the binary operators addition, subtraction, multiplication,
    /// division, modulo, powers, equality, non-equality, or, and greater/less (or equal) than and
    /// the ternary operator. It also supports brackets and function calls.
    /// </summary>
    /// <seealso cref="ShuntingYard" />
    public abstract class StandardArithmeticParser : ShuntingYard
    {
        // Define all operators for parsing arithmetic expressions
        private static readonly Operator _positive = new ArithmeticOperator(OperatorType.Positive, (int) OperatorPrecedence.Unary, false); // ++a = +(+a)
        private static readonly Operator _negative = new ArithmeticOperator(OperatorType.Negative, (int) OperatorPrecedence.Unary, false); // --a = -(-a)
        private static readonly Operator _not = new ArithmeticOperator(OperatorType.Not, (int) OperatorPrecedence.Unary, false); // !!a = !(!a)
        private static readonly Operator _add = new ArithmeticOperator(OperatorType.Add, (int) OperatorPrecedence.Additive, true);
        private static readonly Operator _subtract = new ArithmeticOperator(OperatorType.Subtract, (int) OperatorPrecedence.Additive, true);
        private static readonly Operator _multiply = new ArithmeticOperator(OperatorType.Multiply, (int) OperatorPrecedence.Multiplicative, true);
        private static readonly Operator _divide = new ArithmeticOperator(OperatorType.Divide, (int) OperatorPrecedence.Multiplicative, true);
        private static readonly Operator _modulo = new ArithmeticOperator(OperatorType.Modulo, (int) OperatorPrecedence.Multiplicative, true);
        private static readonly Operator _power = new ArithmeticOperator(OperatorType.Power, (int) OperatorPrecedence.Power, false); // a^b^c = a^(b^c)
        private static readonly Operator _isEqual = new ArithmeticOperator(OperatorType.IsEqual, (int) OperatorPrecedence.Equality, true);
        private static readonly Operator _isNotEqual = new ArithmeticOperator(OperatorType.IsNotEqual, (int) OperatorPrecedence.Equality, true);
        private static readonly Operator _closeTernary = new ClosingTernaryOperator();
        private static readonly Operator _or = new ArithmeticOperator(OperatorType.ConditionalOr, (int) OperatorPrecedence.ConditionalOr, true);
        private static readonly Operator _and = new ArithmeticOperator(OperatorType.ConditionalAnd, (int) OperatorPrecedence.ConditionalAnd, true);
        private static readonly Operator _greaterThan = new ArithmeticOperator(OperatorType.GreaterThan, (int) OperatorPrecedence.Relational, true);
        private static readonly Operator _greaterThanOrEqual = new ArithmeticOperator(OperatorType.GreaterThanOrEqual, (int) OperatorPrecedence.Relational, true);
        private static readonly Operator _lessThan = new ArithmeticOperator(OperatorType.LessThan, (int) OperatorPrecedence.Relational, true);
        private static readonly Operator _lessThanOrEqual = new ArithmeticOperator(OperatorType.LessThanOrEqual, (int) OperatorPrecedence.Relational, true);
        private static readonly Operator _leftBracket = new BracketOperator();
        private static readonly Operator _argument = new ArgumentOperator();

        /// <summary>
        /// Parses a unary operator at the current index. Moves that index if an operator has been found.
        /// </summary>
        /// <returns></returns>
        protected override bool ParseUnaryOperator()
        {
            char c = Input[Index];
            switch (c)
            {
                case '+':
                    PushOperator(_positive);
                    break;
                case '-':
                    PushOperator(_negative);
                    break;
                case '!':
                    PushOperator(_not);
                    break;
                case '(':
                    PushOperator(_leftBracket);
                    break;
                case ')':
                    if (Top is FunctionOperator)
                        PopAndExecuteOperator();
                    else
                        throw new ParserException("Invalid closing bracket", Input, Index);
                    ExpectInfixPostfix = true;
                    break;
                case var mc when mc >= 'a' && mc <= 'z' || mc >= 'A' && mc <= 'Z' || mc == '_':
                    var name = NextWord();

                    // Is it a function name?
                    if (NextChar(name.Length) == '(')
                    {
                        // We found a function definition
                        Index += name.Length + 1;
                        PushOperator(new FunctionOperator(name));
                        return true;
                    }
                    return false;
                default:
                    return false;
            }

            // All operators are only one character long
            Index++;
            return true;
        }

        /// <summary>
        /// Parses a value at the current index. Moves that index if a value has been found.
        /// </summary>
        /// <returns></returns>
        protected override bool ParseValue()
        {
            var c = Input[Index];

            // Parse a variable
            if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c == '_')
            {
                var name = NextWord();

                // Avoid parsing function names
                if (NextChar(name.Length) != '(')
                {
                    Index += name.Length;
                    PushVariable(name);
                    return true;
                }
            }

            // Nothing found (may be implemented by child classes)
            return false;
        }

        /// <summary>
        /// Push a variable to the stack with a specific name.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        protected abstract void PushVariable(string name);

        /// <summary>
        /// Parses a binary operator at the current index. Moves that index if an operator has been found.
        /// </summary>
        /// <returns></returns>
        protected override bool ParseBinaryOperator()
        {
            var c = Input[Index];
            switch (c)
            {
                // Single character operators
                case '+':
                    PushOperator(_add);
                    break;
                case '-':
                    PushOperator(_subtract);
                    break;
                case '*':
                    PushOperator(_multiply);
                    break;
                case '/':
                    PushOperator(_divide);
                    break;
                case '%':
                    PushOperator(_modulo);
                    break;
                case '?':
                    PushOperator(new TernaryOperator());
                    break;
                case ':':
                    // Push and pop the closing ternary operator
                    PushOperator(_closeTernary);
                    PopAndExecuteOperator();

                    // Find the top ternary operator and close it
                    if (!(Top is TernaryOperator))
                        throw new ParserException("Invalid ternary operator", Input, Index);
                    break;
                case '^':
                    PushOperator(_power);
                    break;

                // One/Two-character operators
                case '<':
                    if (NextChar() == '=')
                    {
                        Index++;
                        PushOperator(_lessThanOrEqual);
                    }
                    else
                        PushOperator(_lessThan);
                    break;

                case '>':
                    if (NextChar() == '=')
                    {
                        Index++;
                        PushOperator(_greaterThanOrEqual);
                    }
                    else
                        PushOperator(_greaterThan);
                    break;

                case '!':
                    if (NextChar() == '=')
                    {
                        Index++;
                        PushOperator(_isNotEqual);
                    }
                    else
                        return false;
                    break;

                case '=':
                    if (NextChar() == '=')
                    {
                        Index++;
                        PushOperator(_isEqual);
                    }
                    else
                        return false;
                    break;

                case '&':
                    if (NextChar() == '&')
                    {
                        Index++;
                        PushOperator(_and);
                    }
                    else
                        return false;
                    break;

                case '|':
                    if (NextChar() == '|')
                    {
                        Index++;
                        PushOperator(_or);
                    }
                    else
                        return false;
                    break;

                case ',':
                    // Add and remove an argument
                    PushOperator(_argument);

                    // Pop the argument
                    PopAndExecuteOperator();

                    if (Top is FunctionOperator fo)
                        fo.AddArgument();
                    break;

                case ')':
                    // Add and execute an argument
                    PushOperator(_argument);

                    // Pop the argument
                    PopAndExecuteOperator();

                    if (Top is FunctionOperator fo2)
                        fo2.AddArgument();
                    else if (!(Top is BracketOperator))
                        throw new ParserException("Bracket mismatch", Input, Index);
                    PopAndExecuteOperator();

                    // Tell the parser that we expect an infix or postfix operator next
                    ExpectInfixPostfix = true;
                    break;

                default:
                    return false;
            }

            // Consume the current operator character
            Index++;
            return true;
        }

        /// <summary>
        /// Acts on an operator as soon as it is encountered.
        /// </summary>
        /// <param name="op">The operator.</param>
        protected override void PrepareOperator(Operator op)
        {
        }

        /// <summary>
        /// Look ahead for the next character
        /// </summary>
        /// <returns>The character at the next position</returns>
        protected char NextChar(int offset = 1)
        {
            if (Index + offset < Count)
                return Input[Index + offset];
            return '\0';
        }

        /// <summary>
        /// Looks ahead for a word. Assumes the first character has already been matched!
        /// </summary>
        /// <returns></returns>
        protected string NextWord()
        {
            var index = Index;
            var sb = new StringBuilder(32);
            while (Input[index] >= '0' && Input[index] <= '9' ||
                   Input[index] >= 'a' && Input[index] <= 'z' ||
                   Input[index] >= 'A' && Input[index] <= 'Z' ||
                   Input[index] == '_')
            {
                sb.Append(Input[index]);
                index++;
                if (index >= Count)
                    return sb.ToString();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Looks ahead until a specific character. Assumes the first character has already been matched!
        /// </summary>
        /// <param name="end">The character before we need to end.</param>
        /// <returns></returns>
        protected string NextUntil(char end)
        {
            var index = Index;
            var sb = new StringBuilder(32);
            while (Input[index] != end)
            {
                sb.Append(Input[index]);
                index++;
                if (index >= Count)
                    return sb.ToString();
            }
            return sb.ToString();
        }
    }
}
