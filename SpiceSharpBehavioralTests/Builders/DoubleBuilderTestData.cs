﻿using NUnit.Framework;
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
                yield return new TestCaseData(Node.Constant(2.5), 2.5);
                yield return new TestCaseData(Node.Constant(0.0), 0.0);
                yield return new TestCaseData(Node.Minus(3.9), -3.9);
            }
        }
        public static IEnumerable<TestCaseData> Nodes
        {
            get
            {
                // Operators
                yield return new TestCaseData(Node.Minus(4.0), -4.0).SetName("{m}(-4)");
                yield return new TestCaseData(Node.Plus(2.0), 2.0).SetName("{m}(+2)");
                yield return new TestCaseData(Node.Add(1.0, 1.0), 2.0).SetName("{m}(1 + 1)");
                yield return new TestCaseData(Node.Subtract(2.0, 5.0), -3.0).SetName("{m}(2 - 5)");
                yield return new TestCaseData(Node.Modulo(5.0, 2.0), 1.0).SetName("{m}(5 % 2)");
                yield return new TestCaseData(Node.Add(Node.Multiply(2.0, 3.0), Node.Multiply(4.0, 5.0)), 26.0).SetName("{m}(2*3+4*5)");
                yield return new TestCaseData(Node.Divide(Node.Multiply(4.0, 5.0), 3.0), 20.0 / 3.0).SetName("{m}(4*5/3)");
                yield return new TestCaseData(Node.Add(5.0, Node.Minus(Node.Minus(3.0))), 8.0).SetName("{m}(5+--3)");
                yield return new TestCaseData(Node.Power(3.0, 2.0), 9.0).SetName("{m}(3^2)");
                yield return new TestCaseData(Node.Multiply(Node.Multiply(2.0, Node.Add(3.0, 4.0)), 5.0), 70.0).SetName("{m}(2*[3+4]*5)");
                yield return new TestCaseData(Node.GreaterThan(5.0, 3.0), 1.0).SetName("{m}(5>3)");
                yield return new TestCaseData(Node.LessThan(5.0, 3.0), 0.0).SetName("{m}(5<3)");
                yield return new TestCaseData(Node.GreaterThanOrEqual(5.0, 5.0), 1.0).SetName("{m}(5>=5)");
                yield return new TestCaseData(Node.LessThanOrEqual(5.0, 5.0), 1.0).SetName("{m}(5<=5)");
                yield return new TestCaseData(Node.Equals(1.0, 2.0), 0.0).SetName("{m}(1==2)");
                yield return new TestCaseData(Node.NotEquals(1.0, 2.0), 1.0).SetName("{m}(1!=2)");
            }
        }
        public static IEnumerable<TestCaseData> FunctionNodes
        {
            get
            {
                // Test some execution of arguments
                yield return new TestCaseData(Node.Function("abs", Node.Add(Node.Multiply(Node.Minus(2.0), 6.0), 7.0)), 5.0).SetName("{m}(abs -2*6+7)");
                yield return new TestCaseData(Node.Function("min", Node.Minus(2.0), Node.Multiply(6.0, 2.0)), -2.0).SetName("{m}(min -2, 6*2)");
                yield return new TestCaseData(Node.Function("pwl", 2.0, 0.0, 1.0, 3.0, 4.0), 3.0).SetName("{m}(pwl 2,0,1,3,4)");

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