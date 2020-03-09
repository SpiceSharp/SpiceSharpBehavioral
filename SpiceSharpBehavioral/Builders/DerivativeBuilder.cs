using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// A builder for derivatives.
    /// </summary>
    /// <remarks>
    /// If the base <see cref="IBuilder{T}"/> implements functions of the format d...(...), then this
    /// builder will call them automatically to calculate derivatives. For example, when the base builder
    /// implements "exp(x)" and "dexp(0)", then this builder will automatically invoke "dexp(0)" to calculate
    /// the total derivative. Functions with multiple arguments (eg. "pow(x,y)") should implement multiple derivatives
    /// ("dpow(0)" and "dpow(1)") where the function is differentiated to each argument separately.
    /// The base builder should also be able to create a number from "0" and "1".
    /// </remarks>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IBuilder{T}" />
    /// <seealso cref="Derivatives{T}"/>
    public class DerivativeBuilder<T> : BaseBuilder<Derivatives<T>>, IBuilder<Derivatives<T>>
    {
        private readonly IBuilder<T> _builder;
        private readonly T _zero;
        private readonly T _one;

        /// <summary>
        /// Gets or sets the simulation.
        /// </summary>
        /// <value>
        /// The simulation.
        /// </value>
        public ISimulation Simulation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DerivativeBuilder{T}"/> class.
        /// </summary>
        /// <param name="builder">The builder for the underlying methods.</param>
        public DerivativeBuilder(IBuilder<T> builder)
        {
            _builder = builder.ThrowIfNull(nameof(builder));
            _zero = _builder.CreateNumber("0");
            _one = _builder.CreateNumber("1");
        }

        /// <summary>
        /// Creates a function call.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The result of the function call.
        /// </returns>
        public override Derivatives<T> CreateFunction(string name, IReadOnlyList<Derivatives<T>> arguments)
        {
            var argValues = new T[arguments.Count];
            for (var i = 0; i < arguments.Count; i++)
                argValues[i] = arguments[i].Value;
            Derivatives<T> result = new Derivatives<T>() { Value = _builder.CreateFunction(name, argValues) };

            switch (name)
            {
                case "min": // x < y ? dx : dy
                    result.Combine(arguments[0], arguments[1],
                        (df, dg) => _builder.Conditional(_builder.LessThan(arguments[0].Value, arguments[1].Value), df, dg),
                        df => _builder.Conditional(_builder.LessThan(arguments[0].Value, arguments[1].Value), df, _zero),
                        dg => _builder.Conditional(_builder.LessThan(arguments[0].Value, arguments[1].Value), _zero, dg));
                    break;
                case "max": // x > y ? dx : dy
                    result.Combine(arguments[0], arguments[1],
                        (df, dg) => _builder.Conditional(_builder.GreaterThan(arguments[0].Value, arguments[1].Value), df, dg),
                        df => _builder.Conditional(_builder.GreaterThan(arguments[0].Value, arguments[1].Value), df, _zero),
                        dg => _builder.Conditional(_builder.GreaterThan(arguments[0].Value, arguments[1].Value), _zero, dg));
                    break;
                default:
                    // Try finding the methods in the base builder
                    for (var i = 0; i < arguments.Count; i++)
                    {
                        var factor = _builder.CreateFunction("d{0}({1})".FormatString(name, i), argValues);
                        foreach (var pair in arguments[i])
                        {
                            if (result.Contains(pair.Key))
                                result[pair.Key] = _builder.Add(result[pair.Key], _builder.Multiply(factor, pair.Value));
                            else
                                result[pair.Key] = _builder.Multiply(factor, pair.Value);
                        }
                    }
                    break;
            }
            return result;
        }

        /// <summary>
        /// Creates the value of a number.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The value of the number.
        /// </returns>
        public override Derivatives<T> CreateNumber(string value)
        {
            return new Derivatives<T> { Value = _builder.CreateNumber(value) };
        }

        /// <summary>
        /// Creates the value of a voltage.
        /// </summary>
        /// <param name="node">The name of the node.</param>
        /// <param name="reference">The name of the reference node.</param>
        /// <param name="type">The quantity type.</param>
        /// <returns>
        /// The value of the voltage.
        /// </returns>
        public override Derivatives<T> CreateVoltage(string node, string reference, QuantityTypes type)
        {
            if (Simulation != null)
            {
                if (type == QuantityTypes.Raw)
                {
                    var result = new Derivatives<T> { Value = _builder.CreateVoltage(node, reference, type) };
                    var v = Simulation.Variables.MapNode(node, VariableType.Voltage);
                    result[v] = _one;
                    if (reference != null)
                    {
                        v = Simulation.Variables.MapNode(reference, VariableType.Voltage);
                        result[v] = _builder.Minus(_one);
                    }
                    return result;
                }
                else
                    throw new Exception("Cannot derive");
            }
            else
                return base.CreateVoltage(node, reference, type);
        }

        /// <summary>
        /// Creates the value of a current.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="type">The quantity type.</param>
        /// <returns>
        /// The value of the current.
        /// </returns>
        public override Derivatives<T> CreateCurrent(string name, QuantityTypes type)
        {
            if (Simulation != null)
            {
                if (type == QuantityTypes.Raw)
                {
                    var behaviors = Simulation.EntityBehaviors[name];
                    if (behaviors.TryGetValue(out IBranchedBehavior behavior))
                    {
                        var result = new Derivatives<T> { Value = _builder.CreateCurrent(name, type) };
                        result[behavior.Branch] = _one;
                        return result;
                    }
                    return base.CreateCurrent(name, type);
                }
                else
                    throw new Exception("Cannot derive");
            }
            else
                return base.CreateCurrent(name, type);
        }

        Derivatives<T> IBuilder<Derivatives<T>>.Plus(Derivatives<T> argument)
        {
            var result = new Derivatives<T> { Value = _builder.Plus(argument.Value) };
            foreach (var pair in argument)
                result[pair.Key] = _builder.Plus(pair.Value);
            return result;
        }
        Derivatives<T> IBuilder<Derivatives<T>>.Minus(Derivatives<T> argument)
        {
            var result = new Derivatives<T> { Value = _builder.Minus(argument.Value) };
            foreach (var pair in argument)
                result[pair.Key] = _builder.Minus(pair.Value);
            return result;
        }
        Derivatives<T> IBuilder<Derivatives<T>>.Add(Derivatives<T> left, Derivatives<T> right)
        {
            var result = new Derivatives<T>
            {
                Value = _builder.Add(left.Value, right.Value)
            };
            result.Combine(left, right, _builder.Add, x => x, x => x);
            return result;
        }
        Derivatives<T> IBuilder<Derivatives<T>>.Subtract(Derivatives<T> left, Derivatives<T> right)
        {
            var result = new Derivatives<T> { Value = _builder.Subtract(left.Value, right.Value) };
            result.Combine(left, right,
                _builder.Subtract,
                _builder.Plus,
                _builder.Minus);
            return result;
        }
        Derivatives<T> IBuilder<Derivatives<T>>.Multiply(Derivatives<T> left, Derivatives<T> right)
        {
            var result = new Derivatives<T> { Value = _builder.Multiply(left.Value, right.Value) };
            result.Combine(left, right,
                (a, b) => _builder.Add(_builder.Multiply(a, right.Value), _builder.Multiply(left.Value, b)),
                a => _builder.Multiply(a, right.Value),
                b => _builder.Multiply(left.Value, b));
            return result;
        }
        Derivatives<T> IBuilder<Derivatives<T>>.Divide(Derivatives<T> left, Derivatives<T> right)
        {
            var result = new Derivatives<T>
            {
                Value = _builder.Divide(left.Value, right.Value)
            };
            result.Combine(left, right,
                (a, b) => _builder.Divide(
                    _builder.Subtract(_builder.Multiply(a, right.Value), _builder.Multiply(left.Value, b)),
                    _builder.CreateFunction("square", new[] { b })), // (f/g)' = (f'*g - f*g')/g^2
                a => _builder.Divide(a, right.Value), // (f/b)' = f'/b
                b => _builder.Minus(_builder.Divide(_builder.Multiply(left.Value, b), _builder.CreateFunction("square", new[] { b })))); // (a/g)' = -a/g^2*g'
            return result;
        }
        Derivatives<T> IBuilder<Derivatives<T>>.Pow(Derivatives<T> left, Derivatives<T> right)
        {
            var result = new Derivatives<T> { Value = _builder.Pow(left.Value, right.Value) };
            result.Combine(left, right,
                (a, b) => _builder.Add(
                    _builder.Multiply(_builder.Multiply(_builder.Pow(left.Value, _builder.Subtract(right.Value, _one)), right.Value), a),
                    _builder.Multiply(_builder.Multiply(result.Value, b), _builder.CreateFunction("log", new[] { left.Value }))),
                a => _builder.Multiply(_builder.Multiply(_builder.Pow(left.Value, _builder.Subtract(right.Value, _one)), right.Value), a), // (f^n)' = f^(n-1) * n * f'
                b => _builder.Multiply(_builder.Multiply(result.Value, b), _builder.CreateFunction("log", new[] { left.Value }))); // (c^f)' = (e^(f*ln(c)))' = c^f * ln(c) * f'
            return result;
        }
        Derivatives<T> IBuilder<Derivatives<T>>.Mod(Derivatives<T> left, Derivatives<T> right)
        {
            var result = new Derivatives<T> { Value = _builder.Mod(left.Value, right.Value) };
            result.Combine(left, right,
                (df, dg) => _builder.Subtract(df, _builder.Multiply(dg, result.Value)),
                df => df,
                dg => _builder.Minus(_builder.Multiply(dg, result.Value)));
            return result;
        }
        Derivatives<T> IBuilder<Derivatives<T>>.And(Derivatives<T> left, Derivatives<T> right)
        {
            var result = new Derivatives<T>
            {
                Value = _builder.And(left.Value, right.Value)
            };
            // No derivatives for a logical operator
            return result;
        }
        Derivatives<T> IBuilder<Derivatives<T>>.Conditional(Derivatives<T> condition, Derivatives<T> ifTrue, Derivatives<T> ifFalse)
        {
            var result = new Derivatives<T>
            {
                Value = _builder.Conditional(condition.Value, ifTrue.Value, ifFalse.Value)
            };
            result.Combine(ifTrue, ifFalse,
                (a, b) => _builder.Conditional(condition.Value, a, b),
                a => _builder.Conditional(condition.Value, a, _zero),
                b => _builder.Conditional(condition.Value, _zero, b));
            return result;
        }
        Derivatives<T> IBuilder<Derivatives<T>>.Equals(Derivatives<T> left, Derivatives<T> right)
        {
            return new Derivatives<T>
            {
                Value = _builder.Equals(left.Value, right.Value)
            };
        }
        Derivatives<T> IBuilder<Derivatives<T>>.GreaterThan(Derivatives<T> left, Derivatives<T> right)
        {
            return new Derivatives<T>
            {
                Value = _builder.GreaterThan(left.Value, right.Value)
            };
        }
        Derivatives<T> IBuilder<Derivatives<T>>.GreaterThanOrEqual(Derivatives<T> left, Derivatives<T> right)
        {
            return new Derivatives<T>
            {
                Value = _builder.GreaterThanOrEqual(left.Value, right.Value)
            };
        }
        Derivatives<T> IBuilder<Derivatives<T>>.LessThan(Derivatives<T> left, Derivatives<T> right)
        {
            return new Derivatives<T>
            {
                Value = _builder.LessThan(left.Value, right.Value)
            };
        }
        Derivatives<T> IBuilder<Derivatives<T>>.LessThanOrEqual(Derivatives<T> left, Derivatives<T> right)
        {
            return new Derivatives<T>
            {
                Value = _builder.LessThanOrEqual(left.Value, right.Value)
            };
        }
        Derivatives<T> IBuilder<Derivatives<T>>.NotEquals(Derivatives<T> left, Derivatives<T> right)
        {
            return new Derivatives<T> { Value = _builder.NotEquals(left.Value, right.Value) };
        }
        Derivatives<T> IBuilder<Derivatives<T>>.Or(Derivatives<T> left, Derivatives<T> right)
        {
            return new Derivatives<T> { Value = _builder.Or(left.Value, right.Value) };
        }
    }
}
