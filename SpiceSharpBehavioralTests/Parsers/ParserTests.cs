using NUnit.Framework;
using SpiceSharpBehavioral.Parsers;
using System.Collections.Generic;
using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpBehavioralTests.Parsers
{
    [TestFixture]
    public class ParserTests
    {
        [TestCaseSource(typeof(ParserTestData), nameof(ParserTestData.Expressions))]
        public void When_Expression_Expect_Reference(string input, Node expected)
        {
            var parser = new Parser();
            var actual = parser.Parse(input);
            Assert.AreEqual(expected, actual);
        }

        public class ParserTestData
        {
            public static IEnumerable<TestCaseData> Expressions
            {
                get
                {
                    yield return new TestCaseData("1+1", Node.Add(Node.Constant("1"), Node.Constant("1"))); // Simple addition
                    yield return new TestCaseData("5 % 2", Node.Modulo(Node.Constant("5"), Node.Constant("2"))); // Modulo with spaces
                    yield return new TestCaseData("2*3+4*5", Node.Add(Node.Multiply(Node.Constant("2"), Node.Constant("3")), Node.Multiply(Node.Constant("4"), Node.Constant("5")))); // Multiplication precedence
                    yield return new TestCaseData("4*5/3", Node.Divide(Node.Multiply(Node.Constant("4"), Node.Constant("5")), Node.Constant("3"))); // Chaining multiplications
                    yield return new TestCaseData("5+--3", Node.Add(Node.Constant("5"), Node.Minus(Node.Minus(Node.Constant("3"))))); // Right-associative unary operator
                    yield return new TestCaseData("3^2", Node.Power(Node.Constant("3"), Node.Constant("2"))); // Power
                    yield return new TestCaseData("2*(3+4)*5", Node.Multiply(Node.Multiply(Node.Constant("2"), Node.Add(Node.Constant("3"), Node.Constant("4"))), Node.Constant("5"))); // Parenthesis
                    yield return new TestCaseData("abs(-2*6+7)", Node.Function("abs", new[] { Node.Add(Node.Multiply(Node.Minus(Node.Constant("2")), Node.Constant("6")), Node.Constant("7")) })); // Function
                    yield return new TestCaseData("min(-2,6*2)", Node.Function("min", new[] { Node.Minus(Node.Constant("2")), Node.Multiply(Node.Constant("6"), Node.Constant("2")) })); // Function with multiple arguments
                    yield return new TestCaseData("rnd()", Node.Function("rnd", new Node[0])); // Function without arguments
                }
            }
        }
    }
}
