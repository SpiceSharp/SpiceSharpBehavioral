using SpiceSharp;
using SpiceSharpBehavioral.Parsers.Operators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpBehavioral.Parsers.Derivatives
{
    /// <summary>
    /// Default functions for double parsers.
    /// </summary>
    public static class Defaults<K, V>
    {
        private static Dictionary<string, FunctionDescription> _functions = new Dictionary<string, FunctionDescription>
        {
            { "abs", Abs }, { "sgn", Sgn }, { "sqrt", Sqrt }, { "exp", Exp },
            { "log", Log }, { "ln", Log }, { "log10", Log10 }, { "round", Round },
            { "sin", Sin }, { "cos", Cos }, { "tan", Tan }, { "asin", Asin },
            { "acos", Acos }, { "atan", Atan }, { "sinh", Sinh }, { "cosh", Cosh },
            { "tanh", Tanh }, { "max", Max }, { "min", Min }, { "pow", Pow }
        };

        /// <summary>
        /// Enumerates the combined keys.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The derivative keys.</returns>
        private static IEnumerable<K> CombinedKeys(IDerivatives<K, V> left, IDerivatives<K, V> right)
            => left.Keys.Union(right.Keys).Distinct();

        /// <summary>
        /// Gets or sets the comparer used for functions.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        public static IEqualityComparer<string> Comparer
        {
            get => _functions.Comparer;
            set
            {
                if (_functions.Comparer != value)
                {
                    var nfunctions = new Dictionary<string, FunctionDescription>(value);
                    foreach (var pair in _functions)
                        nfunctions[pair.Key] = pair.Value;
                }
            }
        }

        /// <summary>
        /// Gets the functions.
        /// </summary>
        /// <value>
        /// The functions.
        /// </value>
        public static IDictionary<string, FunctionDescription> Functions => _functions;

        /// <summary>
        /// Event for default functions.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FunctionFoundEventArgs{Double}"/> instance containing the event data.</param>
        public static void FunctionFound(object sender, FunctionFoundEventArgs<IDerivatives<K, V>> e)
        {
            if (sender is ValueFactory<K, V> valueFactory)
            {
                if (_functions.TryGetValue(e.Name, out var description))
                    e.Result = description(valueFactory.Base, valueFactory.Factory, e.Arguments);
            }
        }

        private static IDerivatives<K, V> Abs(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 1)
            {
                var n = factory.Create(arguments[0]);
                n.Value = parameters.ValueFactory.CreateFunction("abs", arguments[0].Value);
                foreach (var pair in arguments[0])
                {
                    n[pair.Key] = parameters.Arithmetic.Multiply(
                        parameters.ValueFactory.CreateFunction("sgn", arguments[0].Value),
                        pair.Value);
                }
                return n;
            }
            throw new Exception();
        }
        private static IDerivatives<K, V> Sgn(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 1)
            {
                var n = factory.Create();
                n.Value = parameters.ValueFactory.CreateFunction("sign", arguments[0].Value);
                // Derivatives are 0
                return n;
            }
            throw new Exception();
        }
        private static IDerivatives<K, V> Sqrt(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 1)
            {
                var n = factory.Create(arguments[0]);
                n.Value = parameters.ValueFactory.CreateFunction("sqrt", arguments[0].Value);
                foreach (var pair in arguments[0])
                {
                    n[pair.Key] = parameters.Arithmetic.Divide(
                        pair.Value,
                        parameters.Arithmetic.Multiply(parameters.ValueFactory.CreateValue(2.0, ""), n.Value));
                }
                return n;
            }
            throw new Exception();
        }
        private static IDerivatives<K, V> Exp(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 1)
            {
                var n = factory.Create(arguments[0]);
                n.Value = parameters.ValueFactory.CreateFunction("exp", arguments[0].Value);
                foreach (var pair in arguments[0])
                    n[pair.Key] = parameters.Arithmetic.Multiply(n.Value, pair.Value);
                return n;
            }
            throw new Exception();
        }
        private static IDerivatives<K, V> Log(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 1)
            {
                var n = factory.Create(arguments[0]);
                n.Value = parameters.ValueFactory.CreateFunction("log", arguments[0].Value);
                foreach (var pair in arguments[0])
                    n[pair.Key] = parameters.Arithmetic.Divide(pair.Value, arguments[0].Value);
                return n;

            }
            throw new Exception();
        }
        private static IDerivatives<K, V> Log10(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 1)
            {
                var n = factory.Create(arguments[0]);
                n.Value = parameters.ValueFactory.CreateFunction("log10", arguments[0].Value);
                foreach (var pair in arguments[0])
                    n[pair.Key] = parameters.Arithmetic.Divide(pair.Value, parameters.Arithmetic.Multiply(parameters.ValueFactory.CreateValue(Math.Log(10.0), ""), arguments[0].Value));
                return n;
            }
            throw new Exception();
        }
        private static IDerivatives<K, V> Round(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 1)
            {
                var n = factory.Create();
                n.Value = parameters.ValueFactory.CreateFunction("round", arguments[0].Value);
                return n;
            }
            else
            {
                var n = factory.Create();
                n.Value = parameters.ValueFactory.CreateFunction("round", arguments[0].Value, arguments[1].Value);
                return n;
            }
            throw new Exception();
        }
        private static IDerivatives<K, V> Sin(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 1)
            {
                var n = factory.Create(arguments[0]);
                n.Value = parameters.ValueFactory.CreateFunction("sin", arguments[0].Value);
                foreach (var pair in arguments[0])
                    n[pair.Key] = parameters.Arithmetic.Multiply(parameters.ValueFactory.CreateFunction("cos", arguments[0].Value), pair.Value);
                return n;
            }
            throw new Exception();
        }
        private static IDerivatives<K, V> Cos(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 1)
            {
                var n = factory.Create(arguments[0]);
                n.Value = parameters.ValueFactory.CreateFunction("cos", arguments[0].Value);
                foreach (var pair in arguments[0])
                    n[pair.Key] = parameters.Arithmetic.Negate(parameters.Arithmetic.Multiply(parameters.ValueFactory.CreateFunction("sin", arguments[0].Value), pair.Value));
                return n;
            }
            throw new Exception();
        }
        private static IDerivatives<K, V> Tan(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 1)
            {
                var n = factory.Create(arguments[0]);
                n.Value = parameters.ValueFactory.CreateFunction("tan", arguments[0].Value);
                foreach (var pair in arguments[0])
                {
                    var cos = parameters.ValueFactory.CreateFunction("cos", arguments[0].Value);
                    n[pair.Key] = parameters.Arithmetic.Divide(pair.Value, parameters.Arithmetic.Pow(cos, 2));
                }
                return n;
            }
            throw new Exception();
        }
        private static IDerivatives<K, V> Asin(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 1)
            {
                var n = factory.Create(arguments[0]);
                n.Value = parameters.ValueFactory.CreateFunction("asin", arguments[0].Value);
                foreach (var pair in arguments[0])
                {
                    n[pair.Key] = parameters.Arithmetic.Divide(pair.Value,
                        parameters.ValueFactory.CreateFunction("sqrt",
                            parameters.Arithmetic.Subtract(
                                parameters.ValueFactory.CreateValue(1.0, ""),
                                parameters.Arithmetic.Pow(arguments[0].Value, 2)
                                )
                            )
                        );
                }
                return n;
            }
            throw new Exception();
        }
        private static IDerivatives<K, V> Acos(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 1)
            {
                var n = factory.Create(arguments[0]);
                n.Value = parameters.ValueFactory.CreateFunction("acos", arguments[0].Value);
                foreach (var pair in arguments[0])
                {
                    n[pair.Key] = parameters.Arithmetic.Divide(parameters.Arithmetic.Negate(pair.Value),
                        parameters.ValueFactory.CreateFunction("sqrt",
                            parameters.Arithmetic.Subtract(
                                parameters.ValueFactory.CreateValue(1.0, ""),
                                parameters.Arithmetic.Pow(arguments[0].Value, 2)
                                )
                            )
                        );
                }
                return n;
            }
            throw new Exception();
        }
        private static IDerivatives<K, V> Atan(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 1)
            {
                var n = factory.Create(arguments[0]);
                n.Value = parameters.ValueFactory.CreateFunction("atan", arguments[0].Value);
                foreach (var pair in arguments[0])
                {
                    n[pair.Key] = parameters.Arithmetic.Divide(pair.Value,
                        parameters.Arithmetic.Add(
                            parameters.ValueFactory.CreateValue(1.0, ""),
                            parameters.Arithmetic.Pow(arguments[0].Value, 2)));
                }
                return n;
            }
            throw new Exception();
        }
        private static IDerivatives<K, V> Sinh(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 1)
            {
                var n = factory.Create(arguments[0]);
                n.Value = parameters.ValueFactory.CreateFunction("sinh", arguments[0].Value);
                foreach (var pair in arguments[0])
                    n[pair.Key] = parameters.Arithmetic.Multiply(parameters.ValueFactory.CreateFunction("cosh", arguments[0].Value), pair.Value);
                return n;
            }
            throw new Exception();
        }
        private static IDerivatives<K, V> Cosh(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 1)
            {
                var n = factory.Create(arguments[0]);
                n.Value = parameters.ValueFactory.CreateFunction("cosh", arguments[0].Value);
                foreach (var pair in arguments[0])
                    n[pair.Key] = parameters.Arithmetic.Multiply(parameters.ValueFactory.CreateFunction("sinh", arguments[0].Value), pair.Value);
                return n;
            }
            throw new Exception();
        }
        private static IDerivatives<K, V> Tanh(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 1)
            {
                var n = factory.Create(arguments[0]);
                n.Value = parameters.ValueFactory.CreateFunction("tanh", arguments[0].Value);
                foreach (var pair in arguments[0])
                {
                    var cosh = parameters.ValueFactory.CreateFunction("cosh", arguments[0].Value);
                    n[pair.Key] = parameters.Arithmetic.Divide(pair.Value, parameters.Arithmetic.Pow(cosh, 2));
                }
                return n;
            }
            throw new Exception();
        }
        private static IDerivatives<K, V> Max(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 0)
                throw new Exception();
            var n = factory.Create(arguments.ToArray());
            n.Value = arguments[0].Value;
            foreach (var pair in arguments[0])
                n[pair.Key] = pair.Value;
            for (var i = 1; i < arguments.Count; i++)
            {
                foreach (var key in CombinedKeys(n, arguments[i]).ToArray())
                {
                    if (!n.TryGetValue(key, out var leftDerivative))
                        leftDerivative = parameters.ValueFactory.CreateValue(0.0, "");
                    if (!arguments[i].TryGetValue(key, out var rightDerivative))
                        rightDerivative = parameters.ValueFactory.CreateValue(0.0, "");
                    n[key] = parameters.Conditional.IfThenElse(
                       parameters.Relational.GreaterThan(arguments[i].Value, n.Value), rightDerivative,
                       parameters.Conditional.IfThenElse(
                           parameters.Relational.GreaterThan(n.Value, arguments[i].Value), leftDerivative,
                           parameters.ValueFactory.CreateValue(0.0, "")));
                }
                n.Value = parameters.ValueFactory.CreateFunction("max", arguments[i].Value, n.Value);
            }
            return n;
        }
        private static IDerivatives<K, V> Min(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 0)
                throw new Exception();
            var n = factory.Create(arguments.ToArray());
            n.Value = arguments[0].Value;
            foreach (var pair in arguments[0])
                n[pair.Key] = pair.Value;
            for (var i = 1; i < arguments.Count; i++)
            {
                foreach (var key in CombinedKeys(n, arguments[i]).ToArray())
                {
                    if (!n.TryGetValue(key, out var leftDerivative))
                        leftDerivative = parameters.ValueFactory.CreateValue(0.0, "");
                    if (!arguments[i].TryGetValue(key, out var rightDerivative))
                        rightDerivative = parameters.ValueFactory.CreateValue(0.0, "");
                    n[key] = parameters.Conditional.IfThenElse(
                       parameters.Relational.LessThan(arguments[i].Value, n.Value), rightDerivative,
                       parameters.Conditional.IfThenElse(
                           parameters.Relational.LessThan(n.Value, arguments[i].Value), leftDerivative,
                           parameters.ValueFactory.CreateValue(0.0, "")));
                }
                n.Value = parameters.ValueFactory.CreateFunction("min", arguments[i].Value, n.Value);
            }
            return n;
        }
        private static IDerivatives<K, V> Pow(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count == 2)
            {
                var n = factory.Create(arguments[0], arguments[1]);
                n.Value = parameters.Arithmetic.Pow(arguments[0].Value, arguments[1].Value);
                foreach (var key in CombinedKeys(arguments[0], arguments[1]))
                {
                    var hasLeft = arguments[0].TryGetValue(key, out var leftDerivative);
                    var hasRight = arguments[1].TryGetValue(key, out var rightDerivative);
                    if (hasLeft && hasRight)
                    {
                        n[key] = parameters.Arithmetic.Add(
                            parameters.Arithmetic.Multiply(parameters.Arithmetic.Multiply(
                                parameters.Arithmetic.Pow(arguments[0].Value, parameters.Arithmetic.Decrement(arguments[1].Value)), arguments[1].Value), leftDerivative),
                            parameters.Arithmetic.Multiply(parameters.Arithmetic.Multiply(n.Value, parameters.Arithmetic.Log(arguments[0].Value)), rightDerivative)
                            );
                    }
                    else if (hasLeft)
                        n[key] = parameters.Arithmetic.Multiply(parameters.Arithmetic.Multiply(
                            parameters.Arithmetic.Pow(arguments[0].Value, parameters.Arithmetic.Decrement(arguments[1].Value)), arguments[1].Value), leftDerivative);
                    else if (hasRight)
                    {
                        n[key] = parameters.Arithmetic.Multiply(parameters.Arithmetic.Multiply(n.Value, parameters.Arithmetic.Log(arguments[0].Value)), rightDerivative);
                    }
                }
                return n;
            }
            throw new Exception();
        }

        /// <summary>
        /// A function description.
        /// </summary>
        /// <param name="parameters">The parser parameters used to construct the functions.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns></returns>
        public delegate IDerivatives<K, V> FunctionDescription(IParserParameters<V> parameters, IDerivativeFactory<K, V> factory, IReadOnlyList<IDerivatives<K, V>> arguments);
    }
}
