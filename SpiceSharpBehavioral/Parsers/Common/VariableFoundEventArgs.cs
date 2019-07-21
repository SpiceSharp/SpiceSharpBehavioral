using SpiceSharp;
using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Event arguments used when a variable was found.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public class VariableFoundEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="defaultValue">The default value.</param>
        public VariableFoundEventArgs(string name, T defaultValue)
        {
            Name = name.ThrowIfNull(nameof(name));
            Result = defaultValue;
        }
    }
}
