using SpiceSharp;
using SpiceSharp.Attributes;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.Operators
{
    /// <summary>
    /// An implementation for a value factory.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <seealso cref="IValueFactory{T}" />
    public abstract class ValueFactory<T> : ParameterSet, IValueFactory<T>
    {
        /// <summary>
        /// Occurs when a function was found.
        /// </summary>
        public event EventHandler<FunctionFoundEventArgs<T>> FunctionFound;

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        [ParameterName("variables"), ParameterInfo("The predefined variables")]
        public IDictionary<string, T> Variables { get; }

        /// <summary>
        /// Occurs when a variable was found.
        /// </summary>
        public event EventHandler<VariableFoundEventArgs<T>> VariableFound;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFactory{T}"/> class.
        /// </summary>
        protected ValueFactory()
        {
            Variables = new Dictionary<string, T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFactory{T}"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        protected ValueFactory(IDictionary<string, T> variables)
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
        /// <exception cref="Exception">Thrown if the function is not found.</exception>
        public virtual T CreateFunction(string name, params T[] arguments)
        {
            var args = new FunctionFoundEventArgs<T>(name, arguments);
            FunctionFound?.Invoke(this, args);
            if (args.Found)
                return args.Result;
            throw new Exception("Unrecognized function '{0}'".FormatString(name));
        }

        /// <summary>
        /// Creates the property.
        /// </summary>
        /// <param name="type">The type of property.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public abstract T CreateProperty(PropertyType type, IReadOnlyList<string> arguments);

        /// <summary>
        /// Creates the value based on its textual representation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="units">The units of the value.</param>
        /// <returns>
        /// The value of the variable.
        /// </returns>
        public abstract T CreateValue(double value, string units = "");

        /// <summary>
        /// Creates the variable based on its textual representation.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        /// The value of the variable.
        /// </returns>
        /// <exception cref="Exception">Unrecognized variable '{0}'".FormatString(variable)</exception>
        public T CreateVariable(string variable)
        {
            // First try finding the variable in the dictionary
            if (Variables.TryGetValue(variable, out T result))
                return result;

            // Let's ask around
            var args = new VariableFoundEventArgs<T>(variable);
            VariableFound?.Invoke(this, args);
            if (args.Found)
                return args.Result;

            // No variable found
            throw new Exception("Unrecognized variable '{0}'".FormatString(variable));
        }

        /// <summary>
        /// A delegate for a standalone value factory.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The factory.</returns>
        public delegate T FunctionDeclaration(IReadOnlyList<T> arguments);
    }
}
