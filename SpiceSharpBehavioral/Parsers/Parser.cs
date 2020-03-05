using SpiceSharp;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A parser that parses Spice expressions.
    /// </summary>
    /// <remarks>
    /// This is a recursive descent parser.
    /// </remarks>
    /// <typeparam name="T">The value type.</typeparam>
    public class Parser<T>
    {
        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>
        /// The builder.
        /// </value>
        public IBuilder<T> Builder { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser{T}"/> class.
        /// </summary>
        /// <param name="builder">The builder for building the result.</param>
        public Parser(IBuilder<T> builder)
        {
            Builder = builder.ThrowIfNull(nameof(builder));
        }

        /// <summary>
        /// Parses the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The value of the parsed expression.</returns>
        public T Parse(string expression)
        {
            return Parse(new Lexer(expression));
        }

        /// <summary>
        /// Parses the expression using a lexer.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns>The value of the lexed expression.</returns>
        /// <exception cref="Exception">Invalid expression</exception>
        public T Parse(ILexer lexer)
        {
            lexer.ReadToken();
            var result = ParseConditional(lexer);
            if (lexer.Token != TokenType.EndOfExpression)
                throw new Exception("Invalid expression");
            return result;
        }

        private T ParseConditional(ILexer lexer)
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
                result = Builder.Conditional(result, ifTrue, ifFalse);
            }
            return result;
        }
        private T ParseConditionalOr(ILexer lexer)
        {
            var result = ParseConditionalAnd(lexer);
            while (lexer.Token == TokenType.Or)
            {
                lexer.ReadToken();
                var right = ParseConditionalAnd(lexer);
                result = Builder.Or(result, right);
            }
            return result;
        }
        private T ParseConditionalAnd(ILexer lexer)
        {
            var result = ParseEquality(lexer);
            while (lexer.Token == TokenType.And)
            {
                lexer.ReadToken();
                var right = ParseEquality(lexer);
                result = Builder.And(result, right);
            }
            return result;
        }
        private T ParseEquality(ILexer lexer)
        {
            var result = ParseRelational(lexer);
            while (lexer.Token == TokenType.Equals || lexer.Token == TokenType.NotEquals)
            {
                T right;
                switch (lexer.Token)
                {
                    case TokenType.Equals:
                        lexer.ReadToken();
                        right = ParseRelational(lexer);
                        result = Builder.Equals(result, right);
                        break;
                    case TokenType.NotEquals:
                        lexer.ReadToken();
                        right = ParseRelational(lexer);
                        result = Builder.NotEquals(result, right);
                        break;
                }
            }
            return result;
        }
        private T ParseRelational(ILexer lexer)
        {
            var result = ParseAdditive(lexer);
            while (true)
            {
                T right;
                switch (lexer.Token)
                {
                    case TokenType.LessThan:
                        lexer.ReadToken();
                        right = ParseAdditive(lexer);
                        result = Builder.LessThan(result, right);
                        break;
                    case TokenType.GreaterThan:
                        lexer.ReadToken();
                        right = ParseAdditive(lexer);
                        result = Builder.GreaterThan(result, right);
                        break;
                    case TokenType.LessEqual:
                        lexer.ReadToken();
                        right = ParseAdditive(lexer);
                        result = Builder.LessThanOrEqual(result, right);
                        break;
                    case TokenType.GreaterEqual:
                        lexer.ReadToken();
                        right = ParseAdditive(lexer);
                        result = Builder.GreaterThanOrEqual(result, right);
                        break;
                    default:
                        return result;
                }
            }
        }
        private T ParseAdditive(ILexer lexer)
        {
            var result = ParseMultiplicative(lexer);
            while (true)
            {
                T right;
                switch (lexer.Token)
                {
                    case TokenType.Plus:
                        lexer.ReadToken();
                        right = ParseMultiplicative(lexer);
                        result = Builder.Add(result, right);
                        break;
                    case TokenType.Minus:
                        lexer.ReadToken();
                        right = ParseMultiplicative(lexer);
                        result = Builder.Subtract(result, right);
                        break;
                    default:
                        return result;
                }
            }
        }
        private T ParseMultiplicative(ILexer lexer)
        {
            var result = ParseUnary(lexer);
            while (true)
            {
                T right;
                switch (lexer.Token)
                {
                    case TokenType.Times:
                        lexer.ReadToken();
                        right = ParseUnary(lexer);
                        result = Builder.Multiply(result, right);
                        break;
                    case TokenType.Divide:
                        lexer.ReadToken();
                        right = ParseUnary(lexer);
                        result = Builder.Divide(result, right);
                        break;
                    case TokenType.Mod:
                        lexer.ReadToken();
                        right = ParseUnary(lexer);
                        result = Builder.Mod(result, right);
                        break;
                    default:
                        return result;
                }
            }
        }
        private T ParseUnary(ILexer lexer)
        {
            T argument;
            switch (lexer.Token)
            {
                case TokenType.Plus:
                    lexer.ReadToken();
                    argument = ParseUnary(lexer);
                    return Builder.Plus(argument);

                case TokenType.Minus:
                    lexer.ReadToken();
                    argument = ParseUnary(lexer);
                    return Builder.Minus(argument);

                default:
                    return ParsePower(lexer);
            }
        }
        private T ParsePower(ILexer lexer)
        {
            var result = ParseTerminal(lexer);
            if (lexer.Token == TokenType.Power)
            {
                lexer.ReadToken();
                var right = ParsePower(lexer);
                result = Builder.Pow(result, right);
            }
            return result;
        }
        private T ParseTerminal(ILexer lexer)
        {
            T result;
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
                    result = Builder.CreateNumber(lexer.Content);
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
                                return Builder.CreateVoltage(a, b, type);

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
                                result = Builder.CreateCurrent(a, type);
                                break;

                            default:
                                List<T> arguments = new List<T>(2);
                                while (lexer.Token != TokenType.RightParenthesis)
                                {
                                    lexer.ReadToken();
                                    arguments.Add(ParseConditional(lexer));
                                    if (lexer.Token != TokenType.Comma && lexer.Token != TokenType.RightParenthesis)
                                        throw new Exception("Invalid function call");
                                }
                                result = Builder.CreateFunction(name, arguments);
                                lexer.ReadToken();
                                break;
                        }
                    }
                    else
                    {
                        result = Builder.CreateVariable(lexer.Content);
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
                    result = Builder.CreateProperty(name, lexer.Content, QuantityTypes.Raw);
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
