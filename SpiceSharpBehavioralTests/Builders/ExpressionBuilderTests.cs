using NUnit.Framework;
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
            Assert.AreEqual(b.Equals(0.0) ? double.PositiveInfinity : a / b, Compile(builder.Divide(ca, cb)).Invoke(), 1e-20);
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

        [TestCaseSource(typeof(ExpressionBuilderTestData), nameof(ExpressionBuilderTestData.Functions1D))]
        public void When_Functions1D_Expect_Reference(string function, double argument, double expected)
        {
            var builder = new ExpressionBuilder { SimplifyConstants = false };
            builder.RegisterFunctions();
            ExpressionBuilderHelper.SimplifyConstants = false;
            Assert.AreEqual(expected, Compile(builder.CreateFunction(function, new[] { Expression.Constant(argument) })).Invoke(), 1e-12);
        }

        [TestCaseSource(typeof(ExpressionBuilderTestData), nameof(ExpressionBuilderTestData.Functions2D))]
        public void When_Functions2D_Expect_Reference(string function, double arg1, double arg2, double expected)
        {
            var builder = new ExpressionBuilder { SimplifyConstants = false };
            builder.RegisterFunctions();
            ExpressionBuilderHelper.SimplifyConstants = false;
            Assert.AreEqual(expected, Compile(builder.CreateFunction(function, new[] { Expression.Constant(arg1), Expression.Constant(arg2) })).Invoke(), 1e-12);
        }

        [TestCaseSource(typeof(ExpressionBuilderTestData), nameof(ExpressionBuilderTestData.Functions1D))]
        public void When_Functions1DSimplified_Expect_Reference(string function, double argument, double expected)
        {
            var builder = new ExpressionBuilder { SimplifyConstants = true };
            builder.RegisterFunctions();
            ExpressionBuilderHelper.SimplifyConstants = true;
            Assert.AreEqual(expected, Compile(builder.CreateFunction(function, new[] { Expression.Constant(argument) })).Invoke(), 1e-12);
        }

        [TestCaseSource(typeof(ExpressionBuilderTestData), nameof(ExpressionBuilderTestData.Functions2D))]
        public void When_Functions2DSimplified_Expect_Reference(string function, double arg1, double arg2, double expected)
        {
            var builder = new ExpressionBuilder { SimplifyConstants = true };
            builder.RegisterFunctions();
            ExpressionBuilderHelper.SimplifyConstants = false;
            Assert.AreEqual(expected, Compile(builder.CreateFunction(function, new[] { Expression.Constant(arg1), Expression.Constant(arg2) })).Invoke(), 1e-12);
        }

        [TestCaseSource(typeof(ExpressionBuilderTestData), nameof(ExpressionBuilderTestData.Voltages))]
        public void When_Voltage_Expect_Reference(string node, string reference, QuantityTypes type, double value)
        {
            var builder = new ExpressionBuilder();
            builder.VoltageFound += (sender, args) =>
            {
                Assert.AreEqual(args.Node, node);
                Assert.AreEqual(args.Reference, reference);
                Assert.AreEqual(args.QuantityType, type);
                args.Result = Expression.Constant(value);
            };
            Assert.AreEqual(value, Compile(builder.CreateVoltage(node, reference, type)).Invoke(), 1e-12);
        }

        [TestCaseSource(typeof(ExpressionBuilderTestData), nameof(ExpressionBuilderTestData.Currents))]
        public void When_Current_Expect_Reference(string name, QuantityTypes type, double value)
        {
            var builder = new ExpressionBuilder();
            builder.CurrentFound += (sender, args) =>
            {
                Assert.AreEqual(args.Name, name);
                Assert.AreEqual(args.QuantityType, type);
                args.Result = Expression.Constant(value);
            };
            Assert.AreEqual(value, Compile(builder.CreateCurrent(name, type)).Invoke(), 1e-12);
        }

        [TestCaseSource(typeof(ExpressionBuilderTestData), nameof(ExpressionBuilderTestData.Properties))]
        public void When_Property_Expect_Reference(string source, string property, QuantityTypes type, double value)
        {
            var builder = new ExpressionBuilder();
            builder.PropertyFound += (sender, args) =>
            {
                Assert.AreEqual(args.Source, source);
                Assert.AreEqual(args.Property, property);
                Assert.AreEqual(args.QuantityType, type);
                args.Result = Expression.Constant(value);
            };
            Assert.AreEqual(value, Compile(builder.CreateProperty(source, property, type)).Invoke(), 1e-12);
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
        public static IEnumerable<TestCaseData> Voltages
        {
            get
            {
                yield return new TestCaseData("in", null, QuantityTypes.Raw, 1.0);
                yield return new TestCaseData("in", "0", QuantityTypes.Real, 0.5);
            }
        }
        public static IEnumerable<TestCaseData> Currents
        {
            get
            {
                yield return new TestCaseData("V1", QuantityTypes.Raw, 1.0);
                yield return new TestCaseData("R1", QuantityTypes.Phase, -1.0);
            }
        }
        public static IEnumerable<TestCaseData> Properties
        {
            get
            {
                yield return new TestCaseData("V1", "acmag", QuantityTypes.Raw, 1.0);
                yield return new TestCaseData("R1", "resistance", QuantityTypes.Real, 0.5);
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
        public static IEnumerable<TestCaseData> Single
        {
            get
            {
                yield return new TestCaseData(2.5);
                yield return new TestCaseData(0.0);
                yield return new TestCaseData(-3.9);
            }
        }
        public static IEnumerable<TestCaseData> Functions1D
        {
            get
            {
                foreach (var data in Single)
                {
                    var arg = (double)data.Arguments[0];
                    yield return new TestCaseData("abs", arg, Math.Abs(arg));
                    yield return new TestCaseData("dabs(0)", arg, (double)Math.Sign(arg));
                    yield return new TestCaseData("sqrt", arg, arg < 0 ? double.PositiveInfinity : Math.Sqrt(arg));
                    yield return new TestCaseData("log", arg, arg < 0.0 ? double.PositiveInfinity : Math.Log(arg));
                    yield return new TestCaseData("dlog(0)", arg, Functions.SafeDivide(1, arg, 1e-20));
                    yield return new TestCaseData("log10", arg, arg < 0 ? double.PositiveInfinity : Math.Log10(arg));
                    yield return new TestCaseData("dlog10(0)", arg, Functions.SafeDivide(1.0 / Math.Log(10.0), arg, 1e-20));
                    yield return new TestCaseData("exp", arg, Math.Exp(arg));
                    yield return new TestCaseData("dexp(0)", arg, Math.Exp(arg));
                    yield return new TestCaseData("sin", arg, Math.Sin(arg));
                    yield return new TestCaseData("dsin(0)", arg, Math.Cos(arg));
                    yield return new TestCaseData("cos", arg, Math.Cos(arg));
                    yield return new TestCaseData("dcos(0)", arg, -Math.Sin(arg));
                    yield return new TestCaseData("tan", arg, Math.Tan(arg));
                    yield return new TestCaseData("dtan(0)", arg, Functions.SafeDivide(1.0, Math.Cos(arg) * Math.Cos(arg), 1e-20));
                    yield return new TestCaseData("asin", arg, Math.Asin(arg));
                    yield return new TestCaseData("dasin(0)", arg, arg < 0 || arg > 1 ? 0.0 : Functions.SafeDivide(1.0, Math.Sqrt(1 - arg * arg), 1e-20));
                    yield return new TestCaseData("acos", arg, Math.Acos(arg));
                    yield return new TestCaseData("dacos(0)", arg, arg < 0 || arg > 1 ? 0.0 : Functions.SafeDivide(-1.0, Math.Sqrt(1 - arg * arg), 1e-20));
                    yield return new TestCaseData("atan", arg, Math.Atan(arg));
                    yield return new TestCaseData("datan(0)", arg, 1.0 / (1.0 + arg * arg));
                    yield return new TestCaseData("sinh", arg, Math.Sinh(arg));
                    yield return new TestCaseData("dsinh(0)", arg, Math.Cosh(arg));
                    yield return new TestCaseData("cosh", arg, Math.Cosh(arg));
                    yield return new TestCaseData("dcosh(0)", arg, Math.Sinh(arg));
                    yield return new TestCaseData("tanh", arg, Math.Tanh(arg));
                    yield return new TestCaseData("dtanh(0)", arg, Functions.SafeDivide(1, Math.Cosh(arg) * Math.Cosh(arg), 1e-20));
                    yield return new TestCaseData("u", arg, Functions.Step(arg));
                    yield return new TestCaseData("du(0)", arg, 0.0);
                    yield return new TestCaseData("u2", arg, Functions.Step2(arg));
                    yield return new TestCaseData("du2(0)", arg, Functions.Step2Derivative(arg));
                    yield return new TestCaseData("uramp", arg, Functions.Ramp(arg));
                    yield return new TestCaseData("duramp(0)", arg, Functions.RampDerivative(arg));
                    yield return new TestCaseData("ceil", arg, Math.Ceiling(arg));
                    yield return new TestCaseData("dceil(0)", arg, 0.0);
                    yield return new TestCaseData("floor", arg, Math.Floor(arg));
                    yield return new TestCaseData("dfloor(0)", arg, 0.0);
                    yield return new TestCaseData("nint", arg, Math.Round(arg, 0));
                    yield return new TestCaseData("square", arg, arg * arg);
                    yield return new TestCaseData("dsquare(0)", arg, 2 * arg);
                }
            }
        }
        public static IEnumerable<TestCaseData> Functions2D
        {
            get
            {
                foreach (var data in Combinations)
                {
                    var arg1 = (double)data.Arguments[0];
                    var arg2 = (double)data.Arguments[1];
                    yield return new TestCaseData("pwr", arg1, arg2, Functions.Power2(arg1, arg2));
                    yield return new TestCaseData("dpwr(0)", arg1, arg2, arg2 * Functions.Power2(arg1, arg2 - 1.0));
                    yield return new TestCaseData("dpwr(1)", arg1, arg2, Functions.Power2(arg1, arg2) * Math.Log(arg1));
                    yield return new TestCaseData("min", arg1, arg2, arg1 < arg2 ? arg1 : arg2);
                    yield return new TestCaseData("max", arg1, arg2, arg1 > arg2 ? arg1 : arg2);
                    if (arg2 >= 0)
                        yield return new TestCaseData("round", arg1, arg2, Math.Round(arg1, (int)arg2));
                }
            }
        }
    }
}
