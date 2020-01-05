using SpiceSharp;
using System;

namespace SpiceSharpBehavioral.Parsers.Operators
{
    /// <summary>
    /// Event arguments used when a variable was found.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <seealso cref="EventArgs" />
    public class VariableFoundEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
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
        /// Gets or sets the variable value.
        /// </summary>
        /// <value>
        /// The variable value.
        /// </value>
        public T Result 
        {
            get => _result;
            set
            {
                _result = value;
                Found = true;
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
            Found = false;
            _result = default;
        }
    }
}
