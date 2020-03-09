using SpiceSharp;
using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Event arguments used when a variable was found.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="EventArgs" />
    public class VariableFoundEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>
        /// The name of the variable.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the value that represents the variable.
        /// </summary>
        /// <value>
        /// The value of the variable.
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
        /// Gets a value indicating whether this <see cref="VariableFoundEventArgs{T}"/> is found.
        /// </summary>
        /// <value>
        ///   <c>true</c> if found; otherwise, <c>false</c>.
        /// </value>
        public bool Found { get; private set; }

        /// <summary>
        /// Gets the type of the quantity.
        /// </summary>
        /// <value>
        /// The type of the quantity.
        /// </value>
        public QuantityTypes QuantityType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableFoundEventArgs{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="type">The type of variable.</param>
        public VariableFoundEventArgs(string name, QuantityTypes type)
        {
            Name = name.ThrowIfNull(nameof(name));
            QuantityType = type;
            _result = default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableFoundEventArgs{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The default value of the variable.</param>
        public VariableFoundEventArgs(string name, T value)
        {
            Name = name.ThrowIfNull(nameof(name));
            _result = value;
        }
    }
}
