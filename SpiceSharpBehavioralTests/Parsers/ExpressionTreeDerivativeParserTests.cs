using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Helper;

namespace SpiceSharpBehavioralTests.Parsers
{
    [TestFixture]
    public class ExpressionTreeDerivativeTests
    {
        protected double RelativeTolerance = 1e-9;
        protected double AbsoluteTolerance = 1e-12;

        protected void Check(double expected, double actual)
        {
            var tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * RelativeTolerance + AbsoluteTolerance;
            Assert.AreEqual(expected, actual, tol);
        }

        protected void Check(double[] expected, Derivatives<Expression> actual)
        {
            for (var i = 0; i < expected.Length; i++)
            {
                var method = Expression.Lambda<Func<double>>(actual[i]).Compile();
                var result = method();
                var tol = Math.Max(Math.Abs(expected[i]), Math.Abs(result)) * RelativeTolerance + AbsoluteTolerance;
                Assert.AreEqual(expected[i], result, tol);
            }
        }

        protected void Check(double expected, Derivatives<Expression> actual)
        {
            Assert.AreEqual(actual.Count, 1);
            var method = Expression.Lambda<Func<double>>(actual[0]).Compile();
            var result = method();
            var tol = Math.Max(Math.Abs(expected), Math.Abs(result)) * RelativeTolerance + AbsoluteTolerance;
            Assert.AreEqual(expected, result, tol);
        }

        protected void Compare(List<Tuple<Func<double>, Func<double>>> list, ExpressionTreeDerivatives derivatives, params Func<double>[] expected)
        {
            Assert.IsTrue(expected.Length >= derivatives.Count);
            for (var i = 0; i < expected.Length; i++)
            {
                var actual = derivatives.Compile<Func<double>>(i);
                list.Add(new Tuple<Func<double>, Func<double>>(
                    actual, expected[i]));
            }
        }

        [Test]
        public void When_SimpleArithmetic_Expect_Reference()
        {
            var parser = new ExpressionTreeDerivativeParser();

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

            var parser = new ExpressionTreeDerivativeParser();

            // Define our variable x
            double x = 0, y = 0;
            parser.VariableFound += (sender, e) =>
            {
                if (e.Name == "x")
                {
                    e.Result = new ExpressionTreeDerivatives();
                    Func<double> getx = () => x;
                    e.Result[0] = Expression.Call(Expression.Constant(getx.Target), getx.Method);
                    e.Result[1] = Expression.Constant(1.0);
                }
            };

            // Define our variable y
            parser.VariableFound += (sender, e) =>
            {
                if (e.Name == "y")
                {
                    e.Result = new ExpressionTreeDerivatives();
                    Func<double> gety = () => y;
                    e.Result[0] = Expression.Call(Expression.Constant(gety.Target), gety.Method);
                    e.Result[2] = Expression.Constant(1.0);
                }
            };

            // Compile all these methods
            var list = new List<Tuple<Func<double>, Func<double>>>();
            Compare(list, parser.Parse("6*x"), () => 6 * x, () => 6);
            Compare(list, parser.Parse("x^2"), () => x * x, () => 2 * x);
            Compare(list, parser.Parse("x * (x - 3)"), () => x * (x - 3), () => 2 * x - 3);
            Compare(list, parser.Parse("1 / x^3"), () => 1 / Math.Pow(x, 3), () => -3 * Math.Pow(x, 2) / Math.Pow(x, 6));
            Compare(list, parser.Parse("2 * x + 3 * y"), () => 2 * x + 3 * y, () => 2, () => 3);
            Compare(list, parser.Parse("x * y"), () => x * y, () => y, () => x);
            Compare(list, parser.Parse("x ^ y"), () => Math.Pow(x, y), () => y * Math.Pow(x, y - 1), () => Math.Log(x) * Math.Pow(x, y));

            // Perform some tests
            for (x = -2; x <= 2; x += 0.5)
            {
                for (y = -2; y <= 2; y += 0.5)
                {
                    for (var i = 0; i < list.Count; i++)
                        Check(list[i].Item2(), list[i].Item1());
                }
            }
        }

        [Test]
        public void When_FunctionDerivatives_Expect_Reference()
        {
            var parser = new ExpressionTreeDerivativeParser();
            parser.RegisterDefaultFunctions();

            // Define our variable x
            double x = 0;
            parser.VariableFound += (sender, e) =>
            {
                if (e.Name == "x")
                {
                    e.Result = new ExpressionTreeDerivatives();
                    Func<double> getx = () => x;
                    e.Result[0] = Expression.Call(Expression.Constant(getx.Target), getx.Method);
                    e.Result[1] = Expression.Constant(1.0);
                }
            };

            // Compile all these methods
            var list = new List<Tuple<Func<double>, Func<double>>>();
            Compare(list, parser.Parse("Exp(x)"), () => Math.Exp(x), () => Math.Exp(x));
            Compare(list, parser.Parse("Log(x^2)"), () => Math.Log(x * x), () => 2 * x / x / x);
            Compare(list, parser.Parse("Log10(x)"), () => Math.Log10(x), () => 1 / Math.Log(10.0) / x);
            Compare(list, parser.Parse("Pow(x, 5)"), () => Math.Pow(x, 5), () => Math.Pow(x, 4) * 5);
            Compare(list, parser.Parse("Sqrt(x)"), () => Math.Sqrt(x), () => -0.5 / Math.Sqrt(x));
            Compare(list, parser.Parse("Sin(x)"), () => Math.Sin(x), () => Math.Cos(x));
            Compare(list, parser.Parse("Cos(x)"), () => Math.Cos(x), () => -Math.Sin(x));
            Compare(list, parser.Parse("Tan(x)"), () => Math.Tan(x), () => 1 / Math.Cos(x) / Math.Cos(x));
            Compare(list, parser.Parse("Asin(x)"), () => Math.Asin(x), () => 1 / Math.Sqrt(1 - x * x));
            Compare(list, parser.Parse("Acos(x)"), () => Math.Acos(x), () => -1 / Math.Sqrt(1 - x * x));
            Compare(list, parser.Parse("Atan(x)"), () => Math.Atan(x), () => 1 / (1 + x * x));


            // Perform some tests
            for (x = -2; x <= 2; x += 0.5)
            {
                for (var i = 0; i < list.Count; i++)
                    Check(list[i].Item2(), list[i].Item1());
            }
        }
    }
}
