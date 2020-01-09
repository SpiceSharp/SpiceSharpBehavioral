using System;
using NUnit.Framework;
using SpiceSharpBehavioral.Parsers;

namespace SpiceSharpBehavioralTests.Parsers
{
    [TestFixture]
    public class DouleFuncParserTests
    {
        protected double RelativeTolerance = 1e-9;
        protected double AbsoluteTolerance = 1e-12;

        protected void Check(double expected, Func<double> actual)
        {
            var value = actual();
            var tol = Math.Max(Math.Abs(expected), Math.Abs(value)) * RelativeTolerance + AbsoluteTolerance;
            Assert.AreEqual(expected, value, tol);
        }

        [Test]
        public void When_Addition_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(1 + 2.5 + 10.8, parser.Parse("1 + 2.5 + 10.8"));
        }

        [Test]
        public void When_Subtraction_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(2 - 5.8 - 12, parser.Parse("2 - 5.8 - 12"));
        }

        [Test]
        public void When_Multiplication_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(3 * 1.8 * 0.9, parser.Parse("3 * 1.8 * 0.9"));
        }

        [Test]
        public void When_Division_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(4 / 0.4 / 2.8, parser.Parse("4 / 0.4 / 2.8"));
        }

        [Test]
        public void When_Power_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(Math.Pow(2, Math.Pow(0.5, 3)), parser.Parse("2^0.5^3"));
        }

        [Test]
        public void When_Brackets_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(1 - (5.8 - 12) - 3, parser.Parse("1 - (5.8 - 12) - 3"));
            Check(2 * (2 + 3) * 4, parser.Parse("2 * ((2 + 3)) * 4"));
        }

        [Test]
        public void When_Conditional_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(1, parser.Parse("1 >= 0 ? 1 : 2"));
            Check(2, parser.Parse("1 >= 3 ? 1 : 2"));
            Check(2, parser.Parse("1 ? 0 ? 1 : 2 : 3"));
            Check(3, parser.Parse("0 ? 1 : 2 ? 3 : 4"));
        }

        [Test]
        public void When_Equal_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(1, parser.Parse("3 == 3"));
            Check(0, parser.Parse("3 == 5"));
        }


        [Test]
        public void When_NotEqual_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(1, parser.Parse("3 != 5 ? 1 : 2"));
            Check(2, parser.Parse("3 != 3 ? 1 : 2"));
        }

        [Test]
        public void When_NotEqual_With_NonZeroTolerance_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            ((SpiceSharpBehavioral.Parsers.DoubleFunc.RelationalOperator)parser.Parameters.Relational).AbsoluteTolerance = 0.1;
            Check(1, parser.Parse("3 != 4.05 ? 1 : 2"));
            Check(2, parser.Parse("3 != 3.05 ? 1 : 2"));
        }

        [Test]
        public void When_Equal_With_NonZeroTolerance_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            ((SpiceSharpBehavioral.Parsers.DoubleFunc.RelationalOperator)parser.Parameters.Relational).AbsoluteTolerance = 0.1;
            Check(1, parser.Parse("3 == 3.05 ? 1 : 2"));
        }

        [Test]
        public void When_Exp_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(Math.Exp(10), parser.Parse("exp(10)"));
            Check(Math.Exp(-10), parser.Parse("exp(-10)"));
        }

        [Test]
        public void When_Log_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(Math.Log(5), parser.Parse("log(5)"));
            Check(Math.Log(-5), parser.Parse("log(-5)")); // Should give NaN
            Check(Math.Log10(5), parser.Parse("log10(5)"));
            Check(Math.Log(3), parser.Parse("ln(3)"));
        }

        [Test]
        public void When_Pow_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(Math.Pow(2, 3), parser.Parse("pow(2, 3)"));
            Check(Math.Pow(-2, -0.5), parser.Parse("pow(-2, -0.5)")); // Should give NaN)
        }

        [Test]
        public void When_Sqrt_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(Math.Sqrt(5), parser.Parse("sqrt(5)"));
            Check(Math.Sqrt(-5), parser.Parse("sqrt(-5)")); // Should give NaN
        }

        [Test]
        public void When_Sin_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(Math.Sin(2), parser.Parse("sin(2)"));
            Check(Math.Sin(-0.5), parser.Parse("sin(-0.5)"));
        }

        [Test]
        public void When_Cos_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(Math.Cos(2), parser.Parse("cos(2)"));
            Check(Math.Cos(-0.5), parser.Parse("cos(-0.5)"));
        }

        [Test]
        public void When_Tan_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(Math.Tan(2), parser.Parse("tan(2)"));
            Check(Math.Tan(-0.5), parser.Parse("tan(-0.5)"));
        }

        [Test]
        public void When_Asin_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(Math.Asin(0.25), parser.Parse("asin(0.25)"));
            Check(Math.Asin(-0.4), parser.Parse("asin(-0.4)"));
        }

        [Test]
        public void When_Acos_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(Math.Acos(0.25), parser.Parse("acos(0.25)"));
            Check(Math.Acos(-0.4), parser.Parse("acos(-0.4)"));
        }

        [Test]
        public void When_Atan_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(Math.Atan(0.25), parser.Parse("atan(0.25)"));
            Check(Math.Atan(-0.4), parser.Parse("atan(-0.4)"));
        }

        [Test]
        public void When_Abs_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(Math.Abs(0.5), parser.Parse("abs(0.5)"));
            Check(Math.Abs(-0.25), parser.Parse("abs(-0.25)"));
        }

        [Test]
        public void When_Round_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(Math.Round(3.8), parser.Parse("round(3.8)"));
            Check(Math.Round(-0.9), parser.Parse("round(-0.9)"));
            Check(Math.Round(0.2345, 2), parser.Parse("round(0.2345, 2)"));
            Check(Math.Round(-18.295, 1), parser.Parse("round(-18.295, 1)"));
        }

        [Test]
        public void When_Min_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(Math.Min(1, -1), parser.Parse("min(1, -1)"));
            Check(Math.Min(1, Math.Min(2, -2)), parser.Parse("min(1, 2, -2)"));
        }

        [Test]
        public void When_Max_Expect_Reference()
        {
            var parser = new Lexer<Func<double>>(new DoubleFuncParserParameters());
            Check(Math.Max(1, -1), parser.Parse("max(1, -1)"));
            Check(Math.Max(1, Math.Max(2, -2)), parser.Parse("max(1, 2, -2)"));
        }
    }
}
