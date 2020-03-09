using SpiceSharp;
using SpiceSharpBehavioral.Parsers;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// A base implementation for a builder.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public abstract class BaseBuilder<T>
    {
        /// <summary>
        /// Occurs when a variable was found.
        /// </summary>
        public event EventHandler<VariableFoundEventArgs<T>> VariableFound;

        /// <summary>
        /// Occurs when an unknown function was found.
        /// </summary>
        public event EventHandler<FunctionFoundEventArgs<T>> FunctionFound;

        /// <summary>
        /// Occurs when a voltage was found.
        /// </summary>
        public event EventHandler<VoltageFoundEventArgs<T>> VoltageFound;

        /// <summary>
        /// Occurs when a current was found.
        /// </summary>
        public event EventHandler<CurrentFoundEventArgs<T>> CurrentFound;

        /// <summary>
        /// Occurs when a property was found.
        /// </summary>
        public event EventHandler<PropertyFoundEventArgs<T>> PropertyFound;

        /// <summary>
        /// Creates the value of a number.
        /// </summary>
        /// <param name="value">The number.</param>
        /// <returns>
        /// The value of the number.
        /// </returns>
        public abstract T CreateNumber(string value);

        /// <summary>
        /// Creates the value of a function.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The value of the function.
        /// </returns>
        /// <exception cref="Exception">Could not find function '{0}' with {1} arguments".FormatString(name, arguments.Count)</exception>
        public virtual T CreateFunction(string name, IReadOnlyList<T> arguments)
        {
            var args = new FunctionFoundEventArgs<T>(name, arguments);
            FunctionFound?.Invoke(this, args);
            if (!args.Found || args.Result == null)
                throw new Exception("Could not find function '{0}' with {1} arguments".FormatString(name, arguments.Count));
            return args.Result;
        }

        /// <summary>
        /// Creates the variable.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public virtual T CreateVariable(string name, QuantityTypes type = QuantityTypes.Raw)
        {
            var args = new VariableFoundEventArgs<T>(name, type);
            VariableFound?.Invoke(this, args);
            if (!args.Found || args.Result == null)
                throw new Exception("Could not find variable '{0}'".FormatString(name));
            return args.Result;
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
        public virtual T CreateVoltage(string node, string reference, QuantityTypes type)
        {
            var args = new VoltageFoundEventArgs<T>(node.ThrowIfNull(nameof(node)), reference, type);
            VoltageFound?.Invoke(this, args);
            if (!args.Found || args.Result == null)
                throw new Exception("Could not find voltage ('{0}','{1}')".FormatString(node, reference));
            return args.Result;
        }

        /// <summary>
        /// Creates the value of a current.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="type">The quantity type.</param>
        /// <returns>
        /// The value of the current.
        /// </returns>
        public virtual T CreateCurrent(string name, QuantityTypes type)
        {
            var args = new CurrentFoundEventArgs<T>(name, type);
            CurrentFound?.Invoke(this, args);
            if (!args.Found || args.Result == null)
                throw new Exception("Could not find current '{0}'".FormatString(name));
            return args.Result;
        }

        /// <summary>
        /// Creates a value of a property.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="property">The name of the property.</param>
        /// <param name="type">The quantity type.</param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public virtual T CreateProperty(string name, string property, QuantityTypes type)
        {
            var args = new PropertyFoundEventArgs<T>(name, property, type);
            PropertyFound?.Invoke(this, args);
            if (!args.Found || args.Result == null)
                throw new Exception("Could not find property '{0}' for '{1}'".FormatString(name, property));
            return args.Result;
        }
    }
}
