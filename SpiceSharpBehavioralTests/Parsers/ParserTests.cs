using NUnit.Framework;
using SpiceSharpBehavioral.Parsers;
using System.Collections.Generic;

namespace SpiceSharpBehavioralTests.Parsers
{
    [TestFixture]
    public class ParserTests
    {
        [TestCaseSource(typeof(ParserTestData), nameof(ParserTestData.Expressions))]
        public void When_Expression_Expect_Reference(string input, double expected)
        {
            var parser = new Parser<double>(new DoubleBuilder());
            Assert.AreEqual(expected, parser.Parse(input), 1e-20);
        }

        public class ParserTestData
        {
            public static IEnumerable<TestCaseData> Expressions
            {
                get
                {
                    yield return new TestCaseData("1+1", 2.0); // Simple addition
                    yield return new TestCaseData("5 % 2", 1.0); // Modulo with spaces
                    yield return new TestCaseData("2*3+4*5", 26.0); // Multiplication precedence
                    yield return new TestCaseData("4*5/3", 20.0 / 3.0); // Chaining multiplications
                    yield return new TestCaseData("5+--3", 8.0); // Right-associative unary operator
                    yield return new TestCaseData("3^2", 9.0); // Power
                    yield return new TestCaseData("2*(3+4)*5", 70.0); // Parenthesis
                    yield return new TestCaseData("abs(-2*6+7)", 5.0); // Function
                    yield return new TestCaseData("min(-2,6*2)", -2.0); // Function with multiple arguments
                }
            }
        }
    }
}
