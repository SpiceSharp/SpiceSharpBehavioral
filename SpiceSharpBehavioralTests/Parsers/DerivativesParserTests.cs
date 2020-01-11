using System;
using NUnit.Framework;
using SpiceSharpBehavioral.Parsers.ShuntingYard;

namespace SpiceSharpBehavioralTests.Parsers
{
    [TestFixture]
    public class DerivativesParserTests
    {
        protected double RelativeTolerance = 1e-9;
        protected double AbsoluteTolerance = 1e-12;

        protected void Check(Func<double[], double>[] reference, DerivativesParser<char, double> parser, string expression)
        {
            // Add our variables
            var operators = ((ShuntingYardDescription<IDerivatives<char, double>>)parser.Description).Operators;
            var variables = new IDerivatives<char, double>[reference.Length - 1];
            for (var i = 0; i < reference.Length - 1; i++)
            {
                char id = (char)('a' + i);
                variables[i] = new Derivatives<char, double>
                {
                    [id] = 1.0
                };
            }
            void VariableFound(object sender, VariableFoundEventArgs<IDerivatives<char, double>> args)
                => args.Result = variables[args.Name[0] - 'a'];
            operators.VariableFound += VariableFound;

            for (var i = 0; i < variables.Length; i++)
                variables[i].Value = -2.0;
            double[] x = new double[variables.Length];
            while (true)
            {
                for (var k = 0; k < variables.Length; k++)
                    x[k] = variables[k].Value;

                var parsed = parser.Parse(expression);
                var expected = reference[0](x);
                var actual = parsed.Value;
                var tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * RelativeTolerance + AbsoluteTolerance;
                Assert.AreEqual(expected, actual, tol);

                for (var i = 1; i < reference.Length; i++)
                {
                    expected = reference[i](x);
                    if (!parsed.TryGetValue((char)('a' + i - 1), out actual))
                        actual = 0;
                    tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * RelativeTolerance + AbsoluteTolerance;
                    Assert.AreEqual(expected, actual, tol);
                }

                // Find the last index that reached the maximum
                var index = variables.Length - 1;
                while (index >= 0 && variables[index].Value >= 2.0)
                    index--;
                if (index < 0)
                    break;
                variables[index].Value += 0.5;
                for (var i = index + 1; i < variables.Length; i++)
                    variables[i].Value = 0.0;
            }
            operators.VariableFound -= VariableFound;
        }

        protected void Check(double expected, IDerivatives<char, double> parsed)
        {
            var actual = parsed.Value;
            var tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * RelativeTolerance + AbsoluteTolerance;
            Assert.AreEqual(expected, actual, tol);
        }

        [Test]
        public void When_Addition_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(1 + 2.5 + 10.8, parser.Parse("1 + 2.5 + 10.8"));
        }

