using SpiceSharp;
using SpiceSharpBehavioral.Parsers;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Components.DoubleFunc
{
    /// <summary>
    /// A value factory for double functions.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IValueFactory{T}" />
    public class ValueFactory : ParameterSet, IValueFactory<Func<double>>
    {
        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IDictionary<string, Func<double>> Variables { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFactory"/> class.
        /// </summary>
        public ValueFactory()
        {
            Variables = new Dictionary<string, Func<double>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFactory"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public ValueFactory(IEqualityComparer<string> comparer)
        {
            Variables = new Dictionary<string, Func<double>>(comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFactory"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public ValueFactory(IDictionary<string, Func<double>> variables)
        {
            Variables = variables.ThrowIfNull(nameof(variables));
        }

        /// <summary>
        /// Creates a function of the specified name.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="arguments">The arguments of the function.</param>
        /// <returns>
        /// The value of the function.
        /// </returns>
        public Func<double> CreateFunction(string name, Func<double>[] arguments)
        {
            switch (arguments.Length)
            {
                case 0:
                    break;
                case 1:
                    var arg = arguments[0];
                    switch (name)
                    {
                        case "abs":
                        case "Abs":
                            return () => Math.Abs(arg());
                        case "sgn":
                        case "Sgn":
                            return () => Math.Sign(arg());

                        case "sqrt":
                        case "Sqrt":
                            return () => Math.Sqrt(arg());
                        case "exp":
                        case "Exp":
                            return () => Math.Exp(arg());
                        case "log":
                        case "Log":
                        case "ln":
                        case "Ln":
                            return () => Math.Log(arg());
                        case "log10":
                        case "Log10":
                            return () => Math.Log10(arg());
                        case "round":
                        case "Round":
                            return () => Math.Round(arg());

                        // Trigonometry
                        case "sin":
                        case "Sin":
                            return () => Math.Sin(arg());
                        case "cos":
                        case "Cos":
                            return () => Math.Cos(arg());
                        case "tan":
                        case "Tan":
                            return () => Math.Tan(arg());

                        // Inverse trigonometry
                        case "asin":
                        case "Asin":
                            return () => Math.Asin(arg());
                        case "acos":
                        case "Acos":
                            return () => Math.Acos(arg());
                        case "atan":
                        case "Atan":
                            return () => Math.Atan(arg());

                        // Hyperbolic functions
                        case "sinh":
                        case "Sinh":
                            return () => Math.Sinh(arg());
                        case "cosh":
                        case "Cosh":
                            return () => Math.Cosh(arg());
                        case "tanh":
                        case "Tanh":
                            return () => Math.Tanh(arg());
                    }
                    break;
                case 2:
                    arg = arguments[0];
                    var arg2 = arguments[1];
                    switch (name)
                    {
                        case "max":
                        case "Max":
                            return () => Math.Max(arg(), arg2());
                        case "min":
                        case "Min":
                            return () => Math.Min(arg(), arg2());
                        case "round":
                        case "Round":
                            return () => Math.Round(arg(), (int)(arg2() + 0.5));
                        case "pow":
                        case "Pow":
                            return () => Math.Pow(arg(), arg2());
                    }
                    break;
                default:
                    switch (name)
                    {
                        case "max":
                        case "Max":
                            return () =>
                            {
                                var result = arguments[0]();
                                for (var i = 1; i < arguments.Length; i++)
                                    result = Math.Max(result, arguments[i]());
                                return result;
                            };
                        case "min":
                        case "Min":
                            return () =>
                            {
                                var result = arguments[0]();
                                for (var i = 1; i < arguments.Length; i++)
                                    result = Math.Min(result, arguments[i]());
                                return result;
                            };
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
        public Func<double> CreateValue(double value, string units) => () => value;

        /// <summary>
        /// Creates the variable based on its textual representation.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        /// The value of the variable.
        /// </returns>
        public Func<double> CreateVariable(string variable) => Variables[variable];

        /// <summary>
        /// Creates the property.
        /// </summary>
        /// <param name="type">The type of property.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public Func<double> CreateProperty(PropertyType type, IReadOnlyList<string> arguments)
        {
            throw new NotSupportedException();
        }
    }
}
