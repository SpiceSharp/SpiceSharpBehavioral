using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SpiceSharpBehavioralTests.Builders
{
    [TestFixture]
    public class ExpressionBuilderTests
    {
        private static Func<double> Compile(Expression e)
        {
            if (e is ConstantExpression ce)
                return () => (double)ce.Value;
            return Expression.Lambda<Func<double>>(e).Compile();
        }

        [TestCaseSource(typeof(ExpressionBuilderTestData), nameof(ExpressionBuilderTestData.SpiceNumbers))]
        public void When_SpiceDouble_Expect_Reference(string input, double expected)
        {
            IBuilder<Expression> builder = new ExpressionBuilder() { SimplifyConstants = true };
            Assert.AreEqual(expected, Compile(builder.CreateNumber(input)).Invoke(), 1e-20);
        }

        [TestCaseSource(typeof(ExpressionBuilderTestData), nameof(ExpressionBuilderTestData.Combinations))]
        public void When_Arithmetic_Expect_Reference(double a, double b)
        {
            var ca = Expression.Constant(a);
            var cb = Expression.Constant(b);
            IBuilder<Expression> builder = new ExpressionBuilder { SimplifyConstants = false, FudgeFactor = 0.0 };
            Assert.AreEqual(+a, Compile(builder.Plus(ca)).Invoke(), 1e-20);
            Assert.AreEqual(-a, Compile(builder.Minus(ca)).Invoke(), 1e-20);
            Assert.AreEqual(a + b, Compile(builder.Add(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a - b, Compile(builder.Subtract(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a * b, Compile(builder.Multiply(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(b != 0.0 ? a / b : double.PositiveInfinity, Compile(builder.Divide(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(Math.Pow(a, b), Compile(builder.Pow(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a % b, Compile(builder.Mod(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a != 0.0 && b != 0.0 ? 1.0 : 0.0, Compile(builder.And(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a != 0.0 || b != 0.0 ? 1.0 : 0.0, Compile(builder.Or(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a != 0.0 ? a : b, Compile(builder.Conditional(ca, ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a == b ? 1.0 : 0.0, Compile(builder.Equals(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a != b ? 1.0 : 0.0, Compile(builder.NotEquals(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a > b ? 1.0 : 0.0, Compile(builder.GreaterThan(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a >= b ? 1.0 : 0.0, Compile(builder.GreaterThanOrEqual(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a < b ? 1.0 : 0.0, Compile(builder.LessThan(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a <= b ? 1.0 : 0.0, Compile(builder.LessThanOrEqual(ca, cb)).Invoke(), 1e-20);
        }

        [TestCaseSource(typeof(ExpressionBuilderTestData), nameof(ExpressionBuilderTestData.Combinations))]
        public void When_ArithmeticSimplified_Expect_Reference(double a, double b)
        {
            var ca = Expression.Constant(a);
            var cb = Expression.Constant(b);
            IBuilder<Expression> builder = new ExpressionBuilder { SimplifyConstants = true, FudgeFactor = 0.0 };
            Assert.AreEqual(+a, Compile(builder.Plus(ca)).Invoke(), 1e-20);
            Assert.AreEqual(-a, Compile(builder.Minus(ca)).Invoke(), 1e-20);
            Assert.AreEqual(a + b, Compile(builder.Add(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a - b, Compile(builder.Subtract(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a * b, Compile(builder.Multiply(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a / b, Compile(builder.Divide(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(Math.Pow(a, b), Compile(builder.Pow(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a % b, Compile(builder.Mod(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a != 0.0 && b != 0.0 ? 1.0 : 0.0, Compile(builder.And(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a != 0.0 || b != 0.0 ? 1.0 : 0.0, Compile(builder.Or(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a != 0.0 ? a : b, Compile(builder.Conditional(ca, ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a == b ? 1.0 : 0.0, Compile(builder.Equals(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a != b ? 1.0 : 0.0, Compile(builder.NotEquals(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a > b ? 1.0 : 0.0, Compile(builder.GreaterThan(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a >= b ? 1.0 : 0.0, Compile(builder.GreaterThanOrEqual(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a < b ? 1.0 : 0.0, Compile(builder.LessThan(ca, cb)).Invoke(), 1e-20);
            Assert.AreEqual(a <= b ? 1.0 : 0.0, Compile(builder.LessThanOrEqual(ca, cb)).Invoke(), 1e-20);
        }

        [TestCaseSource(typeof(ExpressionBuilderTestData), nameof(ExpressionBuilderTestData.Combinations))]
        public void When_Functions_Expect_Reference(double a, double b)
        {
            var ca = Expression.Constant(a);
            var cb = Expression.Constant(b);
            IBuilder<Expression> builder = new ExpressionBuilder() { SimplifyConstants = false };
            Assert.AreEqual(Math.Abs(a), Compile(builder.CreateFunction("abs", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Sqrt(a), Compile(builder.CreateFunction("sqrt", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(a < 0 ? -Math.Pow(-a, b) : Math.Pow(a, b), Compile(builder.CreateFunction("pwr", new[] { ca, cb })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Min(a, b), Compile(builder.CreateFunction("min", new[] { ca, cb })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Max(a, b), Compile(builder.CreateFunction("max", new[] { ca, cb })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Log(a), Compile(builder.CreateFunction("log", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Log10(a), Compile(builder.CreateFunction("log10", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Exp(a), Compile(builder.CreateFunction("exp", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Sin(a), Compile(builder.CreateFunction("sin", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Cos(a), Compile(builder.CreateFunction("cos", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Tan(a), Compile(builder.CreateFunction("tan", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Asin(a), Compile(builder.CreateFunction("asin", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Acos(a), Compile(builder.CreateFunction("acos", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Atan(a), Compile(builder.CreateFunction("atan", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Sinh(a), Compile(builder.CreateFunction("sinh", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Cosh(a), Compile(builder.CreateFunction("cosh", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Tanh(a), Compile(builder.CreateFunction("tanh", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(a > 0 ? 1.0 : a == 0.0 ? 0.5 : 0, Compile(builder.CreateFunction("u", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(a > 0 ? a < 1 ? a : 1 : 0.0, Compile(builder.CreateFunction("u2", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(a > 0 ? a : 0, Compile(builder.CreateFunction("uramp", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Ceiling(a), Compile(builder.CreateFunction("ceil", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Floor(a), Compile(builder.CreateFunction("floor", new[] { ca })).Invoke(), 1e-20);
            Assert.AreEqual(Math.Round(a, 0), Compile(builder.CreateFunction("nint", new[] { ca })).Invoke(), 1e-20);
            if (b >= 0)
                Assert.AreEqual(Math.Round(a, (int)b), Compile(builder.CreateFunction("round", new[] { ca, cb })).Invoke(), 1e-20);
            Assert.AreEqual(a * a, Compile(builder.CreateFunction("square", new[] { ca })).Invoke(), 1e-20);
        }

        [Test]
        public void When_Voltage_Expect_Reference()
        {
            var op = new OP("op");
            IBuilder<Expression> builder = new ExpressionBuilder(op);
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 2.0),
                new Resistor("R1", "in", "out", 1e3),
                new Resistor("R2", "out", "0", 2e3));
            Func<double> func = null, func2 = null;
            op.AfterSetup += (sender, args) =>
            {
                func = Compile(builder.CreateVoltage("out", null, QuantityTypes.Raw));
                func2 = Compile(builder.CreateVoltage("in", "out", QuantityTypes.Raw));
            };
            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(4.0 / 3.0, func(), 1e-12);
                Assert.AreEqual(2.0 / 3.0, func2(), 1e-12);
            };
            op.Run(ckt);
        }

        [Test]
        public void When_ComplexVoltage_Expect_Reference()
        {
            var ac = new AC("ac", new DecadeSweep(1.0, 1e9, 4));
            IBuilder<Expression> builder = new ExpressionBuilder(ac);

            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 2.0).SetParameter("acmag", 1.0),
                new Resistor("R1", "in", "out", 1e3),
                new Capacitor("C1", "out", "0", 1e-6));
            Func<double> func = null, func2 = null, func3 = null, func4 = null, func5 = null;
            ac.AfterSetup += (sender, args) =>
            {
                func = Compile(builder.CreateVoltage("out", null, QuantityTypes.Real));
                func2 = Compile(builder.CreateVoltage("out", null, QuantityTypes.Imaginary));
                func3 = Compile(builder.CreateVoltage("out", null, QuantityTypes.Magnitude));
                func4 = Compile(builder.CreateVoltage("out", null, QuantityTypes.Phase));
                func5 = Compile(builder.CreateVoltage("out", null, QuantityTypes.Decibels));
            };
            ac.ExportSimulationData += (sender, args) =>
            {
                var c = 1.0 / (1.0 + 1e-3 * args.Laplace);
                Assert.AreEqual(c.Real, func(), 1e-12);
                Assert.AreEqual(c.Imaginary, func2(), 1e-12);
                Assert.AreEqual(c.Magnitude, func3(), 1e-12);
                Assert.AreEqual(c.Phase, func4(), 1e-12);
                Assert.AreEqual(10.0 * Math.Log10(c.Real * c.Real + c.Imaginary * c.Imaginary), func5(), 1e-12);
            };
            ac.Run(ckt);
        }

        [Test]
        public void When_Current_Expect_Reference()
        {
            var op = new OP("op");
            IBuilder<Expression> builder = new ExpressionBuilder(op);
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 2.0),
                new Resistor("R1", "in", "out", 1e3),
                new Resistor("R2", "out", "0", 2e3));
            Func<double> func = null, func2 = null;
            op.AfterSetup += (sender, args) =>
            {
                func = Compile(builder.CreateCurrent("V1", QuantityTypes.Raw)); // From a branched entity
                func2 = Compile(builder.CreateCurrent("R1", QuantityTypes.Raw)); // From a non-branched entity
            };
            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(-2.0 / 3.0e3, func(), 1e-12);
                Assert.AreEqual(2.0 / 3.0e3, func2(), 1e-12);
            };
            op.Run(ckt);
        }

        [Test]
        public void When_ComplexCurrent_Expect_Reference()
        {
            var ac = new AC("ac", new DecadeSweep(1.0, 1e9, 4));
            IBuilder<Expression> builder = new ExpressionBuilder(ac);
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 2.0).SetParameter("acmag", 1.0),
                new Resistor("R1", "in", "out", 1e3),
                new Capacitor("C1", "out", "0", 1e-6));
            Func<double> func = null, func2 = null, func3 = null, func4 = null, func5 = null;
            ac.AfterSetup += (sender, args) =>
            {
                func = Compile(builder.CreateCurrent("V1", QuantityTypes.Real));
                func2 = Compile(builder.CreateCurrent("V1", QuantityTypes.Imaginary));
                func3 = Compile(builder.CreateCurrent("V1", QuantityTypes.Magnitude));
                func4 = Compile(builder.CreateCurrent("V1", QuantityTypes.Phase));
                func5 = Compile(builder.CreateCurrent("V1", QuantityTypes.Decibels));
            };
            ac.ExportSimulationData += (sender, args) =>
            {
                var v = 1.0 / (1.0 + 1e-3 * args.Laplace);
                var c = -v * 1e-6 * args.Laplace;
                Assert.AreEqual(c.Real, func(), 1e-12);
                Assert.AreEqual(c.Imaginary, func2(), 1e-12);
                Assert.AreEqual(c.Magnitude, func3(), 1e-12);
                Assert.AreEqual(c.Phase, func4(), 1e-12);
                Assert.AreEqual(10.0 * Math.Log10(c.Real * c.Real + c.Imaginary * c.Imaginary), func5(), 1e-12);
            };
            ac.Run(ckt);
        }

        [Test]
        public void When_Property_Expect_Reference()
        {
            var op = new OP("op");
            IBuilder<Expression> builder = new ExpressionBuilder(op);
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 2.0),
                new Resistor("R1", "in", "out", 1e3),
                new Resistor("R2", "out", "0", 2e3));
            Func<double> func = null;
            op.AfterSetup += (sender, args) =>
            {
                func = Compile(builder.CreateProperty("R1", "resistance", QuantityTypes.Raw));
            };
            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(1.0e3, func(), 1e-12);
            };
            op.Run(ckt);
        }
    }

    public class ExpressionBuilderTestData
    {
        public static IEnumerable<TestCaseData> SpiceNumbers
        {
            get
            {
                yield return new TestCaseData("2e3", 2e3); // Exponential notation
                yield return new TestCaseData("2e-2", 2e-2); // Exponential notation 2
                yield return new TestCaseData("2.5g", 2.5e9); // Giga
                yield return new TestCaseData("3.6x", 3.6e6); // Mega 1
                yield return new TestCaseData("2.6meg", 2.6e6); // Mega 2
                yield return new TestCaseData("8.3k", 8.3e3); // Kilo
                yield return new TestCaseData("2.9m", 2.9e-3); // Milli
                yield return new TestCaseData("1u", 1e-6); // Micro
                yield return new TestCaseData("0.2n", 0.2e-9); // Nano
                yield return new TestCaseData("2.22p", 2.22e-12); // Pico
                yield return new TestCaseData("3.78f", 3.78e-15); // Femto
                yield return new TestCaseData("3mil", 3 * 25.4e-6); // Mil
            }
        }
        public static IEnumerable<TestCaseData> Combinations
        {
            get
            {
                yield return new TestCaseData(2.0, 2.5);
                yield return new TestCaseData(2.5, 2.0);
                yield return new TestCaseData(0.0, 3.0);
                yield return new TestCaseData(3.0, 0.0);
                yield return new TestCaseData(0.0, 0.0);
            }
        }
    }
}
