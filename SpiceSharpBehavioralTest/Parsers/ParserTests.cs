using NUnit.Framework;
using SpiceSharpBehavioral.Parsers;
using System.Collections.Generic;
using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpBehavioralTest.Parsers
{
    [TestFixture]
    public class ParserTests
    {
        [TestCaseSource(nameof(SpiceNumbers))]
        [TestCaseSource(nameof(Expressions))]
        public void When_FasterExpression_Expect_Reference(string input, Node expected)
        {
            var lexer = Lexer.FromString(input);
            var actual = Parser.Parse(lexer);
            Assert.AreEqual(expected, actual);
        }

        public static IEnumerable<TestCaseData> SpiceNumbers
        {
            get
            {
                // Numbers
                yield return new TestCaseData("2e3", Node.Constant(2e3)).SetName("{m}(2e3)"); // Exponential notation
                yield return new TestCaseData("2e-2", Node.Constant(2e-2)).SetName("{m}(2e-2)"); // Exponential notation 2
                yield return new TestCaseData("2.5g", Node.Constant(2.5e9)).SetName("{m}(2.5g)"); // Giga
                yield return new TestCaseData("3.6x", Node.Constant(3.6e6)).SetName("{m}(3.6x)"); // Mega 1
                yield return new TestCaseData("2.6meg", Node.Constant(2.6e6)).SetName("{m}(2.6meg)"); // Mega 2
                yield return new TestCaseData("8.3k", Node.Constant(8.3e3)).SetName("{m}(8.3k)"); // Kilo
                yield return new TestCaseData("2.9m", Node.Constant(2.9e-3)).SetName("{m}(2.9m)"); // Milli
                yield return new TestCaseData("1u", Node.Constant(1e-6)).SetName("{m}(1u)"); // Micro
                yield return new TestCaseData("0.2n", Node.Constant(0.2e-9)).SetName("{m}(0.2n)"); // Nano
                yield return new TestCaseData("2.22p", Node.Constant(2.22E-12)).SetName("{m}(2.22p)"); // Pico - the rounding-off error is due to the multiplication 2.22 * 1e-12.
                yield return new TestCaseData("3.78f", Node.Constant(3.78e-15)).SetName("{m}(3.78f)"); // Femto
                yield return new TestCaseData("3mil", Node.Constant(3 * 25.4e-6)).SetName("{m}(3mil)"); // Mil
                yield return new TestCaseData(".5", Node.Constant(0.5)).SetName("{m}(.5)"); // No leading zero
            }
        }
        public static IEnumerable<TestCaseData> Expressions
        {
            get
            {
                yield return new TestCaseData("1+1", Node.Add(1.0, 1.0)).SetName("{m}(1 + 1)"); // Simple addition
                yield return new TestCaseData("5 % 2", Node.Modulo(5.0, 2.0)).SetName("{m}(5 % 2)"); // Modulo with spaces
                yield return new TestCaseData("2*3+4*5", Node.Add(Node.Multiply(2.0, 3.0), Node.Multiply(4.0, 5.0))).SetName("{m}(2 * 3 + 4 * 5)"); // Multiplication precedence
                yield return new TestCaseData("4*5/3", Node.Divide(Node.Multiply(4.0, 5.0), 3.0)).SetName("{m}(4 * 5 / 3)"); // Chaining multiplications
                yield return new TestCaseData("5+--3", Node.Add(5.0, Node.Minus(Node.Minus(3.0)))).SetName("{m}(5 + --3)"); // Right-associative unary operator
                yield return new TestCaseData("3^2", Node.Power(3.0, 2.0)).SetName("{m}(3^2)"); // Power
                yield return new TestCaseData("2*(3+4)*5", Node.Multiply(Node.Multiply(2.0, Node.Add(3.0, 4.0)), 5.0)).SetName("{m}(\"2 * (3+4) * 5\")"); // Parenthesis
                yield return new TestCaseData("abs(-2*6+7)", Node.Function("abs", Node.Add(Node.Multiply(Node.Minus(2.0), 6.0), 7.0))).SetName("{m}(\"abs(-2*6+7)\")"); // Function
                yield return new TestCaseData("min(-2,6*2)", Node.Function("min", Node.Minus(2.0), Node.Multiply(6.0, 2.0))).SetName("{m}(\"min(-2,6*2)\")"); // Function with multiple arguments
                yield return new TestCaseData("rnd()", Node.Function("rnd")).SetName("{m}(\"rnd()\")"); // Function without arguments
                yield return new TestCaseData("-.14e3", Node.Minus(Node.Constant(0.14e3))).SetName("{m}(-.14e3)");
                yield return new TestCaseData("V(2,0)", Node.Voltage("2", "0"));
            }
        }
    }
}
