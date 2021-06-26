using NUnit.Framework;
using SpiceSharp;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharpBehavioralTest.Builders
{
    public class BuilderTestData
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
                yield return new TestCaseData(Node.Not(0.6), 0.0).SetName("{m}(!0.6)");
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
                yield return new TestCaseData(Node.Or(1.0, 0.0), 1.0).SetName("{m}(1|0)");
                yield return new TestCaseData(Node.And(1.0, 0.0), 0.0).SetName("{m}(1&0)");
                yield return new TestCaseData(Node.Xor(1.0, 0.0), 1.0).SetName("{m}(1^0)");
                yield return new TestCaseData(Node.Conditional(1.0, 0.25, 0.75), 0.25).SetName("{m}(1 ? 0.25 : 0.75)");
                yield return new TestCaseData(Node.Conditional(0.0, 0.25, 0.75), 0.75).SetName("{m}(0 ? 0.25 : 0.75)");
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
                yield return new TestCaseData(Node.Function("pwl", -1.0, 0.0, 1.0, 3.0, 4.0), 1.0).SetName("{m}(pwl -1,0,1,3,4)");
                yield return new TestCaseData(Node.Function("pwl", 4.0, 0.0, 1.0, 3.0, 4.0), 4.0).SetName("{m}(pwl 4,0,1,3,4)");
                yield return new TestCaseData(Node.Function("table", 2.0, 0.0, 1.0, 3.0, 4.0), 3.0).SetName("{m}(table 2,0,1,3,4)");
                yield return new TestCaseData(Node.Function("tbl", 2.0, 0.0, 1.0, 3.0, 4.0), 3.0).SetName("{m}(tbl 2,0,1,3,4)");
                yield return new TestCaseData(Node.Function("if", 1.0, 0.25, 0.75), 0.25).SetName("{m}(if 1 0.25 0.75)");
                yield return new TestCaseData(Node.Function("if", 0.0, 0.25, 0.75), 0.75).SetName("{m}(if 0 0.25 0.75)");
                yield return new TestCaseData(Node.Function("limit", 0.4, 0.25, 0.75), 0.4).SetName("{m}(limit 0.4 0.25 0.75)");
                yield return new TestCaseData(Node.Function("limit", 0.4, 0.25, 0.3), 0.3).SetName("{m}(limit 0.4 0.25 0.3)");
                yield return new TestCaseData(Node.Function("limit", 0.4, 0.5, 0.7), 0.5).SetName("{m}(limit 0.4 0.5 0.7)");
                yield return new TestCaseData(Node.Function("limit", 0.4, 0.7, 0.5), 0.5).SetName("{m}(limit 0.4 0.7 0.5)");


                foreach (var data in Single)
                {
                    var node = (Node)data.Arguments[0];
                    var arg = (double)data.Arguments[1];
                    yield return new TestCaseData(Node.Function("abs", node), Math.Abs(arg)).SetName("{{m}}(abs {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("sqrt", node), HelperFunctions.Sqrt(arg)).SetName("{{m}}(sqrt {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("ln", node), HelperFunctions.Log(arg)).SetName("{{m}}(ln {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("log", node), HelperFunctions.Log(arg)).SetName("{{m}}(log {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("log10", node), HelperFunctions.Log10(arg)).SetName("{{m}}(log10 {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("exp", node), Math.Exp(arg)).SetName("{{m}}(exp {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("sin", node), Math.Sin(arg)).SetName("{{m}}(sin {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("cos", node), Math.Cos(arg)).SetName("{{m}}(cos {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("tan", node), Math.Tan(arg)).SetName("{{m}}(tan {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("asin", node), Math.Asin(arg)).SetName("{{m}}(asin {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("acos", node), Math.Acos(arg)).SetName("{{m}}(acos {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("atan", node), Math.Atan(arg)).SetName("{{m}}(atan {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("atanh", node), HelperFunctions.Atanh(arg)).SetName("{{m}}(atanh {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("sinh", node), Math.Sinh(arg)).SetName("{{m}}(sinh {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("cosh", node), Math.Cosh(arg)).SetName("{{m}}(cosh {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("tanh", node), Math.Tanh(arg)).SetName("{{m}}(tanh {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("u", node), HelperFunctions.Step(arg)).SetName("{{m}}(u {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("u2", node), HelperFunctions.Step2(arg)).SetName("{{m}}(u2 {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("uramp", node), HelperFunctions.Ramp(arg)).SetName("{{m}}(uramp {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("ceil", node), Math.Ceiling(arg)).SetName("{{m}}(ceil {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("floor", node), Math.Floor(arg)).SetName("{{m}}(floor {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("nint", node), Math.Round(arg, 0)).SetName("{{m}}(nint {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("square", node), arg * arg).SetName("{{m}}(square {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("pow", node, node + 1.5), Math.Pow(arg, arg + 1.5)).SetName("{{m}}(pow {0} {1})".FormatString(arg, arg + 1.5));
                    yield return new TestCaseData(Node.Function("pwr", node, node + 1.5), HelperFunctions.Power(arg, arg + 1.5)).SetName("{{m}}(pwr {0} {1})".FormatString(arg, arg + 1.5));
                    yield return new TestCaseData(Node.Function("pwrs", node, node + 1.5), HelperFunctions.Power2(arg, arg + 1.5)).SetName("{{m}}(pwrs {0} {1})".FormatString(arg, arg + 1.5));
                    yield return new TestCaseData(Node.Function("atan2", node, node - 5.0), Math.Atan2(arg, arg - 5)).SetName("{{m}}(atan2 {0} {1})".FormatString(arg, arg - 5));
                    yield return new TestCaseData(Node.Function("hypot", node, node - 5.0), HelperFunctions.Hypot(arg, arg - 5)).SetName("{{m}}(hypot {0} {1})".FormatString(arg, arg - 5));
                    yield return new TestCaseData(Node.Function("db", node), 20 * HelperFunctions.Log10(arg)).SetName("{{m}}(db {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("arg", node), 0.0).SetName("{{m}}(arg {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("real", node), arg).SetName("{{m}}(real {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("imag", node), 0.0).SetName("{{m}}(imag {0})".FormatString(arg));
                }
            }
        }

        public static IEnumerable<TestCaseData> ComplexFunctionNodes
        {
            get
            {
                // Test some execution of arguments
                yield return new TestCaseData(Node.Function("abs", Node.Add(Node.Multiply(Node.Minus(2.0), 6.0), 7.0)), new Complex(5, 0)).SetName("{m}(abs -2*6+7)");
                yield return new TestCaseData(Node.Function("min", Node.Minus(2.0), Node.Multiply(6, 2)), new Complex(-2.0, 0)).SetName("{m}(min -2, 6*2)");
                yield return new TestCaseData(Node.Function("pwl", 2.0, 0.0, 1.0, 3.0, 4.0), new Complex(3, 0)).SetName("{m}(pwl 2,0,1,3,4)");
                yield return new TestCaseData(Node.Function("table", 2.0, 0.0, 1.0, 3.0, 4.0), new Complex(3, 0)).SetName("{m}(table 2,0,1,3,4)");
                yield return new TestCaseData(Node.Function("tbl", 2.0, 0.0, 1.0, 3.0, 4.0), new Complex(3, 0)).SetName("{m}(tbl 2,0,1,3,4)");
                yield return new TestCaseData(Node.Function("if", 1.0, 0.25, 0.75), new Complex(0.25, 0)).SetName("{m}(if 1 0.25 0.75)");
                yield return new TestCaseData(Node.Function("if", 0.0, 0.25, 0.75), new Complex(0.75, 0)).SetName("{m}(if 0 0.25 0.75)");
                yield return new TestCaseData(Node.Function("limit", 0.4, 0.25, 0.3), new Complex(0.3, 0)).SetName("{m}(limit 0.4 0.25 0.3)");
                yield return new TestCaseData(Node.Function("limit", 0.4, 0.5, 0.7), new Complex(0.5, 0)).SetName("{m}(limit 0.4 0.5 0.7)");
                yield return new TestCaseData(Node.Function("limit", 0.4, 0.7, 0.5), new Complex(0.5, 0)).SetName("{m}(limit 0.4 0.7 0.5)");

                foreach (var data in Single)
                {
                    var node = (Node)data.Arguments[0];
                    var arg = new Complex((double)data.Arguments[1], 0);
                    yield return new TestCaseData(Node.Function("abs", node), (Complex)arg.Magnitude).SetName("{{m}}(abs {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("sqrt", node), HelperFunctions.Sqrt(arg)).SetName("{{m}}(sqrt {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("ln", node), HelperFunctions.Log(arg)).SetName("{{m}}(ln {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("log", node), HelperFunctions.Log(arg)).SetName("{{m}}(log {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("log10", node), HelperFunctions.Log10(arg)).SetName("{{m}}(log10 {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("exp", node), Complex.Exp(arg)).SetName("{{m}}(exp {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("sin", node), Complex.Sin(arg)).SetName("{{m}}(sin {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("cos", node), Complex.Cos(arg)).SetName("{{m}}(cos {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("tan", node), Complex.Tan(arg)).SetName("{{m}}(tan {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("asin", node), Complex.Asin(arg)).SetName("{{m}}(asin {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("acos", node), Complex.Acos(arg)).SetName("{{m}}(acos {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("atan", node), Complex.Atan(arg)).SetName("{{m}}(atan {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("atanh", node), HelperFunctions.Atanh(arg)).SetName("{{m}}(atanh {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("sinh", node), Complex.Sinh(arg)).SetName("{{m}}(sinh {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("cosh", node), Complex.Cosh(arg)).SetName("{{m}}(cosh {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("tanh", node), Complex.Tanh(arg)).SetName("{{m}}(tanh {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("u", node), HelperFunctions.Step(arg)).SetName("{{m}}(u {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("u2", node), HelperFunctions.Step2(arg)).SetName("{{m}}(u2 {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("uramp", node), HelperFunctions.Ramp(arg)).SetName("{{m}}(uramp {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("ceil", node), new Complex(Math.Ceiling(arg.Real), 0)).SetName("{{m}}(ceil {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("floor", node), new Complex(Math.Floor(arg.Real), 0)).SetName("{{m}}(floor {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("nint", node), new Complex(Math.Round(arg.Real, 0), 0)).SetName("{{m}}(nint {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("square", node), arg * arg).SetName("{{m}}(square {0})".FormatString(arg));
                    yield return new TestCaseData(Node.Function("pow", node + 0.01, node + 1.51), Complex.Pow(arg + 0.01, arg + 1.51)).SetName("{{m}}(pow {0} {1})".FormatString(arg + 0.01, arg + 1.51));
                    yield return new TestCaseData(Node.Function("pwr", node, node + 1.5), HelperFunctions.Power(arg, arg + 1.5)).SetName("{{m}}(pwr {0} {1})".FormatString(arg, arg + 1.5));
                    yield return new TestCaseData(Node.Function("pwrs", node, node + 1.5), HelperFunctions.Power2(arg, arg + 1.5)).SetName("{{m}}(pwrs {0} {1})".FormatString(arg, arg + 1.5));
                    yield return new TestCaseData(Node.Function("atan2", node, node - 5.0), new Complex(Math.Atan2(arg.Real, arg.Real - 5), 0)).SetName("{{m}}(atan2 {0} {1})".FormatString(arg, arg - 5));
                    yield return new TestCaseData(Node.Function("hypot", node, node - 5.0), HelperFunctions.Hypot(arg, arg - 5)).SetName("{{m}}(hypot {0} {1})".FormatString(arg, arg - 5));
                }
            }
        }
    }
}
