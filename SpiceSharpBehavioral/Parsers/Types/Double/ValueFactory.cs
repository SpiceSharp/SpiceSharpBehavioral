using SpiceSharp;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.Double
{
    /// <summary>
    /// A value factory for doubles.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IValueFactory{Double}" />
    public class ValueFactory : ParameterSet, IValueFactory<double>
    {
        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IDictionary<string, double> Variables { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFactory"/> class.
        /// </summary>
        public ValueFactory()
        {
            Variables = new Dictionary<string, double>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFactory"/> class.
        /// </summary>
        /// <param name="comparer">The comparer for strings.</param>
        public ValueFactory(IEqualityComparer<string> comparer)
        {
            Variables = new Dictionary<string, double>(comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFactory"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public ValueFactory(IDictionary<string, double> variables)
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
        public virtual double CreateFunction(string name, double[] arguments)
        {
            double result;
            switch (arguments.Length)
            {
                case 0:
                    break;
                case 1:
                    switch (name)
                    {
                        case "abs":
                        case "Abs":
                            return Math.Abs(arguments[0]);
                        case "sgn":
                        case "Sgn":
                            return Math.Sign(arguments[0]);

                        case "sqrt":
                        case "Sqrt":
                            return Math.Sqrt(arguments[0]);
                        case "exp":
                        case "Exp":
                            return Math.Exp(arguments[0]);
                        case "log":
                        case "Log":
                        case "ln":
                        case "Ln":
                            return Math.Log(arguments[0]);
                        case "log10":
                        case "Log10":
                            return Math.Log10(arguments[0]);
                        case "round":
                        case "Round":
                            return Math.Round(arguments[0]);

                        // Trigonometry
                        case "sin":
                        case "Sin":
                            return Math.Sin(arguments[0]);
                        case "cos":
                        case "Cos":
                            return Math.Cos(arguments[0]);
                        case "tan":
                        case "Tan":
                            return Math.Tan(arguments[0]);

                        // Inverse trigonometry
                        case "asin":
                        case "Asin":
                            return Math.Asin(arguments[0]);
                        case "acos":
                        case "Acos":
                            return Math.Acos(arguments[0]);
                        case "atan":
                        case "Atan":
                            return Math.Atan(arguments[0]);

                        // Hyperbolic functions
                        case "sinh":
                        case "Sinh":
                            return Math.Sinh(arguments[0]);
                        case "cosh":
                        case "Cosh":
                            return Math.Cosh(arguments[0]);
                        case "tanh":
                        case "Tanh":
                            return Math.Tanh(arguments[0]);
                    }
                    break;
                case 2:
                    switch (name)
                    {
                        case "max":
                        case "Max":
                            return Math.Max(arguments[0], arguments[1]);
                        case "min":
                        case "Min":
                            return Math.Min(arguments[0], arguments[1]);
                        case "round":
                        case "Round":
                            return Math.Round(arguments[0], (int)(arguments[1] + 0.5));
                        case "pow":
                        case "Pow":
                            return Math.Pow(arguments[0], arguments[1]);
                    }
                    break;
                default:
                    switch (name)
                    {
                        case "max":
                        case "Max":
                            result = arguments[0];
                            for (var i = 1; i < arguments.Length; i++)
                                result = Math.Max(result, arguments[i]);
                            return result;
                        case "min":
                        case "Min":
                            result = arguments[0];
                            for (var i = 1; i < arguments.Length; i++)
                                result = Math.Min(result, arguments[i]);
                            return result;
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
        public double CreateValue(double value, string units) => value;

        /// <summary>
        /// Creates the variable based on its textual representation.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        /// The value of the variable.
        /// </returns>
        public double CreateVariable(string variable) => Variables[variable];

        /// <summary>
        /// Creates the property.
        /// </summary>
        /// <param name="type">The type of property.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public double CreateProperty(PropertyType type, IReadOnlyList<string> arguments)
        {
            throw new NotSupportedException();
        }
    }
}
