using SpiceSharp;
using SpiceSharpBehavioral.Parsers;
using System;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// Event arguments used when a variable was found.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="EventArgs" />
    public class PropertyFoundEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string Property { get; }

        /// <summary>
        /// Gets the name of the source entity.
        /// </summary>
        /// <value>
        /// The name of the source entity.
        /// </value>
        public string Source { get; }

        /// <summary>
        /// Gets the type of the quantity.
        /// </summary>
        /// <value>
        /// The type of the quantity.
        /// </value>
        public QuantityTypes QuantityType { get; set; }

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
        /// Initializes a new instance of the <see cref="VariableFoundEventArgs{T}"/> class.
        /// </summary>
        /// <param name="source">The name of the source entity.</param>
        /// <param name="property">The name of the property.</param>
        /// <param name="type">The quantity type.</param>
        public PropertyFoundEventArgs(string source, string property, QuantityTypes type)
        {
            Source = source.ThrowIfNull(nameof(source));
            Property = property.ThrowIfNull(nameof(property));
            _result = default;
            QuantityType = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyFoundEventArgs{T}"/> class.
        /// </summary>
        /// <param name="source">The name of the source entity.</param>
        /// <param name="property">The name of the property.</param>
        /// <param name="type">The quantity type.</param>
        /// <param name="value">The default value.</param>
        public PropertyFoundEventArgs(string source, string property, QuantityTypes type, T value)
        {
            Source = source.ThrowIfNull(nameof(source));
            Property = property.ThrowIfNull(nameof(property));
            _result = value;
            QuantityType = type;
        }
    }
}
