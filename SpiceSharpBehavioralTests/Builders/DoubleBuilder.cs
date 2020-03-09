using NUnit.Framework;
using SpiceSharp;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioralTests.Builders
{
    /// <summary>
    /// A simple implementation for a builder using doubles for testing.
    /// </summary>
    /// <seealso cref="IBuilder{T}" />
    public class DoubleBuilder : IBuilder<double>
    {
        double IBuilder<double>.Add(double left, double right) => left + right;
        double IBuilder<double>.And(double left, double right) => left.Equals(0.0) || right.Equals(0.0) ? 0.0 : 1.0;
        double IBuilder<double>.Conditional(double condition, double ifTrue, double ifFalse) => condition.Equals(0.0) ? ifFalse : ifTrue;
        double IBuilder<double>.CreateNumber(string value) => double.Parse(value);
        double IBuilder<double>.CreateFunction(string name, IReadOnlyList<double> arguments)
        {
            switch (name)
            {
                case "abs": Assert.AreEqual(1, arguments.Count); return Math.Abs(arguments[0]);
                case "sqrt": Assert.AreEqual(1, arguments.Count); return Math.Sqrt(arguments[0]);
                case "pwr": Assert.AreEqual(2, arguments.Count); return Functions.Power2(arguments[0], arguments[1]);
                case "min": Assert.AreEqual(2, arguments.Count); return Math.Min(arguments[0], arguments[1]);
                case "max": Assert.AreEqual(2, arguments.Count); return Math.Max(arguments[0], arguments[1]);
                case "log": Assert.AreEqual(1, arguments.Count); return Math.Log(arguments[0]);
                case "dlog(0)": Assert.AreEqual(1, arguments.Count); return 1.0 / arguments[0];
                case "log10": Assert.AreEqual(1, arguments.Count); return Math.Log10(arguments[0]);
                case "exp": Assert.AreEqual(1, arguments.Count); return Math.Exp(arguments[0]);
                case "dexp(0)": Assert.AreEqual(1, arguments.Count); return Math.Exp(arguments[0]);
                case "sin": Assert.AreEqual(1, arguments.Count); return Math.Sin(arguments[0]);
                case "dsin(0)": Assert.AreEqual(1, arguments.Count); return Math.Cos(arguments[0]);
                case "cos": Assert.AreEqual(1, arguments.Count); return Math.Cosh(arguments[0]);
                case "tan": Assert.AreEqual(1, arguments.Count); return Math.Tan(arguments[0]);
                case "asin": Assert.AreEqual(1, arguments.Count); return Math.Asin(arguments[0]);
                case "acos": Assert.AreEqual(1, arguments.Count); return Math.Acos(arguments[0]);
                case "atan": Assert.AreEqual(1, arguments.Count); return Math.Atan(arguments[0]);
                case "sinh": Assert.AreEqual(1, arguments.Count); return Math.Sinh(arguments[0]);
                case "cosh": Assert.AreEqual(1, arguments.Count); return Math.Cosh(arguments[0]);
                case "tanh": Assert.AreEqual(1, arguments.Count); return Math.Tanh(arguments[0]);
                case "u": Assert.AreEqual(1, arguments.Count); return Functions.Step(arguments[0]);
                case "u2": Assert.AreEqual(1, arguments.Count); return Functions.Step2(arguments[0]);
                case "du2(0)": Assert.AreEqual(1, arguments.Count); return Functions.Step2Derivative(arguments[0]);
                case "uramp": Assert.AreEqual(1, arguments.Count); return Functions.Ramp(arguments[0]);
                case "duramp(0)": Assert.AreEqual(1, arguments.Count); return Functions.RampDerivative(arguments[0]);
                case "ceil": Assert.AreEqual(1, arguments.Count); return Math.Ceiling(arguments[0]);
                case "floor": Assert.AreEqual(1, arguments.Count); return Math.Floor(arguments[0]);
                case "nint": Assert.AreEqual(1, arguments.Count); return Math.Round(arguments[0], 0);
                case "round": Assert.AreEqual(2, arguments.Count); return Math.Round(arguments[0], (int)arguments[1]);
                case "square": Assert.AreEqual(1, arguments.Count); return Functions.Square(arguments[0]);
                case "pwl":
                case "dpwl(0)":
                    int points = (arguments.Count - 1) / 2;
                    var arr = new Point[points];
                    for (var i = 0; i < points; i++)
                        arr[i] = new Point(arguments[i * 2 + 1], arguments[i * 2 + 2]);
                    if (name == "pwl")
                        return Functions.Pwl(arguments[0], arr);
                    return Functions.PwlDerivative(arguments[0], arr);
                default:
                    throw new Exception("Unrecognized function {0}".FormatString(name));
            }
        }
        double IBuilder<double>.Divide(double left, double right) => left / right;
        double IBuilder<double>.Equals(double left, double right) => left == right ? 1.0 : 0.0;
        double IBuilder<double>.NotEquals(double left, double right) => left != right ? 1.0 : 0.0;
        double IBuilder<double>.GreaterThanOrEqual(double left, double right) => left >= right ? 1.0 : 0.0;
        double IBuilder<double>.GreaterThan(double left, double right) => left > right ? 1.0 : 0.0;
        double IBuilder<double>.LessThanOrEqual(double left, double right) => left <= right ? 1.0 : 0.0;
        double IBuilder<double>.LessThan(double left, double right) => left < right ? 1.0 : 0.0;
        double IBuilder<double>.Minus(double argument) => -argument;
        double IBuilder<double>.Mod(double left, double right) => left % right;
        double IBuilder<double>.Multiply(double left, double right) => left * right;
        double IBuilder<double>.Or(double left, double right) => left.Equals(0.0) && right.Equals(0.0) ? 0.0 : 1.0;
        double IBuilder<double>.Plus(double argument) => argument;
        double IBuilder<double>.Pow(double left, double right) => Math.Pow(left, right);
        double IBuilder<double>.Subtract(double left, double right) => left - right;

        double IBuilder<double>.CreateVariable(string name, QuantityTypes type)
        {
            throw new NotImplementedException();
        }
        double IBuilder<double>.CreateVoltage(string node, string reference, QuantityTypes type)
        {
            throw new NotImplementedException();
        }
        double IBuilder<double>.CreateCurrent(string name, QuantityTypes type)
        {
            throw new NotImplementedException();
        }
        double IBuilder<double>.CreateProperty(string name, string property, QuantityTypes type)
        {
            throw new NotImplementedException();
        }
    }
}
