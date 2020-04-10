using NUnit.Framework;
using SpiceSharp;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioralTests.Builders
{
    public class DoubleBuilderTestData
    {
        public static IEnumerable<TestCaseData> Single
        {
            get
            {
                yield return new TestCaseData(Node.Constant("2.5"), 2.5);
                yield return new TestCaseData(Node.Constant("0"), 0.0);
                yield return new TestCaseData(Node.Minus(Node.Constant("3.9")), -3.9);
            }
        }
        public static IEnumerable<TestCaseData> Nodes
        {
            get
            {
                // Numbers
                yield return new TestCaseData(Node.Constant("2e3"), 2e3); // Exponential notation
                yield return new TestCaseData(Node.Constant("2e-2"), 2e-2); // Exponential notation 2
                yield return new TestCaseData(Node.Constant("2.5g"), 2.5e9); // Giga
                yield return new TestCaseData(Node.Constant("3.6x"), 3.6e6); // Mega 1
                yield return new TestCaseData(Node.Constant("2.6meg"), 2.6e6); // Mega 2
                yield return new TestCaseData(Node.Constant("8.3k"), 8.3e3); // Kilo
                yield return new TestCaseData(Node.Constant("2.9m"), 2.9e-3); // Milli
                yield return new TestCaseData(Node.Constant("1u"), 1e-6); // Micro
                yield return new TestCaseData(Node.Constant("0.2n"), 0.2e-9); // Nano
                yield return new TestCaseData(Node.Constant("2.22p"), 2.22e-12); // Pico
                yield return new TestCaseData(Node.Constant("3.78f"), 3.78e-15); // Femto
                yield return new TestCaseData(Node.Constant("3mil"), 3 * 25.4e-6); // Mil

                // Operators
                yield return new TestCaseData(Node.Minus(Node.Constant("4")), -4.0).SetName("{m}(-4)");
                yield return new TestCaseData(Node.Plus(Node.Constant("2")), 2.0).SetName("{m}(+2)");
                yield return new TestCaseData(Node.Add(Node.Constant("1"), Node.Constant("1")), 2.0).SetName("{m}(1 + 1)");
                yield return new TestCaseData(Node.Subtract(Node.Constant("2"), Node.Constant("5")), -3.0).SetName("{m}(2 - 5)");
                yield return new TestCaseData(Node.Modulo(Node.Constant("5"), Node.Constant("2")), 1.0).SetName("{m}(5 % 2)");
                yield return new TestCaseData(Node.Add(Node.Multiply(Node.Constant("2"), Node.Constant("3")), Node.Multiply(Node.Constant("4"), Node.Constant("5"))), 26.0).SetName("{m}(2*3+4*5)");
                yield return new TestCaseData(Node.Divide(Node.Multiply(Node.Constant("4"), Node.Constant("5")), Node.Constant("3")), 20.0 / 3.0).SetName("{m}(4*5/3)");
                yield return new TestCaseData(Node.Add(Node.Constant("5"), Node.Minus(Node.Minus(Node.Constant("3")))), 8.0).SetName("{m}(5+--3)");
                yield return new TestCaseData(Node.Power(Node.Constant("3"), Node.Constant("2")), 9.0).SetName("{m}(3^2)");
                yield return new TestCaseData(Node.Multiply(Node.Multiply(Node.Constant("2"), Node.Add(Node.Constant("3"), Node.Constant("4"))), Node.Constant("5")), 70.0).SetName("{m}(2*[3+4]*5)");
                yield return new TestCaseData(Node.GreaterThan(Node.Constant("5"), Node.Constant("3")), 1.0).SetName("{m}(5>3)");
                yield return new TestCaseData(Node.LessThan(Node.Constant("5"), Node.Constant("3")), 0.0).SetName("{m}(5<3)");
                yield return new TestCaseData(Node.GreaterThanOrEqual(Node.Constant("5"), Node.Constant("5")), 1.0).SetName("{m}(5>=5)");
                yield return new TestCaseData(Node.LessThanOrEqual(Node.Constant("5"), Node.Constant("5")), 1.0).SetName("{m}(5<=5)");
                yield return new TestCaseData(Node.Equals(Node.Constant("1"), Node.Constant("2")), 0.0).SetName("{m}(1==2)");
                yield return new TestCaseData(Node.NotEquals(Node.Constant("1"), Node.Constant("2")), 1.0).SetName("{m}(1!=2)");
            }
        }
        public static IEnumerable<TestCaseData> FunctionNodes
        {
            get
            {
                double fudgeFactor = 1e-20;

                // Test some execution of arguments
                yield return new TestCaseData(Node.Function("abs", Node.Add(Node.Multiply(Node.Minus(Node.Constant("2")), Node.Constant("6")), Node.Constant("7"))), 5.0).SetName("{m}(abs -2*6+7)");
                yield return new TestCaseData(Node.Function("min", Node.Minus(Node.Constant("2")), Node.Multiply(Node.Constant("6"), Node.Constant("2"))), -2.0).SetName("{m}(min -2, 6*2)");
                yield return new TestCaseData(Node.Function("pwl", Node.Constant("2"), Node.Constant("0"), Node.Constant("1"), Node.Constant("3"), Node.Constant("4")), 3.0).SetName("{m}(pwl 2,0,1,3,4)");

                foreach (var data in Single)
                {
                    var node = (Node)data.Arguments[0];
                    var arg = (double)data.Arguments[1];
                    yield return new TestCaseData(Node.Function("abs", node), Math.Abs(arg)).SetName("{{m}}(abs {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("sqrt", node), Functions.Sqrt(arg)).SetName("{{m}}(sqrt {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("log", node), Functions.Log(arg)).SetName("{{m}}(log {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("log10", node), Functions.Log10(arg)).SetName("{{m}}(log10 {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("exp", node), Math.Exp(arg)).SetName("{{m}}(exp {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("sin", node), Math.Sin(arg)).SetName("{{m}}(sin {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("cos", node), Math.Cos(arg)).SetName("{{m}}(cos {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("tan", node), Math.Tan(arg)).SetName("{{m}}(tan {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("asin", node), Math.Asin(arg)).SetName("{{m}}(asin {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("acos", node), Math.Acos(arg)).SetName("{{m}}(acos {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("atan", node), Math.Atan(arg)).SetName("{{m}}(atan {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("sinh", node), Math.Sinh(arg)).SetName("{{m}}(sinh {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("cosh", node), Math.Cosh(arg)).SetName("{{m}}(cosh {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("tanh", node), Math.Tanh(arg)).SetName("{{m}}(tanh {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("u", node), Functions.Step(arg)).SetName("{{m}}(u {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("u2", node), Functions.Step2(arg)).SetName("{{m}}(u2 {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("uramp", node), Functions.Ramp(arg)).SetName("{{m}}(uramp {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("ceil", node), Math.Ceiling(arg)).SetName("{{m}}(ceil {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("floor", node), Math.Floor(arg)).SetName("{{m}}(floor {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("nint", node), Math.Round(arg, 0)).SetName("{{m}}(nint {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("square", node), arg * arg).SetName("{{m}}(square {0})".FormatString(arg));
                }
            }
        }
    }
}
