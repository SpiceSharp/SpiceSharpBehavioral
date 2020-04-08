using System;
using System.Collections.Generic;
using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A parser that parses Spice expressions.
    /// </summary>
    /// <remarks>
    /// This is a recursive descent parser.
    /// </remarks>
    public class Parser
    {
        /// <summary>
        /// Parses the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The value of the parsed expression.</returns>
        public Node Parse(string expression) => Parse(new Lexer(expression));

        /// <summary>
        /// Parses the expression using a lexer.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns>The value of the lexed expression.</returns>
        /// <exception cref="Exception">Invalid expression</exception>
        public Node Parse(ILexer lexer)
        {
            lexer.ReadToken();
            var result = ParseConditional(lexer);
            if (lexer.Token != TokenType.EndOfExpression)
                throw new Exception("Invalid expression");
            return result;
        }

        private Node ParseConditional(ILexer lexer)
        {
            var result = ParseConditionalOr(lexer);
            while (lexer.Token == TokenType.Huh)
            {
                lexer.ReadToken();
                var ifTrue = ParseConditional(lexer);
                if (lexer.Token != TokenType.Colon)
                    throw new Exception("Invalid conditional");
                lexer.ReadToken();
                var ifFalse = ParseConditional(lexer);
                result = Node.Conditional(result, ifTrue, ifFalse);
            }
            return result;
        }
        private Node ParseConditionalOr(ILexer lexer)
        {
            var result = ParseConditionalAnd(lexer);
            while (lexer.Token == TokenType.Or)
            {
                lexer.ReadToken();
                var right = ParseConditionalAnd(lexer);
                result = Node.Or(result, right);
            }
            return result;
        }
        private Node ParseConditionalAnd(ILexer lexer)
        {
            var result = ParseEquality(lexer);
            while (lexer.Token == TokenType.And)
            {
                lexer.ReadToken();
                var right = ParseEquality(lexer);
                result = Node.And(result, right);
            }
            return result;
        }
        private Node ParseEquality(ILexer lexer)
        {
            var result = ParseRelational(lexer);
            while (lexer.Token == TokenType.Equals || lexer.Token == TokenType.NotEquals)
            {
                Node right;
                switch (lexer.Token)
                {
                    case TokenType.Equals:
                        lexer.ReadToken();
                        right = ParseRelational(lexer);
                        result = Node.Equals(result, right);
                        break;
                    case TokenType.NotEquals:
                        lexer.ReadToken();
                        right = ParseRelational(lexer);
                        result = Node.NotEquals(result, right);
                        break;
                }
            }
            return result;
        }
        private Node ParseRelational(ILexer lexer)
        {
            var result = ParseAdditive(lexer);
            while (true)
            {
                Node right;
                switch (lexer.Token)
                {
                    case TokenType.LessThan:
                        lexer.ReadToken();
                        right = ParseAdditive(lexer);
                        result = Node.LessThan(result, right);
                        break;
                    case TokenType.GreaterThan:
                        lexer.ReadToken();
                        right = ParseAdditive(lexer);
                        result = Node.GreaterThan(result, right);
                        break;
                    case TokenType.LessEqual:
                        lexer.ReadToken();
                        right = ParseAdditive(lexer);
                        result = Node.LessThanOrEqual(result, right);
                        break;
                    case TokenType.GreaterEqual:
                        lexer.ReadToken();
                        right = ParseAdditive(lexer);
                        result = Node.GreaterThanOrEqual(result, right);
                        break;
                    default:
                        return result;
                }
            }
        }
        private Node ParseAdditive(ILexer lexer)
        {
            var result = ParseMultiplicative(lexer);
            while (true)
            {
                Node right;
                switch (lexer.Token)
                {
                    case TokenType.Plus:
                        lexer.ReadToken();
                        right = ParseMultiplicative(lexer);
                        result = Node.Add(result, right);
                        break;
                    case TokenType.Minus:
                        lexer.ReadToken();
                        right = ParseMultiplicative(lexer);
                        result = Node.Subtract(result, right);
                        break;
                    default:
                        return result;
                }
            }
        }
        private Node ParseMultiplicative(ILexer lexer)
        {
            var result = ParseUnary(lexer);
            while (true)
            {
                Node right;
                switch (lexer.Token)
                {
                    case TokenType.Times:
                        lexer.ReadToken();
                        right = ParseUnary(lexer);
                        result = Node.Multiply(result, right);
                        break;
                    case TokenType.Divide:
                        lexer.ReadToken();
                        right = ParseUnary(lexer);
                        result = Node.Divide(result, right);
                        break;
                    case TokenType.Mod:
                        lexer.ReadToken();
                        right = ParseUnary(lexer);
                        result = Node.Modulo(result, right);
                        break;
                    default:
                        return result;
                }
            }
        }
        private Node ParseUnary(ILexer lexer)
        {
            Node argument;
            switch (lexer.Token)
            {
                case TokenType.Plus:
                    lexer.ReadToken();
                    argument = ParseUnary(lexer);
                    return Node.Plus(argument);

                case TokenType.Minus:
                    lexer.ReadToken();
                    argument = ParseUnary(lexer);
                    return Node.Minus(argument);

                default:
                    return ParsePower(lexer);
            }
        }
        private Node ParsePower(ILexer lexer)
        {
            var result = ParseTerminal(lexer);
            if (lexer.Token == TokenType.Power)
            {
                lexer.ReadToken();
                var right = ParsePower(lexer);
                result = Node.Power(result, right);
            }
            return result;
        }
        private Node ParseTerminal(ILexer lexer)
        {
            Node result;
            switch (lexer.Token)
            {
                // Nested
                case TokenType.LeftParenthesis:
                    lexer.ReadToken();
                    result = ParseConditional(lexer);
                    if (lexer.Token != TokenType.RightParenthesis)
                        throw new Exception("Unclosed parenthesis");
                    lexer.ReadToken();
                    break;

                // A number
                case TokenType.Number:
                    result = Node.Constant(lexer.Content);
                    lexer.ReadToken();
                    break;

                // Can be a variable or a function call
                case TokenType.Identifier:
                    string name = lexer.Content;
                    lexer.ReadToken();
                    if (lexer.Token == TokenType.LeftParenthesis)
                    {
                        string a, b = null;
                        QuantityTypes type = QuantityTypes.Raw;
                        switch (name)
                        {
                            case "vr":
                                type = QuantityTypes.Real; goto case "v";
                            case "vi":
                                type = QuantityTypes.Imaginary; goto case "v";
                            case "vm":
                                type = QuantityTypes.Magnitude; goto case "v";
                            case "vdb":
                                type = QuantityTypes.Decibels; goto case "v";
                            case "vph":
                                type = QuantityTypes.Phase; goto case "v";
                            case "v":
                                // Read the nodes
                                lexer.ReadNode();
                                a = lexer.Content;
                                lexer.ReadToken();
                                if (lexer.Token == TokenType.Comma)
                                {
                                    lexer.ReadNode();
                                    b = lexer.Content;
                                    lexer.ReadToken();
                                }
                                if (lexer.Token != TokenType.RightParenthesis)
                                    throw new Exception("Invalid voltage specifier");
                                lexer.ReadToken();
                                result = Node.Voltage(a, b, type);
                                break;

                            case "ir":
                                type = QuantityTypes.Real; goto case "i";
                            case "ii":
                                type = QuantityTypes.Imaginary; goto case "i";
                            case "im":
                                type = QuantityTypes.Magnitude; goto case "i";
                            case "idb":
                                type = QuantityTypes.Decibels; goto case "i";
                            case "iph":
                                type = QuantityTypes.Phase; goto case "i";
                            case "i":
                                // Read the name
                                lexer.ReadNode();
                                a = lexer.Content;
                                lexer.ReadToken();
                                if (lexer.Token != TokenType.RightParenthesis)
                                    throw new Exception("Invalid current specifier");
                                lexer.ReadToken();
                                result = Node.Current(a, type);
                                break;

                            default:
                                var arguments = new List<Node>(2);

                                // Let's go to the first token after the function call
                                lexer.ReadToken();
                                while (lexer.Token != TokenType.RightParenthesis)
                                {
                                    arguments.Add(ParseConditional(lexer));
                                    if (lexer.Token == TokenType.Comma)
                                        lexer.ReadToken();
                                    else if (lexer.Token != TokenType.RightParenthesis)
                                        throw new Exception("Invalid function call");
                                }
                                result = Node.Function(name, arguments);
                                lexer.ReadToken();
                                break;
                        }
                    }
                    else
                    {
                        result = Node.Variable(lexer.Content);
                        lexer.ReadToken();
                    }
                    break;

                case TokenType.At:
                    lexer.ReadNode();
                    name = lexer.Content;
                    lexer.ReadToken();
                    if (lexer.Token != TokenType.LeftIndex)
                        throw new Exception("Invalid property identifier");
                    lexer.ReadNode();
                    result = Node.Property(name, lexer.Content, QuantityTypes.Raw);
                    lexer.ReadToken();
                    if (lexer.Token != TokenType.RightIndex)
                        throw new Exception("Invalid property identifier");
                    lexer.ReadToken();
                    break;

                // There is no level below this, so let's throw an exception
                default:
                    throw new Exception("Invalid value");
            }
            return result;
        }
    }
}
