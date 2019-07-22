using System;
using NUnit.Framework;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Helper;

namespace SpiceSharpBehavioralTests.Parsers
{
    [TestFixture]
    public class SimpleParserTests
    {
        protected double RelativeTolerance = 1e-9;
        protected double AbsoluteTolerance = 1e-12;

        protected void Check(double expected, double actual)
        {
            var tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * RelativeTolerance + AbsoluteTolerance;
            Assert.AreEqual(expected, actual, tol);
        }

        [Test]
        public void When_SimpleArithmetic_Expect_Reference()
        {
            var parser = new SimpleParser();

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
        public void When_Functions_Expect_Reference()
        {
            var parser = new SimpleParser();
            parser.RegisterDefaultFunctions();

            // Test basic functions
            Check(Math.Exp(10), parser.Parse("Exp(10)"));
            Check(Math.Exp(-10), parser.Parse("Exp(-10)"));
            Check(Math.Log(5), parser.Parse("Log(5)"));
            Check(Math.Log(-5), parser.Parse("Log(-5)")); // Should give NaN
            Check(Math.Log10(5), parser.Parse("Log10(5)"));
            Check(Math.Pow(2, 3), parser.Parse("Pow(2, 3)"));
            Check(Math.Pow(-2, -0.5), parser.Parse("Pow(-2, -0.5)")); // Should give NaN)
            Check(Math.Sqrt(5), parser.Parse("Sqrt(5)"));
            Check(Math.Sqrt(-5), parser.Parse("Sqrt(-5)")); // Should give NaN
            Check(Math.Sin(2), parser.Parse("Sin(2)"));
            Check(Math.Sin(-0.5), parser.Parse("Sin(-0.5)"));
            Check(Math.Cos(2), parser.Parse("Cos(2)"));
            Check(Math.Cos(-0.5), parser.Parse("Cos(-0.5)"));
            Check(Math.Tan(2), parser.Parse("Tan(2)"));
            Check(Math.Tan(-0.5), parser.Parse("Tan(-0.5)"));
            Check(Math.Asin(0.25), parser.Parse("Asin(0.25)"));
            Check(Math.Asin(-0.4), parser.Parse("Asin(-0.4)"));
            Check(Math.Acos(0.25), parser.Parse("Acos(0.25)"));
            Check(Math.Acos(-0.4), parser.Parse("Acos(-0.4)"));
        }
    }
}
