using SpiceSharp;
using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Event arguments used when a Spice property is found.
    /// </summary>
    /// <typeparam name="T">The base value.</typeparam>
    public class SpicePropertyFoundEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the property.
        /// </summary>
        public SpiceProperty Property { get; }

        /// <summary>
        /// Gets or sets the resulting value.
        /// </summary>
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
        /// Gets whether or not the property has been found.
        /// </summary>
        public bool Found { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="property">The Spice property.</param>
        /// <param name="defaultValue">The default value.</param>
        public SpicePropertyFoundEventArgs(SpiceProperty property, T defaultValue)
        {
            Property = property.ThrowIfNull(nameof(property));
            _result = defaultValue;
            Found = false;
        }
    }
}
