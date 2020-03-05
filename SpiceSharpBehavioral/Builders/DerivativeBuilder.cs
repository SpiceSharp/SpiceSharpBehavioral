using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Diagnostics;
using SpiceSharpBehavioral.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// A builder for derivatives.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IBuilder{T}" />
    /// <seealso cref="Derivatives{T}"/>
    public class DerivativeBuilder<T> : IBuilder<Derivatives<T>>
    {
        private readonly ISimulation _simulation;
        private readonly IBuilder<T> _builder;
        private readonly T _zero;
        private readonly T _one;
        private readonly T _two;

        /// <summary>
        /// Enumerates all common variables of two derivative containers.
        /// </summary>
        /// <param name="a">The first derivatives instance.</param>
        /// <param name="b">The second derivatives instance.</param>
        /// <returns>The common variables.</returns>
        private static IEnumerable<Variable> GetVariables(Derivatives<T> a, Derivatives<T> b)
            => a.Keys.Union(b.Keys).Distinct();

        /// <summary>
        /// Initializes a new instance of the <see cref="DerivativeBuilder{T}"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="builder">The builder for the underlying methods.</param>
        public DerivativeBuilder(ISimulation simulation, IBuilder<T> builder)
        {
            _simulation = simulation.ThrowIfNull(nameof(simulation));
            _builder = builder.ThrowIfNull(nameof(builder));
            _zero = _builder.CreateNumber("0");
            _one = _builder.CreateNumber("1");
            _two = _builder.CreateNumber("2");
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
        Derivatives<T> IBuilder<Derivatives<T>>.CreateCurrent(string name, QuantityTypes type)
        {
            var result = new Derivatives<T>
            {
                Value = _builder.CreateCurrent(name, type)
            };
            if (type == QuantityTypes.Raw)
            {
                var behaviors = _simulation.EntityBehaviors[name];
                if (behaviors.TryGetValue(out IBranchedBehavior behavior))
                    result[behavior.Branch] = _one;
            }
            return result;
        }
        Derivatives<T> IBuilder<Derivatives<T>>.CreateFunction(string name, IReadOnlyList<Derivatives<T>> arguments)
        {
            var argValues = new T[arguments.Count];
            for (var i = 0; i < arguments.Count; i++)
                argValues[i] = arguments[i].Value;
            Derivatives<T> result = new Derivatives<T>() { Value = _builder.CreateFunction(name, argValues) };

            switch (name)
            {
                case "abs": result.Combine(arguments[0], df => _builder.Multiply(_builder.CreateFunction("sgn", new[] { arguments[0].Value }), df)); break; // sgn(x)
                case "sqrt": result.Combine(arguments[0], df => _builder.Divide(df, _builder.Multiply(_two, arguments[0].Value))); break; // 1/(2sqrt(x))
                case "pwr": break; // To calculate
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
                case "log": result.Combine(arguments[0], df => _builder.Divide(df, arguments[0].Value)); break; // 1/x
                case "log10":
                    result.Combine(arguments[0], df => _builder.Multiply(
          _builder.CreateFunction("log10", new[] { _builder.CreateFunction("exp", new[] { _one }) }), // log10(exp(1))
          _builder.Divide(df, arguments[0].Value)));
                    break; // log10(e)/x
                case "exp": result.Combine(arguments[0], df => _builder.Multiply(arguments[0].Value, df)); break; // exp(x)
                case "sin": result.Combine(arguments[0], df => _builder.Multiply(_builder.CreateFunction("cos", new[] { arguments[0].Value }), df)); break; // cos(x)
                case "cos": result.Combine(arguments[0], df => _builder.Minus(_builder.Multiply(_builder.CreateFunction("sin", new[] { arguments[0].Value }), df))); break; // -sin(x)
                case "tan":
                    result.Combine(arguments[0], df => _builder.Divide(df, _builder.CreateFunction("square", new[] {
                        _builder.CreateFunction("cos", new[] { arguments[0].Value })
                    }))); break; // 1/cos2(x)
                case "asin":
                    result.Combine(arguments[0], df => _builder.Divide(df,
           _builder.CreateFunction("sqrt", new[] {
                        _builder.Subtract(_one, _builder.CreateFunction("square", new[] { arguments[0].Value }))
           }))); break; // 1/sqrt(1-x2)
                case "acos":
                    result.Combine(arguments[0], df => _builder.Divide(_builder.Minus(df),
           _builder.CreateFunction("sqrt", new[] {
                        _builder.Subtract(_one, _builder.CreateFunction("square", new[] { arguments[0].Value }))
           }))); break; // -1/sqrt(1-x2)
                case "atan":
                    result.Combine(arguments[0], df => _builder.Divide(df,
           _builder.Add(_one, _builder.CreateFunction("square", new[] { arguments[0].Value })))); break; // 1/(1+x2)
                case "sinh": result.Combine(arguments[0], df => _builder.Multiply(_builder.CreateFunction("sinh", new[] { arguments[0].Value }), df)); break; // cosh(x)
                case "cosh": result.Combine(arguments[0], df => _builder.Multiply(_builder.CreateFunction("cosh", new[] { arguments[0].Value }), df)); break; // sinh(x)
                case "tanh":
                    result.Combine(arguments[0], df => _builder.Divide(df,
           _builder.CreateFunction("square", new[] {
                        _builder.CreateFunction("cosh", new[] { arguments[0].Value })
           }))); break; // 1/cosh2(x)
                case "u": break; // 0
                case "u2": break; // 0
                case "uramp": result.Combine(arguments[0], df => _builder.Conditional(_builder.GreaterThanOrEqual(arguments[0].Value, _zero), df, _zero)); break; // x > 0 ? 1 : 0
                case "ceil": break; // 0
                case "floor": break; // 0
                case "nint": break; // 0
                case "round": break; // 0
                case "square": result.Combine(arguments[0], df => _builder.Multiply(_builder.Multiply(_two, arguments[0].Value), df)); break; // 2x
                case "pwl": result.Combine(arguments[0], df => _builder.CreateFunction("pwl_derivative", argValues)); break; // pwl_derivative(x)
            }
            return result;
        }
        Derivatives<T> IBuilder<Derivatives<T>>.CreateNumber(string value)
        {
            return new Derivatives<T> { Value = _builder.CreateNumber(value) };
        }
        Derivatives<T> IBuilder<Derivatives<T>>.CreateProperty(string name, string property, QuantityTypes type)
        {
            return new Derivatives<T> { Value = _builder.CreateProperty(name, property, type) };
        }
        Derivatives<T> IBuilder<Derivatives<T>>.CreateVariable(string name)
        {
            throw new NotImplementedException();
        }
        Derivatives<T> IBuilder<Derivatives<T>>.CreateVoltage(string node, string reference, QuantityTypes type)
        {
            var result = new Derivatives<T>
            {
                Value = _builder.CreateVoltage(node, reference, type)
            };
            if (type == QuantityTypes.Raw)
            {
                var variable = _simulation.Variables[node];
                result[variable] = _one;
                if (reference != null)
                {
                    variable = _simulation.Variables[reference];
                    result[variable] = _builder.Minus(_one);
                }
            }
            return result;
        }
    }
}
