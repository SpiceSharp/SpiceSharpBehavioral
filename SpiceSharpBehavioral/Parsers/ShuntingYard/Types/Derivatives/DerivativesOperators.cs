using SpiceSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpiceSharpBehavioral.Parsers.ShuntingYard
{
    /// <summary>
    /// Defines default derivative operators.
    /// </summary>
    /// <typeparam name="K">The derivative key type.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    /// <seealso cref="IShuntingYardOperators{T}" />
    public class DerivativesOperators<K, V> : IShuntingYardOperators<IDerivatives<K, V>>
    {
        /// <summary>
        /// Gets the parent operators.
        /// </summary>
        /// <value>
        /// The parent operators.
        /// </value>
        public IShuntingYardOperators<V> Parent { get; }

        /// <summary>
        /// Occurs when a variable was found.
        /// </summary>
        public event EventHandler<VariableFoundEventArgs<IDerivatives<K, V>>> VariableFound;

        /// <summary>
        /// Occurs when a function was found.
        /// </summary>
        public event EventHandler<FunctionFoundEventArgs<IDerivatives<K, V>>> FunctionFound;

        /// <summary>
        /// Initializes a new instance of the <see cref="DerivativesOperators{K, V}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public DerivativesOperators(IShuntingYardOperators<V> parent)
        {
            Parent = parent.ThrowIfNull(nameof(parent));
            FunctionFound += DerivativesDefaults<K, V>.FunctionFound;
            VariableFound += DerivativesDefaults<K, V>.VariableFound;
        }

        /// <summary>
        /// Gets all keys in a number of key collections.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns>The keys.</returns>
        protected static IEnumerable<K> GetKeys(params IEnumerable<K>[] keys)
        {
            var result = keys[0];
            for (var i = 1; i < keys.Length; i++)
                result = result.Union(keys[i]);
            return result.Distinct();
        }

        /// <summary>
        /// Applies a unary plus operator.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> UnaryPlus(IDerivatives<K, V> argument)
            => argument;

        /// <summary>
        /// Applies a unary minus operator.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> UnaryMinus(IDerivatives<K, V> argument)
        {
            var n = new Derivatives<K, V>();
            n.Value = Parent.UnaryMinus(argument.Value);
            foreach (var pair in argument)
                n[pair.Key] = Parent.UnaryMinus(pair.Value);
            return n;
        }

        /// <summary>
        /// Applies a unary not operator.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> Not(IDerivatives<K, V> argument)
        {
            var n = new Derivatives<K, V>();
            n.Value = Parent.Not(argument.Value);
            // Output is either 0 or 1, no derivative
            return n;
        }

        /// <summary>
        /// Sign of the argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> Sign(IDerivatives<K, V> argument)
        {
            return new Derivatives<K, V>
            {
                Value = Parent.Sign(argument.Value)
            };
        }

        /// <summary>
        /// Addition.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> Add(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = new Derivatives<K, V>
            {
                Value = Parent.Add(left.Value, right.Value)
            };
            foreach (var key in GetKeys(left.Keys, right.Keys))
            {
                bool hasLeft = left.TryGetValue(key, out V leftDerivative);
                bool hasRight = right.TryGetValue(key, out V rightDerivative);
                if (hasLeft && hasRight)
                    n[key] = Parent.Add(leftDerivative, rightDerivative);
                else if (hasLeft)
                    n[key] = leftDerivative;
                else if (hasRight)
                    n[key] = rightDerivative;
            }
            return n;
        }

        /// <summary>
        /// Subtraction.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> Subtract(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = new Derivatives<K, V>
            {
                Value = Parent.Subtract(left.Value, right.Value)
            };
            foreach (var key in GetKeys(left.Keys, right.Keys))
            {
                bool hasLeft = left.TryGetValue(key, out V leftDerivative);
                bool hasRight = right.TryGetValue(key, out V rightDerivative);
                if (hasLeft && hasRight)
                    n[key] = Parent.Subtract(leftDerivative, rightDerivative);
                else if (hasLeft)
                    n[key] = leftDerivative;
                else if (hasRight)
                    n[key] = Parent.UnaryMinus(rightDerivative);
            }
            return n;
        }

        /// <summary>
        /// Multiplication.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> Multiply(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = new Derivatives<K, V>
            {
                Value = Parent.Multiply(left.Value, right.Value)
            };
            foreach (var key in GetKeys(left.Keys, right.Keys))
            {
                bool hasLeft = left.TryGetValue(key, out V leftDerivative);
                bool hasRight = right.TryGetValue(key, out V rightDerivative);
                if (hasLeft && hasRight)
                {
                    n[key] = Parent.Add(
                        Parent.Multiply(leftDerivative, right.Value),
                        Parent.Multiply(left.Value, rightDerivative));
                }
                else if (hasLeft)
                    n[key] = Parent.Multiply(leftDerivative, right.Value);
                else if (hasRight)
                    n[key] = Parent.Multiply(left.Value, rightDerivative);
            }
            return n;
        }

        /// <summary>
        /// Division.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> Divide(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = new Derivatives<K, V>
            {
                Value = Parent.Divide(left.Value, right.Value)
            };
            foreach (var key in GetKeys(left.Keys, right.Keys))
            {
                bool hasLeft = left.TryGetValue(key, out V leftDerivative);
                bool hasRight = right.TryGetValue(key, out V rightDerivative);
                if (hasLeft && hasRight)
                {
                    n[key] = Parent.Divide(
                        Parent.Subtract(
                            Parent.Multiply(leftDerivative, right.Value),
                            Parent.Multiply(left.Value, rightDerivative)),
                        Parent.Pow(right.Value, Parent.CreateValue(2.0)));
                }
                else if (hasLeft)
                    n[key] = Parent.Divide(leftDerivative, right.Value);
                else if (hasRight)
                {
                    n[key] = Parent.UnaryMinus(Parent.Divide(
                        Parent.Multiply(left.Value, rightDerivative),
                        Parent.Pow(right.Value, Parent.CreateValue(2.0))));
                }
            }
            return n;
        }

        /// <summary>
        /// Raising to a power.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> Pow(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = new Derivatives<K, V>
            {
                Value = Parent.Pow(left.Value, right.Value)
            };
            foreach (var key in GetKeys(left.Keys, right.Keys))
            {
                var hasLeft = left.TryGetValue(key, out var leftDerivative);
                var hasRight = right.TryGetValue(key, out var rightDerivative);
                if (hasLeft && hasRight)
                {
                    n[key] = Parent.Add(
                        Parent.Multiply(
                            Parent.Multiply(
                                Parent.Pow(left.Value, Parent.Subtract(right.Value, Parent.CreateValue(1.0))),
                                right.Value),
                            leftDerivative),
                        Parent.Multiply(
                            Parent.Multiply(
                                Parent.Pow(left.Value, right.Value),
                                Parent.Log(left.Value)),
                            rightDerivative));
                }
                else if (hasLeft)
                {
                    n[key] = Parent.Multiply(
                        Parent.Multiply(
                            Parent.Pow(left.Value, Parent.Subtract(right.Value, Parent.CreateValue(1.0))),
                            right.Value),
                        leftDerivative);
                }
                else if (hasRight)
                {
                    n[key] = Parent.Multiply(
                        Parent.Multiply(
                            Parent.Pow(left.Value, right.Value),
                            Parent.Log(left.Value)),
                        rightDerivative);
                }
            }
            return n;
        }

        /// <summary>
        /// Logarithm.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> Log(IDerivatives<K, V> argument)
        {
            var n = new Derivatives<K, V>
            {
                Value = Parent.Log(argument.Value)
            };
            foreach (var pair in argument)
                n[pair.Key] = Parent.Divide(pair.Value, argument.Value);
            return n;
        }

        /// <summary>
        /// Remainder.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> Modulo(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = new Derivatives<K, V>
            {
                Value = Parent.Modulo(left.Value, right.Value)
            };
            foreach (var pair in left)
                n[pair.Key] = pair.Value;
            return n;
        }

        /// <summary>
        /// Greater than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> Greater(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            return new Derivatives<K, V>
            {
                Value = Parent.Greater(left.Value, right.Value)
            };
        }

        /// <summary>
        /// Less than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> Less(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            return new Derivatives<K, V>
            {
                Value = Parent.Less(left.Value, right.Value)
            };
        }

        /// <summary>
        /// Greater or equal than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> GreaterOrEqual(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            return new Derivatives<K, V>
            {
                Value = Parent.GreaterOrEqual(left.Value, right.Value)
            };
        }

        /// <summary>
        /// Less or equal than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> LessOrEqual(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            return new Derivatives<K, V>
            {
                Value = Parent.LessOrEqual(left.Value, right.Value)
            };
        }

        /// <summary>
        /// Equality.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> Equal(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            return new Derivatives<K, V>
            {
                Value = Parent.Equal(left.Value, right.Value)
            };
        }

        /// <summary>
        /// Inequality.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> NotEqual(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            return new Derivatives<K, V>
            {
                Value = Parent.NotEqual(left.Value, right.Value)
            };
        }

        /// <summary>
        /// A conditional.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="ifTrue">If true.</param>
        /// <param name="ifFalse">If false.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> IfThenElse(IDerivatives<K, V> condition, IDerivatives<K, V> ifTrue, IDerivatives<K, V> ifFalse)
        {
            var n = new Derivatives<K, V>
            {
                Value = Parent.IfThenElse(condition.Value, ifTrue.Value, ifFalse.Value)
            };
            foreach (var key in GetKeys(ifTrue.Keys, ifFalse.Keys))
            {
                if (!ifTrue.TryGetValue(key, out V trueDerivative))
                    trueDerivative = Parent.CreateValue(0.0);
                if (!ifFalse.TryGetValue(key, out V falseDerivative))
                    falseDerivative = Parent.CreateValue(0.0);
                n[key] = Parent.IfThenElse(condition.Value, trueDerivative, falseDerivative);
            }
            return n;
        }

        /// <summary>
        /// Creates a value.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="unit">The unit.</param>
        /// <returns>
        /// The value.
        /// </returns>
        public IDerivatives<K, V> CreateValue(double input, string unit = "")
        {
            return new Derivatives<K, V>
            {
                Value = Parent.CreateValue(input, unit)
            };
        }

        /// <summary>
        /// Creates a variable value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The value.
        /// </returns>
        public IDerivatives<K, V> CreateVariable(string name)
        {
            var args = new VariableFoundEventArgs<IDerivatives<K, V>>(name);
            VariableFound?.Invoke(this, args);
            if (args.Found)
                return args.Result;
            return new Derivatives<K, V> { Value = Parent.CreateVariable(name) };
        }

        /// <summary>
        /// Creates a function value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The function value.
        /// </returns>
        public IDerivatives<K, V> CreateFunction(string name, IReadOnlyList<IDerivatives<K, V>> arguments)
        {
            var args = new FunctionFoundEventArgs<IDerivatives<K, V>>(name, arguments);
            FunctionFound?.Invoke(this, args);
            if (args.Found)
                return args.Result;

            // Create arguments
            var newArgs = new V[arguments.Count];
            for (var i = 0; i < newArgs.Length; i++)
                newArgs[i] = arguments[i].Value;
            return new Derivatives<K, V> { Value = Parent.CreateFunction(name, newArgs) };
        }

        /// <summary>
        /// And.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> And(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            return new Derivatives<K, V>
            {
                Value = Parent.And(left.Value, right.Value)
            };
        }

        /// <summary>
        /// Or.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public IDerivatives<K, V> Or(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            return new Derivatives<K, V>
            {
                Value = Parent.Or(left.Value, right.Value)
            };
        }
    }
}
