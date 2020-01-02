using SpiceSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpBehavioral.Parsers.Derivatives
{
    /// <summary>
    /// Implementation of a value factory for derivatives.
    /// </summary>
    /// <typeparam name="K">The derivative key.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    /// <seealso cref="IValueFactory{T}" />
    public class ValueFactory<K, V> : IValueFactory<IDerivatives<K, V>>
    {
        private readonly IValueFactory<V> _parent;
        private readonly IArithmeticOperator<V> _arithmetic;
        private readonly IRelationalOperator<V> _relational;
        private readonly IConditionalOperator<V> _conditional;
        private readonly IDerivativeFactory<K, V> _factory;
        
        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IDictionary<string, IDerivatives<K, V>> Variables { get; }

        /// <summary>
        /// Enumerates the combined keys.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The derivative keys.</returns>
        protected IEnumerable<K> CombinedKeys(IDerivatives<K, V> left, IDerivatives<K, V> right)
            => left.Keys.Union(right.Keys).Distinct();

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFactory{K, V}"/> class.
        /// </summary>
        /// <param name="parent">The parent value factory.</param>
        /// <param name="arithmetic">The arithmetic operator.</param>
        /// <param name="relational">The relational operator.</param>
        /// <param name="conditional">The conditioanl operator.</param>
        /// <param name="factory">The factory for derivatives.</param>
        public ValueFactory(IValueFactory<V> parent, IArithmeticOperator<V> arithmetic, IRelationalOperator<V> relational, IConditionalOperator<V> conditional, IDerivativeFactory<K, V> factory)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
            _factory = factory.ThrowIfNull(nameof(factory));
            _relational = relational.ThrowIfNull(nameof(relational));
            _arithmetic = arithmetic.ThrowIfNull(nameof(arithmetic));
            _conditional = conditional.ThrowIfNull(nameof(conditional));
            Variables = new Dictionary<string, IDerivatives<K, V>>();
        }

        /// <summary>
        /// Creates a function of the specified name.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="arguments">The arguments of the function.</param>
        /// <returns>
        /// The value of the function.
        /// </returns>
        public IDerivatives<K, V> CreateFunction(string name, IDerivatives<K, V>[] arguments)
        {
            var n = _factory.Create(arguments);
            switch (arguments.Length)
            {
                case 0:
                    break;
                case 1:
                    switch (name)
                    {
                        case "abs":
                        case "Abs":
                            n.Value = _parent.CreateFunction("Abs", arguments[0].Value);
                            foreach (var pair in arguments[0])
                            {
                                n[pair.Key] = _arithmetic.Multiply(
                                    _parent.CreateFunction("sgn", arguments[0].Value),
                                    pair.Value);
                            }
                            return n;
                        case "sqrt":
                        case "Sqrt":
                            n.Value = _parent.CreateFunction("Sqrt", arguments[0].Value);
                            foreach (var pair in arguments[0])
                            {
                                n[pair.Key] = _arithmetic.Divide(
                                    pair.Value,
                                    _arithmetic.Multiply(_parent.CreateValue(2.0, ""), n.Value));
                            }
                            return n;
                        case "exp":
                        case "Exp":
                            n.Value = _parent.CreateFunction("Exp", arguments[0].Value);
                            foreach (var pair in arguments[0])
                                n[pair.Key] = _arithmetic.Multiply(n.Value, pair.Value);
                            return n;
                        case "log":
                        case "Log":
                        case "ln":
                        case "Ln":
                            n.Value = _parent.CreateFunction("Log", arguments[0].Value);
                            foreach (var pair in arguments[0])
                                n[pair.Key] = _arithmetic.Divide(pair.Value, arguments[0].Value);
                            return n;
                        case "log10":
                        case "Log10":
                            n.Value = _parent.CreateFunction("Log10", arguments[0].Value);
                            foreach (var pair in arguments[0])
                                n[pair.Key] = _arithmetic.Divide(pair.Value, _arithmetic.Multiply(_parent.CreateValue(Math.Log(10.0), ""), arguments[0].Value));
                            return n;
                        case "round":
                        case "Round":
                            n.Value = _parent.CreateFunction("Round", arguments[0].Value);
                            return n;

                        // Trigonometry
                        case "sin":
                        case "Sin":
                            n.Value = _parent.CreateFunction("Sin", arguments[0].Value);
                            foreach (var pair in arguments[0])
                                n[pair.Key] = _arithmetic.Multiply(_parent.CreateFunction("Cos", arguments[0].Value), pair.Value);
                            return n;
                        case "cos":
                        case "Cos":
                            n.Value = _parent.CreateFunction("Cos", arguments[0].Value);
                            foreach (var pair in arguments[0])
                                n[pair.Key] = _arithmetic.Negate(_arithmetic.Multiply(_parent.CreateFunction("Sin", arguments[0].Value), pair.Value));
                            return n;
                        case "tan":
                        case "Tan":
                            n.Value = _parent.CreateFunction("Tan", arguments[0].Value);
                            foreach (var pair in arguments[0])
                            {
                                var cos = _parent.CreateFunction("Cos", arguments[0].Value);
                                n[pair.Key] = _arithmetic.Divide(pair.Value, _arithmetic.Pow(cos, 2));
                            }
                            return n;

                        // Inverse trigonometry
                        case "asin":
                        case "Asin":
                            n.Value = _parent.CreateFunction("Asin", arguments[0].Value);
                            foreach (var pair in arguments[0])
                            {
                                n[pair.Key] = _arithmetic.Divide(pair.Value, 
                                    _parent.CreateFunction("Sqrt",
                                        _arithmetic.Subtract(
                                            _parent.CreateValue(1.0, ""), 
                                            _arithmetic.Pow(arguments[0].Value, 2)
                                            )
                                        )
                                    );
                            }
                            return n;
                        case "acos":
                        case "Acos":
                            n.Value = _parent.CreateFunction("Acos", arguments[0].Value);
                            foreach (var pair in arguments[0])
                            {
                                n[pair.Key] = _arithmetic.Divide(_arithmetic.Negate(pair.Value),
                                    _parent.CreateFunction("Sqrt",
                                        _arithmetic.Subtract(
                                            _parent.CreateValue(1.0, ""),
                                            _arithmetic.Pow(arguments[0].Value, 2)
                                            )
                                        )
                                    );
                            }
                            return n;
                        case "atan":
                        case "Atan":
                            n.Value = _parent.CreateFunction("Atan", arguments[0].Value);
                            foreach (var pair in arguments[0])
                            {
                                n[pair.Key] = _arithmetic.Divide(pair.Value,
                                    _arithmetic.Add(
                                        _parent.CreateValue(1.0, ""),
                                        _arithmetic.Pow(arguments[0].Value, 2)));
                            }
                            return n;

                        // Hyperbolic functions
                        case "sinh":
                        case "Sinh":
                            n.Value = _parent.CreateFunction("Sinh", arguments[0].Value);
                            foreach (var pair in arguments[0])
                                n[pair.Key] = _arithmetic.Multiply(_parent.CreateFunction("Cosh", arguments[0].Value), pair.Value);
                            return n;
                        case "cosh":
                        case "Cosh":
                            n.Value = _parent.CreateFunction("Cosh", arguments[0].Value);
                            foreach (var pair in arguments[0])
                                n[pair.Key] = _arithmetic.Multiply(_parent.CreateFunction("Sinh", arguments[0].Value), pair.Value);
                            return n;
                        case "tanh":
                        case "Tanh":
                            n.Value = _parent.CreateFunction("Tanh", arguments[0].Value);
                            foreach (var pair in arguments[0])
                            {
                                var cosh = _parent.CreateFunction("Cosh", arguments[0].Value);
                                n[pair.Key] = _arithmetic.Divide(pair.Value, _arithmetic.Pow(cosh, 2));
                            }
                            return n;
                    }
                    break;
                case 2:
                    switch (name)
                    {
                        case "max":
                        case "Max":
                            n.Value = arguments[0].Value;
                            foreach (var pair in arguments[0])
                                n[pair.Key] = pair.Value;
                            for (var i = 1; i < arguments.Length; i++)
                            {
                                foreach (var key in CombinedKeys(n, arguments[i]).ToArray())
                                {
                                    if (!n.TryGetValue(key, out var leftDerivative))
                                        leftDerivative = _parent.CreateValue(0.0, "");
                                    if (!arguments[i].TryGetValue(key, out var rightDerivative))
                                        rightDerivative = _parent.CreateValue(0.0, "");
                                    n[key] = _conditional.IfThenElse(
                                        _relational.GreaterThan(arguments[i].Value, n.Value), rightDerivative,
                                        _conditional.IfThenElse(
                                            _relational.GreaterThan(n.Value, arguments[i].Value), leftDerivative,
                                            _parent.CreateValue(0.0, "")));
                                }
                                n.Value = _parent.CreateFunction("Max", arguments[i].Value, n.Value);
                            }
                            return n;
                        case "min":
                        case "Min":
                            n.Value = arguments[0].Value;
                            foreach (var pair in arguments[0])
                                n[pair.Key] = pair.Value;
                            for (var i = 1; i < arguments.Length; i++)
                            {
                                foreach (var key in CombinedKeys(n, arguments[i]).ToArray())
                                {
                                    if (!n.TryGetValue(key, out var leftDerivative))
                                        leftDerivative = _parent.CreateValue(0.0, "");
                                    if (!arguments[i].TryGetValue(key, out var rightDerivative))
                                        rightDerivative = _parent.CreateValue(0.0, "");
                                    n[key] = _conditional.IfThenElse(
                                        _relational.LessThan(arguments[i].Value, n.Value), rightDerivative,
                                        _conditional.IfThenElse(
                                            _relational.LessThan(n.Value, arguments[i].Value), leftDerivative,
                                            _parent.CreateValue(0.0, "")));
                                }
                                n.Value = _parent.CreateFunction("Min", arguments[i].Value, n.Value);
                            }
                            return n;
                        case "round":
                        case "Round":
                            n.Value = _parent.CreateFunction("Round", arguments[0].Value, arguments[1].Value);
                            return n;
                        case "pow":
                        case "Pow":
                            n.Value = _arithmetic.Pow(arguments[0].Value, arguments[1].Value);
                            foreach (var key in CombinedKeys(arguments[0], arguments[1]))
                            {
                                var hasLeft = arguments[0].TryGetValue(key, out var leftDerivative);
                                var hasRight = arguments[1].TryGetValue(key, out var rightDerivative);
                                if (hasLeft && hasRight)
                                {
                                    n[key] = _arithmetic.Add(
                                        _arithmetic.Multiply(_arithmetic.Multiply(
                                            _arithmetic.Pow(arguments[0].Value, _arithmetic.Decrement(arguments[1].Value)), arguments[1].Value), leftDerivative),
                                        _arithmetic.Multiply(_arithmetic.Multiply(n.Value, _arithmetic.Log(arguments[0].Value)), rightDerivative)
                                        );
                                }
                                else if (hasLeft)
                                    n[key] = _arithmetic.Multiply(_arithmetic.Multiply(
                                        _arithmetic.Pow(arguments[0].Value, _arithmetic.Decrement(arguments[1].Value)), arguments[1].Value), leftDerivative);
                                else if (hasRight)
                                {
                                    n[key] = _arithmetic.Multiply(_arithmetic.Multiply(n.Value, _arithmetic.Log(arguments[0].Value)), rightDerivative);
                                }
                            }
                            return n;
                    }
                    break;
                default:
                    switch (name)
                    {
                        case "max":
                        case "Max":
                            n.Value = arguments[0].Value;
                            foreach (var pair in arguments[0])
                                n[pair.Key] = pair.Value;
                            for (var i = 1; i < arguments.Length; i++)
                            {
                                foreach (var key in CombinedKeys(n, arguments[i]).ToArray())
                                {
                                    if (!n.TryGetValue(key, out var leftDerivative))
                                        leftDerivative = _parent.CreateValue(0.0, "");
                                    if (!arguments[i].TryGetValue(key, out var rightDerivative))
                                        rightDerivative = _parent.CreateValue(0.0, "");
                                    n[key] = _conditional.IfThenElse(
                                        _relational.GreaterThan(arguments[i].Value, n.Value), rightDerivative,
                                        _conditional.IfThenElse(
                                            _relational.GreaterThan(n.Value, arguments[i].Value), leftDerivative,
                                            _parent.CreateValue(0.0, "")));
                                }
                                n.Value = _parent.CreateFunction("Max", arguments[i].Value, n.Value);
                            }
                            return n;
                        case "min":
                        case "Min":
                            n.Value = arguments[0].Value;
                            foreach (var pair in arguments[0])
                                n[pair.Key] = pair.Value;
                            for (var i = 1; i < arguments.Length; i++)
                            {
                                foreach (var key in CombinedKeys(n, arguments[i]).ToArray())
                                {
                                    if (!n.TryGetValue(key, out var leftDerivative))
                                        leftDerivative = _parent.CreateValue(0.0, "");
                                    if (!arguments[i].TryGetValue(key, out var rightDerivative))
                                        rightDerivative = _parent.CreateValue(0.0, "");
                                    n[key] = _conditional.IfThenElse(
                                        _relational.LessThan(arguments[i].Value, n.Value), rightDerivative,
                                        _conditional.IfThenElse(
                                            _relational.LessThan(n.Value, arguments[i].Value), leftDerivative,
                                            _parent.CreateValue(0.0, "")));
                                }
                                n.Value = _parent.CreateFunction("Min", arguments[i].Value, n.Value);
                            }
                            return n;
                    }
                    break;
            }
            throw new Exception("Unrecognized method");
        }

        /// <summary>
        /// Creates the value based on its textual representation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="units">The units of the value.</param>
        /// <returns>
        /// The value of the variable.
        /// </returns>
        public IDerivatives<K, V> CreateValue(double value, string units)
        {
            var n = _factory.Create();
            n.Value = _parent.CreateValue(value, units);
            return n;
        }

        /// <summary>
        /// Creates the variable based on its textual representation.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        /// The value of the variable.
        /// </returns>
        public IDerivatives<K, V> CreateVariable(string variable) => Variables[variable];
    }
}
