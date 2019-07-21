using System;
using NUnit.Framework;
using SpiceSharpBehavioral.Parsers;

namespace SpiceSharpBehavioralTests.Parsers
{
    [TestFixture]
    public class SimpleDerivativeTests
    {
        protected double RelativeTolerance = 1e-9;
        protected double AbsoluteTolerance = 1e-12;

        protected void Check(double[] expected, Derivatives<double> actual)
        {
            for (var i = 0; i < expected.Length; i++)
            {
                var tol = Math.Max(Math.Abs(expected[i]), Math.Abs(actual[i])) * RelativeTolerance + AbsoluteTolerance;
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        protected void Check(double expected, Derivatives<double> actual)
        {
            Assert.AreEqual(actual.Count, 1);
            var tol = Math.Max(Math.Abs(expected), Math.Abs(actual[0])) * RelativeTolerance + AbsoluteTolerance;
            Assert.AreEqual(expected, actual[0], tol);
        }

        [Test]
        public void When_SimpleArithmetic_Expect_Reference()
        {
            var parser = new SimpleDerivativeParser();

            // Test associativity
            Check(1 + 2.5 + 10.8, parser.Parse("1 + 2.5 + 10.8")); // Addition associativity
            Check(2 - 5.8 - 12, parser.Parse("2 - 5.8 - 12")); // Subtraction associativity
            Check(3 * 1.8 * 0.9, parser.Parse("3 * 1.8 * 0.9")); // Multiplication associativity
            Check(4 / 0.4 / 2.8, parser.Parse("4 / 0.4 / 2.8")); // Division associativity
            Check(Math.Pow(2, Math.Pow(0.5, 3)), parser.Parse("2^0.5^3")); // Power associativity

            // Test brackets
            Check(1 - (5.8 - 12) - 3, parser.Parse("1 - (5.8 - 12) - 3"));
            Check(2 * (2 + 3) * 4, parser.Parse("2 * ((2 + 3)) * 4"));
        }

        [Test]
        public void When_SimpleDerivatives_Expect_Reference()
        {
            var parser = new SimpleDerivativeParser();

            // Define our variable x
            double x = 0, y = 0;
            parser.VariableFound += (sender, e) =>
            {
                if (e.Name == "x")
                {
                    e.Result = new DoubleDerivatives();
                    e.Result[0] = x;
                    e.Result[1] = 1;
                }
            };

            // Define our variable y
            parser.VariableFound += (sender, e) =>
            {
                if (e.Name == "y")
                {
                    e.Result = new DoubleDerivatives();
                    e.Result[0] = y;
                    e.Result[2] = 1;
                }
            };

            // Perform some tests
            for (x = -2; x <= 2; x += 0.5)
            {
                for (y = -2; y <= 2; y += 0.5)
                {
                    Check(new[] { 6 * x, 6 }, parser.Parse("6*x"));
                    Check(new[] { x * x, 2 * x }, parser.Parse("x^2"));
                    Check(new[] { x * (x - 3), 2 * x - 3 }, parser.Parse("x * (x - 3)"));
                    Check(new[] { 1 / Math.Pow(x, 3), -3 * Math.Pow(x, 2) / Math.Pow(x, 6) }, parser.Parse("1 / x^3")); // The parser will perform the chainrule: -1/(x^3)^2*(3*x^2) = -3*x^2/x^6
                    Check(new[] { 2 * x + 3 * y, 2, 3 }, parser.Parse("2 * x + 3 * y"));
                    Check(new[] { x * y, y, x }, parser.Parse("x * y"));
                    Check(new[] { Math.Pow(x, y), y * Math.Pow(x, y - 1), Math.Log(x) * Math.Pow(x, y) }, parser.Parse("x^y"));
                }
            }
        }
    }
}
