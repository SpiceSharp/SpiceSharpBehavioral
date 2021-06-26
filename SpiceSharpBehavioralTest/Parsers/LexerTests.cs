using NUnit.Framework;
using SpiceSharpBehavioral.Parsers;
using System.Collections.Generic;

namespace SpiceSharpBehavioralTest.Parsers
{
    [TestFixture]
    public class LexerTests
    {
        [TestCaseSource(typeof(LexerTestData), nameof(LexerTestData.Numbers))]
        public void When_Number_Expect_Token(string input)
        {
            var lexer = Lexer.FromString(input);
            Assert.AreEqual(TokenType.Number, lexer.Type);
            Assert.AreEqual(input, lexer.Content);
        }

        [TestCaseSource(typeof(LexerTestData), nameof(LexerTestData.Operators))]
        public void When_Operator_Expect_Token(string input, TokenType expected)
        {
            var lexer = Lexer.FromString(input);
            Assert.AreEqual(expected, lexer.Type);
        }

        [TestCaseSource(typeof(LexerTestData), nameof(LexerTestData.Sequences))]
        public void When_Sequence_Expect_Tokens(string input, TokenType[] expected)
        {
            var lexer = Lexer.FromString(input);
            foreach (var token in expected)
            {
                Assert.AreEqual(token, lexer.Type);
                lexer.Next();
            }
        }

        [Test]
        public void When_Nodes_Expect_Identifiers()
        {
            var lexer = Lexer.FromString("V(a_2.8, 2&#@;k");
            Assert.AreEqual(TokenType.Identifier, lexer.Type);
            lexer.Next();
            Assert.AreEqual(TokenType.LeftParenthesis, lexer.Type);
            lexer.Next(); lexer.ContinueWhileNode();
            Assert.AreEqual("a_2.8", lexer.Content);
            lexer.Next();
            Assert.AreEqual(TokenType.Comma, lexer.Type);
            lexer.Next(); lexer.ContinueWhileNode();
            Assert.AreEqual(@"2&#@;k", lexer.Content);
            lexer.Next();
            Assert.AreEqual(TokenType.EndOfExpression, lexer.Type);
        }
    }

    public class LexerTestData
    {
        public static IEnumerable<TestCaseData> Numbers
        {
            get
            {
                yield return new TestCaseData("5");
                yield return new TestCaseData("4.");
                yield return new TestCaseData("3.5");
                yield return new TestCaseData("2e");
                yield return new TestCaseData("3fF");
                yield return new TestCaseData("2.5e2");
                yield return new TestCaseData("8.9e-8");
                yield return new TestCaseData("1.");
                yield return new TestCaseData("2.e2A");
                yield return new TestCaseData("2e3");
                yield return new TestCaseData(".2");
            }
        }
        public static IEnumerable<TestCaseData> Operators
        {
            get
            {
                yield return new TestCaseData("+", TokenType.Plus);
                yield return new TestCaseData("+", TokenType.Plus);
                yield return new TestCaseData("-", TokenType.Minus);
                yield return new TestCaseData("*", TokenType.Times);
                yield return new TestCaseData("/", TokenType.Divide);
                yield return new TestCaseData(".", TokenType.Dot);
                yield return new TestCaseData("%", TokenType.Mod);
                yield return new TestCaseData("&&", TokenType.And);
                yield return new TestCaseData("||", TokenType.Or);
                yield return new TestCaseData("=", TokenType.Assign);
                yield return new TestCaseData("==", TokenType.Equals);
                yield return new TestCaseData("!=", TokenType.NotEquals);
                yield return new TestCaseData("<", TokenType.LessThan);
                yield return new TestCaseData(">", TokenType.GreaterThan);
                yield return new TestCaseData("<=", TokenType.LessEqual);
                yield return new TestCaseData(">=", TokenType.GreaterEqual);
                yield return new TestCaseData("!", TokenType.Bang);
                yield return new TestCaseData("?", TokenType.Huh);
                yield return new TestCaseData(":", TokenType.Colon);
                yield return new TestCaseData(",", TokenType.Comma);
                yield return new TestCaseData("^", TokenType.Power);
                yield return new TestCaseData("", TokenType.EndOfExpression);
                yield return new TestCaseData("(", TokenType.LeftParenthesis);
                yield return new TestCaseData(")", TokenType.RightParenthesis);
                yield return new TestCaseData("[", TokenType.LeftIndex);
                yield return new TestCaseData("]", TokenType.RightIndex);
                yield return new TestCaseData("@", TokenType.At);
            }
        }
        public static IEnumerable<TestCaseData> Sequences
        {
            get
            {
                yield return new TestCaseData(
                    "5+a > 2 ? 2u*3k : (-8 / 5)", 
                    new[] { TokenType.Number, TokenType.Plus, TokenType.Identifier, TokenType.GreaterThan, TokenType.Number,
                        TokenType.Huh, TokenType.Number, TokenType.Times, TokenType.Number,
                        TokenType.Colon, TokenType.LeftParenthesis, TokenType.Minus, TokenType.Number, TokenType.Divide, TokenType.Number, TokenType.RightParenthesis,
                        TokenType.EndOfExpression });
            }
        }
    }
}
