using SpiceSharp;
using SpiceSharp.Attributes;
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
    public class ValueFactory<K, V> : ParameterSet, IValueFactory<IDerivatives<K, V>>
    {
        private readonly IParserParameters<V> _parent;
        private readonly IDerivativeFactory<K, V> _factory;
        
        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        [ParameterName("variables"), ParameterInfo("The variables that are defined for derivatives")]
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
        /// <param name="parent">The parent parser parameters.</param>
        /// <param name="factory">The factory for derivatives.</param>
        public ValueFactory(IParserParameters<V> parent, IDerivativeFactory<K, V> factory)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
            _factory = factory.ThrowIfNull(nameof(factory));
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
                            n.Value = _parent.ValueFactory.CreateFunction("Abs", arguments[0].Value);
                            foreach (var pair in arguments[0])
                            {
                                n[pair.Key] = _parent.Arithmetic.Multiply(
                                    _parent.ValueFactory.CreateFunction("sgn", arguments[0].Value),
                                    pair.Value);
                            }
                            return n;
                        case "sqrt":
                        case "Sqrt":
                            n.Value = _parent.ValueFactory.CreateFunction("Sqrt", arguments[0].Value);
                            foreach (var pair in arguments[0])
                            {
                                n[pair.Key] = _parent.Arithmetic.Divide(
                                    pair.Value,
                                    _parent.Arithmetic.Multiply(_parent.ValueFactory.CreateValue(2.0, ""), n.Value));
                            }
                            return n;
                        case "exp":
                        case "Exp":
                            n.Value = _parent.ValueFactory.CreateFunction("Exp", arguments[0].Value);
                            foreach (var pair in arguments[0])
                                n[pair.Key] = _parent.Arithmetic.Multiply(n.Value, pair.Value);
                            return n;
                        case "log":
                        case "Log":
                        case "ln":
                        case "Ln":
                            n.Value = _parent.ValueFactory.CreateFunction("Log", arguments[0].Value);
                            foreach (var pair in arguments[0])
                                n[pair.Key] = _parent.Arithmetic.Divide(pair.Value, arguments[0].Value);
                            return n;
                        case "log10":
                        case "Log10":
                            n.Value = _parent.ValueFactory.CreateFunction("Log10", arguments[0].Value);
                            foreach (var pair in arguments[0])
                                n[pair.Key] = _parent.Arithmetic.Divide(pair.Value, _parent.Arithmetic.Multiply(_parent.ValueFactory.CreateValue(Math.Log(10.0), ""), arguments[0].Value));
                            return n;
                        case "round":
                        case "Round":
                            n.Value = _parent.ValueFactory.CreateFunction("Round", arguments[0].Value);
                            return n;

                        // Trigonometry
                        case "sin":
                        case "Sin":
                            n.Value = _parent.ValueFactory.CreateFunction("Sin", arguments[0].Value);
                            foreach (var pair in arguments[0])
                                n[pair.Key] = _parent.Arithmetic.Multiply(_parent.ValueFactory.CreateFunction("Cos", arguments[0].Value), pair.Value);
                            return n;
                        case "cos":
                        case "Cos":
                            n.Value = _parent.ValueFactory.CreateFunction("Cos", arguments[0].Value);
                            foreach (var pair in arguments[0])
                                n[pair.Key] = _parent.Arithmetic.Negate(_parent.Arithmetic.Multiply(_parent.ValueFactory.CreateFunction("Sin", arguments[0].Value), pair.Value));
                            return n;
                        case "tan":
                        case "Tan":
                            n.Value = _parent.ValueFactory.CreateFunction("Tan", arguments[0].Value);
                            foreach (var pair in arguments[0])
                            {
                                var cos = _parent.ValueFactory.CreateFunction("Cos", arguments[0].Value);
                                n[pair.Key] = _parent.Arithmetic.Divide(pair.Value, _parent.Arithmetic.Pow(cos, 2));
                            }
                            return n;

                        // Inverse trigonometry
                        case "asin":
                        case "Asin":
                            n.Value = _parent.ValueFactory.CreateFunction("Asin", arguments[0].Value);
                            foreach (var pair in arguments[0])
                            {
                                n[pair.Key] = _parent.Arithmetic.Divide(pair.Value, 
                                    _parent.ValueFactory.CreateFunction("Sqrt",
                                        _parent.Arithmetic.Subtract(
                                            _parent.ValueFactory.CreateValue(1.0, ""), 
                                            _parent.Arithmetic.Pow(arguments[0].Value, 2)
                                            )
                                        )
                                    );
                            }
                            return n;
                        case "acos":
                        case "Acos":
                            n.Value = _parent.ValueFactory.CreateFunction("Acos", arguments[0].Value);
                            foreach (var pair in arguments[0])
                            {
                                n[pair.Key] = _parent.Arithmetic.Divide(_parent.Arithmetic.Negate(pair.Value),
                                    _parent.ValueFactory.CreateFunction("Sqrt",
                                        _parent.Arithmetic.Subtract(
                                            _parent.ValueFactory.CreateValue(1.0, ""),
                                            _parent.Arithmetic.Pow(arguments[0].Value, 2)
                                            )
                                        )
                                    );
                            }
                            return n;
                        case "atan":
                        case "Atan":
                            n.Value = _parent.ValueFactory.CreateFunction("Atan", arguments[0].Value);
                            foreach (var pair in arguments[0])
                            {
                                n[pair.Key] = _parent.Arithmetic.Divide(pair.Value,
                                    _parent.Arithmetic.Add(
                                        _parent.ValueFactory.CreateValue(1.0, ""),
                                        _parent.Arithmetic.Pow(arguments[0].Value, 2)));
                            }
                            return n;

                        // Hyperbolic functions
                        case "sinh":
                        case "Sinh":
                            n.Value = _parent.ValueFactory.CreateFunction("Sinh", arguments[0].Value);
                            foreach (var pair in arguments[0])
                                n[pair.Key] = _parent.Arithmetic.Multiply(_parent.ValueFactory.CreateFunction("Cosh", arguments[0].Value), pair.Value);
                            return n;
                        case "cosh":
                        case "Cosh":
                            n.Value = _parent.ValueFactory.CreateFunction("Cosh", arguments[0].Value);
                            foreach (var pair in arguments[0])
                                n[pair.Key] = _parent.Arithmetic.Multiply(_parent.ValueFactory.CreateFunction("Sinh", arguments[0].Value), pair.Value);
                            return n;
                        case "tanh":
                        case "Tanh":
                            n.Value = _parent.ValueFactory.CreateFunction("Tanh", arguments[0].Value);
                            foreach (var pair in arguments[0])
                            {
                                var cosh = _parent.ValueFactory.CreateFunction("Cosh", arguments[0].Value);
                                n[pair.Key] = _parent.Arithmetic.Divide(pair.Value, _parent.Arithmetic.Pow(cosh, 2));
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
                                        leftDerivative = _parent.ValueFactory.CreateValue(0.0, "");
                                    if (!arguments[i].TryGetValue(key, out var rightDerivative))
                                        rightDerivative = _parent.ValueFactory.CreateValue(0.0, "");
                                    n[key] = _parent.Conditional.IfThenElse(
                                        _parent.Relational.GreaterThan(arguments[i].Value, n.Value), rightDerivative,
                                        _parent.Conditional.IfThenElse(
                                            _parent.Relational.GreaterThan(n.Value, arguments[i].Value), leftDerivative,
                                            _parent.ValueFactory.CreateValue(0.0, "")));
                                }
                                n.Value = _parent.ValueFactory.CreateFunction("Max", arguments[i].Value, n.Value);
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
                                        leftDerivative = _parent.ValueFactory.CreateValue(0.0, "");
                                    if (!arguments[i].TryGetValue(key, out var rightDerivative))
                                        rightDerivative = _parent.ValueFactory.CreateValue(0.0, "");
                                    n[key] = _parent.Conditional.IfThenElse(
                                        _parent.Relational.LessThan(arguments[i].Value, n.Value), rightDerivative,
                                        _parent.Conditional.IfThenElse(
                                            _parent.Relational.LessThan(n.Value, arguments[i].Value), leftDerivative,
                                            _parent.ValueFactory.CreateValue(0.0, "")));
                                }
                                n.Value = _parent.ValueFactory.CreateFunction("Min", arguments[i].Value, n.Value);
                            }
                            return n;
                        case "round":
                        case "Round":
                            n.Value = _parent.ValueFactory.CreateFunction("Round", arguments[0].Value, arguments[1].Value);
                            return n;
                        case "pow":
                        case "Pow":
                            n.Value = _parent.Arithmetic.Pow(arguments[0].Value, arguments[1].Value);
                            foreach (var key in CombinedKeys(arguments[0], arguments[1]))
                            {
                                var hasLeft = arguments[0].TryGetValue(key, out var leftDerivative);
                                var hasRight = arguments[1].TryGetValue(key, out var rightDerivative);
                                if (hasLeft && hasRight)
                                {
                                    n[key] = _parent.Arithmetic.Add(
                                        _parent.Arithmetic.Multiply(_parent.Arithmetic.Multiply(
                                            _parent.Arithmetic.Pow(arguments[0].Value, _parent.Arithmetic.Decrement(arguments[1].Value)), arguments[1].Value), leftDerivative),
                                        _parent.Arithmetic.Multiply(_parent.Arithmetic.Multiply(n.Value, _parent.Arithmetic.Log(arguments[0].Value)), rightDerivative)
                                        );
                                }
                                else if (hasLeft)
                                    n[key] = _parent.Arithmetic.Multiply(_parent.Arithmetic.Multiply(
                                        _parent.Arithmetic.Pow(arguments[0].Value, _parent.Arithmetic.Decrement(arguments[1].Value)), arguments[1].Value), leftDerivative);
                                else if (hasRight)
                                {
                                    n[key] = _parent.Arithmetic.Multiply(_parent.Arithmetic.Multiply(n.Value, _parent.Arithmetic.Log(arguments[0].Value)), rightDerivative);
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
                                        leftDerivative = _parent.ValueFactory.CreateValue(0.0, "");
                                    if (!arguments[i].TryGetValue(key, out var rightDerivative))
                                        rightDerivative = _parent.ValueFactory.CreateValue(0.0, "");
                                    n[key] = _parent.Conditional.IfThenElse(
                                        _parent.Relational.GreaterThan(arguments[i].Value, n.Value), rightDerivative,
                                        _parent.Conditional.IfThenElse(
                                            _parent.Relational.GreaterThan(n.Value, arguments[i].Value), leftDerivative,
                                            _parent.ValueFactory.CreateValue(0.0, "")));
                                }
                                n.Value = _parent.ValueFactory.CreateFunction("Max", arguments[i].Value, n.Value);
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
                                        leftDerivative = _parent.ValueFactory.CreateValue(0.0, "");
                                    if (!arguments[i].TryGetValue(key, out var rightDerivative))
                                        rightDerivative = _parent.ValueFactory.CreateValue(0.0, "");
                                    n[key] = _parent.Conditional.IfThenElse(
                                        _parent.Relational.LessThan(arguments[i].Value, n.Value), rightDerivative,
                                        _parent.Conditional.IfThenElse(
                                            _parent.Relational.LessThan(n.Value, arguments[i].Value), leftDerivative,
                                            _parent.ValueFactory.CreateValue(0.0, "")));
                                }
                                n.Value = _parent.ValueFactory.CreateFunction("Min", arguments[i].Value, n.Value);
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
            n.Value = _parent.ValueFactory.CreateValue(value, units);
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

        /// <summary>
        /// Creates the property.
        /// </summary>
        /// <param name="type">The type of property.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public IDerivatives<K, V> CreateProperty(PropertyType type, IReadOnlyList<string> arguments)
        {
            throw new NotSupportedException();
        }
    }
}