        [Test]
        public void When_Subtraction_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(2 - 5.8 - 12, parser.Parse("2 - 5.8 - 12"));
        }

        [Test]
        public void When_Multiplication_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(3 * 1.8 * 0.9, parser.Parse("3 * 1.8 * 0.9"));
        }

        [Test]
        public void When_Division_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(4 / 0.4 / 2.8, parser.Parse("4 / 0.4 / 2.8"));
        }

        [Test]
        public void When_Power_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(Math.Pow(2, Math.Pow(0.5, 3)), parser.Parse("2^0.5^3"));
        }

        [Test]
        public void When_Brackets_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(1 - (5.8 - 12) - 3, parser.Parse("1 - (5.8 - 12) - 3"));
            Check(2 * (2 + 3) * 4, parser.Parse("2 * ((2 + 3)) * 4"));
        }

        [Test]
        public void When_Conditional_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(1, parser.Parse("1 >= 0 ? 1 : 2"));
            Check(2, parser.Parse("1 >= 3 ? 1 : 2"));
        }

        [Test]
        public void When_Arithmetic_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(new Func<double[], double>[] { x => 6 * x[0], x => 6 }, parser, "6*a");
            Check(new Func<double[], double>[] { x => x[0] * x[0], x => 2 * x[0] }, parser, "a^2");
            Check(new Func<double[], double>[] { x => x[0] * (x[0] - 3), x => 2 * x[0] - 3 }, parser, "a * (a - 3)");
            Check(new Func<double[], double>[] { x => 1 / Math.Pow(x[0], 3), x => -3 * Math.Pow(x[0], 2) / Math.Pow(x[0], 6) }, parser, "1 / a^3");
            Check(new Func<double[], double>[] { x => 2 * x[0] + 3 * x[1], x => 2, x => 3 }, parser, "2 * a + 3 * b");
            Check(new Func<double[], double>[] { x => x[0] * x[1], x => x[1], x => x[0] }, parser, "a * b");
            Check(new Func<double[], double>[] { x => Math.Pow(x[0], x[1]), x => x[1] * Math.Pow(x[0], x[1] - 1), x => Math.Log(x[0]) * Math.Pow(x[0], x[1]) }, parser, "a^b");
        }

        [Test]
        public void When_Exp_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(new Func<double[], double>[] { x => Math.Exp(x[0]), x => Math.Exp(x[0]) }, parser, "exp(a)");
        }

        [Test]
        public void When_Log_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(new Func<double[], double>[] { x => Math.Log(x[0]), x => 1 / x[0] }, parser, "log(a)");
            Check(new Func<double[], double>[] { x => Math.Log10(x[0]), x => 1 / Math.Log(10.0) / x[0] }, parser, "log10(a)");
        }

        [Test]
        public void When_Pow_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(new Func<double[], double>[] {
                x => Math.Pow(x[0], x[1]),
                x => x[1] * Math.Pow(x[0], x[1] - 1),
                x => Math.Pow(x[0], x[1]) * Math.Log(x[0]) }, parser, "pow(a, b)");
        }

        [Test]
        public void When_Sqrt_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(new Func<double[], double>[] { x => Math.Sqrt(x[0]), x => 0.5 / Math.Sqrt(x[0]) }, parser, "sqrt(a)");
        }

        [Test]
        public void When_Sin_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(new Func<double[], double>[] { x => Math.Sin(x[0]), x => Math.Cos(x[0]) }, parser, "sin(a)");
        }

        [Test]
        public void When_Cos_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(new Func<double[], double>[] { x => Math.Cos(x[0]), x => -Math.Sin(x[0]) }, parser, "cos(a)");
        }

        [Test]
        public void When_Tan_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(new Func<double[], double>[] { x => Math.Tan(x[0]), x => 1.0 / Math.Cos(x[0]) / Math.Cos(x[0]) }, parser, "tan(a)");
        }

        [Test]
        public void When_Asin_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(new Func<double[], double>[] { x => Math.Asin(x[0]), x => 1.0 / Math.Sqrt(1 - x[0] * x[0]) }, parser, "asin(a)");
        }

        [Test]
        public void When_Acos_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(new Func<double[], double>[] { x => Math.Acos(x[0]), x => -1.0 / Math.Sqrt(1 - x[0] * x[0]) }, parser, "acos(a)");
        }

        [Test]
        public void When_Atan_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(new Func<double[], double>[] { x => Math.Atan(x[0]), x => 1.0 / (1 + x[0] * x[0]) }, parser, "atan(a)");
        }

        [Test]
        public void When_Abs_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(new Func<double[], double>[] { x => Math.Abs(x[0]), x => x[0] > 0 ? 1 : x[0] < 0 ? -1 : 0 }, parser, "abs(a)");
        }

        [Test]
        public void When_Abs_NotSimpleArgument_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(new Func<double[], double>[] { x => Math.Abs(2 * x[0]), x => x[0] > 0 ? 2 : x[0] < 0 ? -2 : 0 }, parser, "abs(2*a)");
        }
        [Test]
        public void When_Round_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(new Func<double[], double>[] { x => Math.Round(x[0]), x => 0 }, parser, "round(a)");
            Check(new Func<double[], double>[] { x => Math.Round(x[0], 2), x => 0 }, parser, "round(a, 2)");
        }

        [Test]
        public void When_Min_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(new Func<double[], double>[] { x => Math.Min(x[0], 1), x => x[0] < 1.0 ? 1 : 0 }, parser, "min(a, 1)");
            Check(new Func<double[], double>[] {
                x => Math.Min(x[0] * x[0], x[0]),
                x =>
                {
                    if (x[0] > 0 && x[0] < 1)
                        return 2 * x[0];
                    return 1.0;
                }}, parser, "min(a^2, a)");
        }

        [Test]
        public void When_Max_Expect_Reference()
        {
            var parser = new DerivativesParser<char, double>(new DoubleOperators());
            Check(new Func<double[], double>[] { x => Math.Max(x[0], 1), x => x[0] > 1.0 ? 1 : 0 }, parser, "max(a, 1)");
            Check(new Func<double[], double>[] {
                x => Math.Max(x[0] * x[0], x[0]),
                x =>
                {
                    if (x[0] < 0 || x[0] > 1)
                        return 2 * x[0];
                    return 1.0;
                }}, parser, "max(a^2, a)");
        }
    }
}
