using NUnit.Framework;
using SpiceSharpBehavioral.Parsers;
using System.Collections.Generic;
using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpBehavioralTests.Parsers
{
    [TestFixture]
    public class ParserTests
    {
        [TestCaseSource(nameof(SpiceNumbers))]
        [TestCaseSource(nameof(Expressions))]
        public void When_Expression_Expect_Reference(string input, Node expected)
        {
            var parser = new Parser();
            var actual = parser.Parse(input);
            Assert.AreEqual(expected, actual);
        }

        public static IEnumerable<TestCaseData> SpiceNumbers
        {
            get
            {
                // Numbers
                yield return new TestCaseData("2e3", Node.Constant(2e3)); // Exponential notation
                yield return new TestCaseData("2e-2", Node.Constant(2e-2)); // Exponential notation 2
                yield return new TestCaseData("2.5g", Node.Constant(2.5e9)); // Giga
                yield return new TestCaseData("3.6x", Node.Constant(3.6e6)); // Mega 1
                yield return new TestCaseData("2.6meg", Node.Constant(2.6e6)); // Mega 2
                yield return new TestCaseData("8.3k", Node.Constant(8.3e3)); // Kilo
                yield return new TestCaseData("2.9m", Node.Constant(2.9e-3)); // Milli
                yield return new TestCaseData("1u", Node.Constant(1e-6)); // Micro
                yield return new TestCaseData("0.2n", Node.Constant(0.2e-9)); // Nano
                yield return new TestCaseData("2.22p", Node.Constant(2.22E-12)); // Pico - the rounding-off error is due to the multiplication 2.22 * 1e-12.
                yield return new TestCaseData("3.78f", Node.Constant(3.78e-15)); // Femto
                yield return new TestCaseData("3mil", Node.Constant(3 * 25.4e-6)); // Mil
            }
        }
        public static IEnumerable<TestCaseData> Expressions
        {
            get
            {
                yield return new TestCaseData("1+1", Node.Add(1.0, 1.0)); // Simple addition
                yield return new TestCaseData("5 % 2", Node.Modulo(5.0, 2.0)); // Modulo with spaces
                yield return new TestCaseData("2*3+4*5", Node.Add(Node.Multiply(2.0, 3.0), Node.Multiply(4.0, 5.0))); // Multiplication precedence
                yield return new TestCaseData("4*5/3", Node.Divide(Node.Multiply(4.0, 5.0), 3.0)); // Chaining multiplications
                yield return new TestCaseData("5+--3", Node.Add(5.0, Node.Minus(Node.Minus(3.0)))); // Right-associative unary operator
                yield return new TestCaseData("3^2", Node.Power(3.0, 2.0)); // Power
                yield return new TestCaseData("2*(3+4)*5", Node.Multiply(Node.Multiply(2.0, Node.Add(3.0, 4.0)), 5.0)); // Parenthesis
                yield return new TestCaseData("abs(-2*6+7)", Node.Function("abs", Node.Add(Node.Multiply(Node.Minus(2.0), 6.0), 7.0))); // Function
                yield return new TestCaseData("min(-2,6*2)", Node.Function("min", Node.Minus(2.0), Node.Multiply(6.0, 2.0))); // Function with multiple arguments
                yield return new TestCaseData("rnd()", Node.Function("rnd")); // Function without arguments
            }
        }
    }
}
