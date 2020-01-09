using SpiceSharp;
using System;

namespace SpiceSharpBehavioral.Parsers.ShuntingYard
{
    /// <summary>
    /// Event arguments that are used when a variable has been found.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <seealso cref="EventArgs" />
    public class VariableFoundEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>
        /// The variable name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the variable was found.
        /// </summary>
        /// <value>
        ///   <c>true</c> if found; otherwise, <c>false</c>.
        /// </value>
        public bool Found { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public T Result
        {
            get => _result;
            set
            {
                Found = true;
                _result = value;
            }
        }
        private T _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableFoundEventArgs{T}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public VariableFoundEventArgs(string name)
        {
            Name = name.ThrowIfNull(nameof(name));
        }
    }
}
