using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpBehavioral.Parsers.ShuntingYard
{
    /// <summary>
    /// Defaults for double parsers.
    /// </summary>
    public static class DerivativesDefaults<K, V>
    {
        /// <summary>
        /// Gets or sets the default functions.
        /// </summary>
        /// <value>
        /// The default functions.
        /// </value>
        public static IDictionary<string, FunctionDescription> Functions { get; set; } = new Dictionary<string, FunctionDescription>
        {
            { "abs", Abs }, { "exp", Exp }, { "log", Log }, { "ln", Log }, { "log10", Log10 }, { "sqrt", Sqrt },
            { "sin", Sin }, { "cos", Cos }, { "tan", Tan },
            { "asin", Asin }, { "acos", Acos }, { "atan", Atan },
            { "sinh", Sinh }, { "cosh", Cosh }, { "tanh", Tanh },
            { "max", Max }, { "min", Min }, { "pow", Pow }
        };

        /// <summary>
        /// Gets all keys in a number of key collections.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns>The keys.</returns>
        private static IEnumerable<K> GetKeys(params IEnumerable<K>[] keys)
        {
            var result = keys[0];
            for (var i = 1; i < keys.Length; i++)
                result = result.Union(keys[i]);
            return result.Distinct();
        }

        /// <summary>
        /// Gets or sets the default variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public static IDictionary<string, IDerivatives<K, V>> Variables { get; set; } = new Dictionary<string, IDerivatives<K, V>>();

        /// <summary>
        /// Called when a function was found.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments containing the event data.</param>
        public static void FunctionFound(object sender, FunctionFoundEventArgs<IDerivatives<K, V>> args)
        {
            if (sender is DerivativesOperators<K, V> parent)
            {
                if (Functions != null && Functions.TryGetValue(args.Name, out var fd))
                    args.Result = fd(parent.Parent, args.Arguments);
            }
        }

        /// <summary>
        /// Called when a variable was found.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments containing the event data.</param>
        public static void VariableFound(object sender, VariableFoundEventArgs<IDerivatives<K, V>> args)
        {
            if (Variables != null && Variables.TryGetValue(args.Name, out var value))
                args.Result = value;
        }

        private static IDerivatives<K, V> Abs(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for abs()");
            var n = new Derivatives<K, V>
            {
                Value = parent.CreateFunction("abs", new[] { arguments[0].Value })
            };
            foreach (var pair in arguments[0])
                n[pair.Key] = parent.Multiply(parent.Sign(arguments[0].Value), pair.Value);
            return n;
        }
        private static IDerivatives<K, V> Exp(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for exp()");
            var n = new Derivatives<K, V>
            {
                Value = parent.CreateFunction("exp", new[] { arguments[0].Value })
            };
            foreach (var pair in arguments[0])
                n[pair.Key] = parent.Multiply(n.Value, pair.Value);
            return n;
        }
        private static IDerivatives<K, V> Log(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for log()");
            var n = new Derivatives<K, V>
            {
                Value = parent.CreateFunction("log", new[] { arguments[0].Value })
            };
            foreach (var pair in arguments[0])
                n[pair.Key] = parent.Divide(pair.Value, arguments[0].Value);
            return n;
        }
        private static IDerivatives<K, V> Log10(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for log10()");
            var n = new Derivatives<K, V>
            {
                Value = parent.CreateFunction("log10", new[] { arguments[0].Value })
            };
            foreach (var pair in arguments[0])
                n[pair.Key] = parent.Divide(pair.Value, parent.Multiply(arguments[0].Value, parent.CreateValue(Math.Log(10.0))));
            return n;
        }
        private static IDerivatives<K, V> Sqrt(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for sqrt()");
            var n = new Derivatives<K, V>
            {
                Value = parent.CreateFunction("sqrt", new[] { arguments[0].Value })
            };
            foreach (var pair in arguments[0])
                n[pair.Key] = parent.Divide(pair.Value, parent.Multiply(n.Value, parent.CreateValue(2.0)));
            return n;
        }
        private static IDerivatives<K, V> Sin(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for sin()");
            var n = new Derivatives<K, V>
            {
                Value = parent.CreateFunction("sin", new[] { arguments[0].Value })
            };
            foreach (var pair in arguments[0])
                n[pair.Key] = parent.Multiply(parent.CreateFunction("cos", new[] { arguments[0].Value }), pair.Value);
            return n;
        }
        private static IDerivatives<K, V> Cos(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for cos()");
            var n = new Derivatives<K, V>
            {
                Value = parent.CreateFunction("cos", new[] { arguments[0].Value })
            };
            foreach (var pair in arguments[0])
                n[pair.Key] = parent.UnaryMinus(parent.Multiply(parent.CreateFunction("sin", new[] { arguments[0].Value }), pair.Value));
            return n;
        }
        private static IDerivatives<K, V> Tan(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for tan()");
            var n = new Derivatives<K, V>
            {
                Value = parent.CreateFunction("tan", new[] { arguments[0].Value })
            };
            foreach (var pair in arguments[0])
                n[pair.Key] = parent.Divide(
                    pair.Value,
                    parent.Pow(
                        parent.CreateFunction("cos", new[] { arguments[0].Value }),
                        parent.CreateValue(2.0)));
            return n;
        }
        private static IDerivatives<K, V> Asin(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for asin()");
            var n = new Derivatives<K, V>
            {
                Value = parent.CreateFunction("asin", new[] { arguments[0].Value })
            };
            foreach (var pair in arguments[0])
                n[pair.Key] = parent.Divide(
                    pair.Value,
                    parent.CreateFunction("sqrt", new[] {
                        parent.Subtract(
                            parent.CreateValue(1.0),
                            parent.Pow(arguments[0].Value, parent.CreateValue(2.0)))
                    }));
            return n;
        }
        private static IDerivatives<K, V> Acos(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for acos()");
            var n = new Derivatives<K, V>
            {
                Value = parent.CreateFunction("acos", new[] { arguments[0].Value })
            };
            foreach (var pair in arguments[0])
                n[pair.Key] = parent.UnaryMinus(parent.Divide(
                    pair.Value,
                    parent.CreateFunction("sqrt", new[]
                    {
                        parent.Subtract(
                            parent.CreateValue(1.0),
                            parent.Pow(arguments[0].Value, parent.CreateValue(2.0)))
                    })));
            return n;
        }
        private static IDerivatives<K, V> Atan(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for atan()");
            var n = new Derivatives<K, V>
            {
                Value = parent.CreateFunction("atan", new[] { arguments[0].Value })
            };
            foreach (var pair in arguments[0])
                n[pair.Key] = parent.Divide(
                    pair.Value,
                    parent.Add(
                        parent.CreateValue(1.0),
                        parent.Pow(arguments[0].Value, parent.CreateValue(2.0))));
            return n;
        }
        private static IDerivatives<K, V> Sinh(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for sinh()");
            var n = new Derivatives<K, V>
            {
                Value = parent.CreateFunction("sinh", new[] { arguments[0].Value })
            };
            foreach (var pair in arguments[0])
                n[pair.Key] = parent.Multiply(
                    parent.CreateFunction("cosh", new[] { arguments[0].Value }),
                    pair.Value);
            return n;
        }
        private static IDerivatives<K, V> Cosh(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for cosh()");
            var n = new Derivatives<K, V>
            {
                Value = parent.CreateFunction("cosh", new[] { arguments[0].Value })
            };
            foreach (var pair in arguments[0])
                n[pair.Key] = parent.Multiply(
                    parent.CreateFunction("sinh", new[] { arguments[0].Value }),
                    pair.Value);
            return n;
        }
        private static IDerivatives<K, V> Tanh(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for tanh()");
            var n = new Derivatives<K, V>
            {
                Value = parent.CreateFunction("tanh", new[] { arguments[0].Value })
            };
            foreach (var pair in arguments[0])
                n[pair.Key] = parent.Divide(
                    pair.Value,
                    parent.Pow(
                        parent.CreateFunction("cosh", new[] { arguments[0].Value }),
                        parent.CreateValue(2.0)));
            return n;
        }
        private static IDerivatives<K, V> Max(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count < 2)
                throw new ParserException("Invalid number of arguments for max()");
            var result = arguments[0];
            for (var i = 1; i < arguments.Count; i++)
            {
                var n = new Derivatives<K, V>
                {
                    Value = parent.CreateFunction("max", new[] { result.Value, arguments[i].Value })
                };
                foreach (var key in GetKeys(result.Keys, arguments[i].Keys))
                {
                    if (!result.TryGetValue(key, out var left))
                        left = parent.CreateValue(0.0);
                    if (!arguments[i].TryGetValue(key, out var right))
                        right = parent.CreateValue(0.0);
                    n[key] = parent.IfThenElse(
                        parent.Greater(result.Value, arguments[i].Value),
                        left,
                        right);
                }
                result = n;
            }
            return result;
        }
        private static IDerivatives<K, V> Min(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count < 2)
                throw new ParserException("Invalid number of arguments for min()");
            var result = arguments[0];
            for (var i = 1; i < arguments.Count; i++)
            {
                var n = new Derivatives<K, V>
                {
                    Value = parent.CreateFunction("min", new[] { result.Value, arguments[i].Value })
                };
                foreach (var key in GetKeys(result.Keys, arguments[i].Keys))
                {
                    if (!result.TryGetValue(key, out var left))
                        left = parent.CreateValue(0.0);
                    if (!arguments[i].TryGetValue(key, out var right))
                        right = parent.CreateValue(0.0);
                    n[key] = parent.IfThenElse(
                        parent.Less(result.Value, arguments[i].Value),
                        left,
                        right);
                }
                result = n;
            }
            return result;
        }
        private static IDerivatives<K, V> Pow(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            if (arguments.Count != 2)
                throw new ParserException("Invalid number of arguments for pow()");
            var n = new Derivatives<K, V>
            {
                Value = parent.Pow(arguments[0].Value, arguments[1].Value)
            };
            foreach (var key in GetKeys(arguments[0].Keys, arguments[1].Keys))
            {
                var hasLeft = arguments[0].TryGetValue(key, out var leftDerivative);
                var hasRight = arguments[1].TryGetValue(key, out var rightDerivative);
                if (hasLeft && hasRight)
                {
                    n[key] = parent.Add(
                        parent.Multiply(
                            parent.Multiply(
                                parent.Pow(arguments[0].Value, parent.Subtract(arguments[1].Value, parent.CreateValue(1.0))),
                                arguments[1].Value),
                            leftDerivative),
                        parent.Multiply(
                            parent.Multiply(
                                parent.Pow(arguments[0].Value, arguments[1].Value),
                                parent.Log(arguments[0].Value)),
                            rightDerivative));
                }
                else if (hasLeft)
                {
                    n[key] = parent.Multiply(
                        parent.Multiply(
                            parent.Pow(arguments[0].Value, parent.Subtract(arguments[1].Value, parent.CreateValue(1.0))),
                            arguments[1].Value),
                        leftDerivative);
                }
                else if (hasRight)
                {
                    n[key] = parent.Multiply(
                        parent.Multiply(
                            parent.Pow(arguments[0].Value, arguments[1].Value),
                            parent.Log(arguments[0].Value)),
                        rightDerivative);
                }
            }
            return n;
        }

        /// <summary>
        /// Describes a function.
        /// </summary>
        /// <param name="parent">The poerators for the value type.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The function value.</returns>
        public delegate IDerivatives<K, V> FunctionDescription(IShuntingYardOperators<V> parent, IReadOnlyList<IDerivatives<K, V>> arguments);
    }
}
