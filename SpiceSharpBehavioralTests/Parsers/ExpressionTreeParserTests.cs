using System;
using System.Linq.Expressions;
using NUnit.Framework;
using SpiceSharpBehavioral.Parsers;

namespace SpiceSharpBehavioralTests.Parsers
{
    [TestFixture]
    public class ExpressionTreeParserTests
    {
        protected double RelativeTolerance = 1e-9;
        protected double AbsoluteTolerance = 1e-12;

        protected void Check(double expected, Expression actual)
        {
            var method = Expression.Lambda<Func<double>>(actual).Compile();
            var result = method();
            var tol = Math.Max(Math.Abs(expected), Math.Abs(result)) * RelativeTolerance + AbsoluteTolerance;
            Assert.AreEqual(expected, result, tol);
        }

        [Test]
        public void When_SimpleArithmetic_Expect_Reference()
        {
            var parser = new ExpressionTreeParser();

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
    }
}
