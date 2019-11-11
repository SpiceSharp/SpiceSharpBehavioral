using System;
using System.Linq.Expressions;
using NUnit.Framework;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Helper;

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
        public void When_Addition_Expect_Reference()
        {
            var parser = new ExpressionTreeParser();
            Check(1 + 2.5 + 10.8, parser.Parse("1 + 2.5 + 10.8"));
        }

        [Test]
        public void When_Subtraction_Expect_Reference()
        {
            var parser = new ExpressionTreeParser();
            Check(2 - 5.8 - 12, parser.Parse("2 - 5.8 - 12"));
        }

        [Test]
        public void When_Multiplication_Expect_Reference()
        {
            var parser = new ExpressionTreeParser();
            Check(3 * 1.8 * 0.9, parser.Parse("3 * 1.8 * 0.9"));
        }

        [Test]
        public void When_Division_Expect_Reference()
        {
            var parser = new ExpressionTreeParser();
            Check(4 / 0.4 / 2.8, parser.Parse("4 / 0.4 / 2.8"));
        }

        [Test]
        public void When_Power_Expect_Reference()
        {
            var parser = new ExpressionTreeParser();
            Check(Math.Pow(2, Math.Pow(0.5, 3)), parser.Parse("2^0.5^3"));
        }

        [Test]
        public void When_Brackets_Expect_Reference()
        {
            var parser = new ExpressionTreeParser();
            Check(1 - (5.8 - 12) - 3, parser.Parse("1 - (5.8 - 12) - 3"));
            Check(2 * (2 + 3) * 4, parser.Parse("2 * ((2 + 3)) * 4"));
        }

        [Test]
        public void When_Conditional_Expect_Reference()
        {
            var parser = new ExpressionTreeParser();
            Check(1, parser.Parse("1 >= 0 ? 1 : 2"));
            Check(2, parser.Parse("1 >= 3 ? 1 : 2"));
        }

        private ExpressionTreeParser Parser
        {
            get
            {
                var parser = new ExpressionTreeParser();
                parser.RegisterDefaultFunctions();
                return parser;
            }
        }

        [Test]
        public void When_Exp_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Exp(10), parser.Parse("Exp(10)"));
            Check(Math.Exp(-10), parser.Parse("Exp(-10)"));
        }

        [Test]
        public void When_Log_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Log(5), parser.Parse("Log(5)"));
            Check(Math.Log(-5), parser.Parse("Log(-5)")); // Should give NaN
            Check(Math.Log10(5), parser.Parse("Log10(5)"));
        }

        [Test]
        public void When_Pow_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Pow(2, 3), parser.Parse("Pow(2, 3)"));
            Check(Math.Pow(-2, -0.5), parser.Parse("Pow(-2, -0.5)")); // Should give NaN)
        }

        [Test]
        public void When_Sqrt_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Sqrt(5), parser.Parse("Sqrt(5)"));
            Check(Math.Sqrt(-5), parser.Parse("Sqrt(-5)")); // Should give NaN
        }

        [Test]
        public void When_Sin_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Sin(2), parser.Parse("Sin(2)"));
            Check(Math.Sin(-0.5), parser.Parse("Sin(-0.5)"));
        }

        [Test]
        public void When_Cos_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Cos(2), parser.Parse("Cos(2)"));
            Check(Math.Cos(-0.5), parser.Parse("Cos(-0.5)"));
        }

        [Test]
        public void When_Tan_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Tan(2), parser.Parse("Tan(2)"));
            Check(Math.Tan(-0.5), parser.Parse("Tan(-0.5)"));
        }

        [Test]
        public void When_Sinh_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Sinh(2), parser.Parse("Sinh(2)"));
            Check(Math.Sinh(-0.5), parser.Parse("Sinh(-0.5)"));
        }

        [Test]
        public void When_Cosh_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Cosh(2), parser.Parse("Cosh(2)"));
            Check(Math.Cosh(-0.5), parser.Parse("Cosh(-0.5)"));
        }

        [Test]
        public void When_Tanh_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Tanh(2), parser.Parse("Tanh(2)"));
            Check(Math.Tanh(-0.5), parser.Parse("Tanh(-0.5)"));
        }

        [Test]
        public void When_Asin_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Asin(0.25), parser.Parse("Asin(0.25)"));
            Check(Math.Asin(-0.4), parser.Parse("Asin(-0.4)"));
        }

        [Test]
        public void When_Acos_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Acos(0.25), parser.Parse("Acos(0.25)"));
            Check(Math.Acos(-0.4), parser.Parse("Acos(-0.4)"));
        }

        [Test]
        public void When_Atan_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Atan(0.25), parser.Parse("Atan(0.25)"));
            Check(Math.Atan(-0.4), parser.Parse("Atan(-0.4)"));
        }

        [Test]
        public void When_Abs_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Abs(0.5), parser.Parse("Abs(0.5)"));
            Check(Math.Abs(-0.25), parser.Parse("Abs(-0.25)"));
        }

        [Test]
        public void When_Round_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Round(3.8), parser.Parse("Round(3.8)"));
            Check(Math.Round(-0.9), parser.Parse("Round(-0.9)"));
            Check(Math.Round(0.2345, 2), parser.Parse("Round(0.2345, 2)"));
            Check(Math.Round(-18.295, 1), parser.Parse("Round(-18.295, 1)"));
        }

        [Test]
        public void When_Min_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Min(1, -1), parser.Parse("Min(1, -1)"));
            Check(Math.Min(1, Math.Min(2, -2)), parser.Parse("Min(1, 2, -2)"));
        }

        [Test]
        public void When_Max_Expect_Reference()
        {
            var parser = Parser;
            Check(Math.Max(1, -1), parser.Parse("Max(1, -1)"));
            Check(Math.Max(1, Math.Max(2, -2)), parser.Parse("Max(1, 2, -2)"));
        }
    }
}
