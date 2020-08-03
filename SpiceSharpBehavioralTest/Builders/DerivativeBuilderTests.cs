using NUnit.Framework;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioralTests.Builders
{
    [TestFixture]
    public class DerivativeBuilderTests
    {
        [TestCaseSource(typeof(DerivativesBuilderTestData), nameof(DerivativesBuilderTestData.Combinations))]
        public void When_Arithmetic_Expect_Reference(double a, double b)
        {
            IBuilder<Derivatives<double>> builder = new DerivativeBuilder<double>(new DoubleBuilder());
            Assert.AreEqual(+a, builder.Plus(a).Value, 1e-20);
            Assert.AreEqual(-a, builder.Minus(a).Value, 1e-20);
            Assert.AreEqual(a + b, builder.Add(a, b).Value, 1e-20);
            Assert.AreEqual(a - b, builder.Subtract(a, b).Value, 1e-20);
            Assert.AreEqual(a * b, builder.Multiply(a, b).Value, 1e-20);
            Assert.AreEqual(a / b, builder.Divide(a, b).Value, 1e-20);
            Assert.AreEqual(Math.Pow(a, b), builder.Pow(a, b).Value, 1e-20);
            Assert.AreEqual(a % b, builder.Mod(a, b).Value, 1e-20);
            Assert.AreEqual(a != 0.0 && b != 0.0 ? 1.0 : 0.0, builder.And(a, b).Value, 1e-20);
            Assert.AreEqual(a != 0.0 || b != 0.0 ? 1.0 : 0.0, builder.Or(a, b).Value, 1e-20);
            Assert.AreEqual(a != 0.0 ? a : b, builder.Conditional(a, a, b).Value, 1e-20);
            Assert.AreEqual(a == b ? 1.0 : 0.0, builder.Equals(a, b).Value, 1e-20);
            Assert.AreEqual(a != b ? 1.0 : 0.0, builder.NotEquals(a, b).Value, 1e-20);
            Assert.AreEqual(a > b ? 1.0 : 0.0, builder.GreaterThan(a, b).Value, 1e-20);
            Assert.AreEqual(a >= b ? 1.0 : 0.0, builder.GreaterThanOrEqual(a, b).Value, 1e-20);
            Assert.AreEqual(a < b ? 1.0 : 0.0, builder.LessThan(a, b).Value, 1e-20);
            Assert.AreEqual(a <= b ? 1.0 : 0.0, builder.LessThanOrEqual(a, b).Value, 1e-20);
        }

        [TestCaseSource(typeof(DerivativesBuilderTestData), nameof(DerivativesBuilderTestData.Functions1D))]
        public void When_Function1D_Expect_Reference(string func, double arg, double darg, double expected, double dexpected)
        {
            var base_builder = new DoubleBuilder();
            var builder = new DerivativeBuilder<double>(base_builder);
            var x = new Variable("x");
            var derivative = new Derivatives<double> { Value = arg };
            derivative[x] = darg;

            var result = ((IBuilder<Derivatives<double>>)builder).CreateFunction(func, new[] { derivative });
            Assert.AreEqual(expected, result.Value, 1e-12);
            Assert.AreEqual(dexpected, result[x], 1e-12);
        }
    }

    public class DerivativesBuilderTestData
    {
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
        public static IEnumerable<TestCaseData> Functions1D
        {
            get
            {
                foreach (var data in Combinations)
                {
                    var arg = (double)data.Arguments[0];
                    var darg = (double)data.Arguments[1];
                    yield return new TestCaseData("log", arg, darg, Math.Log(arg), darg / arg);
                    yield return new TestCaseData("exp", arg, darg, Math.Exp(arg), Math.Exp(arg) * darg);
                    yield return new TestCaseData("sin", arg, darg, Math.Sin(arg), Math.Cos(arg) * darg);
                }
            }
        }
    }
}
