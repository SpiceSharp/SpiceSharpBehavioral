using NUnit.Framework;
using SpiceSharpBehavioral.Parsers;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioralTests.Parsers
{
    public class DoubleBuilder : IBuilder<double>
    {
        double IBuilder<double>.Add(double left, double right) => left + right;
        double IBuilder<double>.And(double left, double right) => left.Equals(0.0) || right.Equals(0.0) ? 0.0 : 1.0;
        double IBuilder<double>.Conditional(double condition, double ifTrue, double ifFalse) => condition.Equals(0.0) ? ifFalse : ifTrue;
        double IBuilder<double>.CreateNumber(string value) => double.Parse(value);
        double IBuilder<double>.CreateVariable(string name)
        {
            throw new NotImplementedException();
        }
        double IBuilder<double>.CreateFunction(string name, IReadOnlyList<double> arguments)
        {
            switch (name)
            {
                case "abs":
                    Assert.AreEqual(1, arguments.Count);
                    return Math.Abs(arguments[0]);
                case "min":
                    Assert.AreEqual(2, arguments.Count);
                    return Math.Min(arguments[0], arguments[1]);
                default:
                    throw new Exception("Could not find function");
            }
        }
        double IBuilder<double>.Divide(double left, double right) => left / right;
        double IBuilder<double>.Equals(double left, double right) => left == right ? 1.0 : 0.0;
        double IBuilder<double>.NotEquals(double left, double right) => left != right ? 1.0 : 0.0;
        double IBuilder<double>.GreaterThanOrEqual(double left, double right) => left <= right ? 1.0 : 0.0;
        double IBuilder<double>.GreaterThan(double left, double right) => left < right ? 1.0 : 0.0;
        double IBuilder<double>.LessThanOrEqual(double left, double right) => left <= right ? 1.0 : 0.0;
        double IBuilder<double>.LessThan(double left, double right) => left < right ? 1.0 : 0.0;
        double IBuilder<double>.Minus(double argument) => -argument;
        double IBuilder<double>.Mod(double left, double right) => left % right;
        double IBuilder<double>.Multiply(double left, double right) => left * right;
        double IBuilder<double>.Or(double left, double right) => left.Equals(0.0) && right.Equals(0.0) ? 1.0 : 0.0;
        double IBuilder<double>.Plus(double argument) => argument;
        double IBuilder<double>.Pow(double left, double right) => Math.Pow(left, right);
        double IBuilder<double>.Subtract(double left, double right) => left - right;
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
