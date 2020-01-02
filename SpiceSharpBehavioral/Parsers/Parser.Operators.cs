using SpiceSharp;
using System;

namespace SpiceSharpBehavioral.Parsers
{
    public partial class Parser<T>
    {
        private class FunctionDeclaration
        {
            public string Name { get; }
            public int Arguments { get; set; }
            public FunctionDeclaration(string name)
            {
                Name = name.ThrowIfNull(nameof(name));
            }
        }

        /// <summary>
        /// A list of all possible operators in our parser.
        /// </summary>
        [Flags]
        private enum Operators
        {
            None = 0x00,

            // Associativity
            LeftAssociative = 0x001000,
            Associativity = 0x001000,

            // Precedence
            PrecedenceNone = 0x000000,
            PrecedenceConditional = 0x010000,
            PrecedenceConditionalOr = 0x020000,
            PrecedenceConditionalAnd = 0x030000,
            PrecedenceLogicalOr = 0x040000,
            PrecedenceLogicalXor = 0x050000,
            PrecedenceLogicalAnd = 0x060000,
            PrecedenceEquality = 0x070000,
            PrecedenceRelational = 0x080000,
            PrecedenceShift = 0x090000,
            PrecedenceAdditive = 0x0a0000,
            PrecedenceMultiplicative = 0x0b0000,
            PrecedenceUnary = 0x0c0000,
            PrecedencePrimary = 0x0d0000,
            PrecedencePower = 0x0e0000,
            PrecedenceSpecial = 0x0f0000,
            Precedence = 0x0ff0000,

            Positive = PrecedenceUnary | 0x01,
            Negative = PrecedenceUnary | 0x02,
            Addition = PrecedenceAdditive | LeftAssociative | 0x03,
            Subtraction = PrecedenceAdditive | LeftAssociative | 0x04,
            Multiplication = PrecedenceMultiplicative | LeftAssociative | 0x05,
            Division = PrecedenceMultiplicative | LeftAssociative | 0x06,
            Power = PrecedencePower | 0x07,
            Modulo = PrecedenceMultiplicative | 0x08,
            Equal = PrecedenceRelational | LeftAssociative | 0x09,
            NotEqual = PrecedenceRelational | LeftAssociative | 0x0a,
            Greater = PrecedenceRelational | LeftAssociative | 0x0b,
            Less = PrecedenceRelational | LeftAssociative | 0x0c,
            GreaterOrEqual = PrecedenceRelational | LeftAssociative | 0x0d,
            LessOrEqual = PrecedenceRelational | LeftAssociative | 0x0e,
            Not = PrecedenceUnary | 0x0f,
            And = PrecedenceConditionalAnd | 0x010,
            Or = PrecedenceConditionalOr | LeftAssociative | 0x020,
            Ternary = PrecedenceNone | 0x030,
            TernarySeparator = PrecedenceNone | 0x040,
            LeftBracket = PrecedenceNone | 0x050,
            RightBracket = PrecedenceSpecial | 0x060,
            Function = PrecedenceNone | 0x060,
            Argument = PrecedenceNone | 0x070
        }

        /// <summary>
        /// Determines which operator has precedence.
        /// </summary>
        /// <param name="first">The first operator.</param>
        /// <param name="second">The second operator.</param>
        /// <returns>
        ///   <c>true</c> if the first operator has precedence; otherwise, <c>false</c>.
        /// </returns>
        private bool HasPrecedence(Operators first, Operators second)
        {
            // Brackets have special precedence rules
            if (second == Operators.RightBracket)
            {
                if (first == Operators.LeftBracket)
                    return false;
                if (first == Operators.Function)
                    return false;
                return true;
            }

            // Ternary operators have special precedence rules
            if (second == Operators.TernarySeparator)
                return first != Operators.Ternary;

            // Function arguments have special precedence rules
            if (second == Operators.Argument)
                return first != Operators.Function;

            // Regular arithmetic precedence
            var fp = first & Operators.Precedence;
            var sp = second & Operators.Precedence;
            if (fp > sp)
                return true;
            if (fp == sp && (second & Operators.Associativity) != 0)
                return true;
            return false;
        }
    }
}
