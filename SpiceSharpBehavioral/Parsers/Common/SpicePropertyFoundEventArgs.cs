using SpiceSharp;
using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Event arguments used when a Spice property is found.
    /// </summary>
    /// <typeparam name="T">The base value.</typeparam>
    public abstract class SpicePropertyFoundEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the property.
        /// </summary>
        public SpiceProperty Property { get; }

        /// <summary>
        /// Gets or sets whether the method has been applied or not.
        /// </summary>
        public bool Applied { get; protected set; }

        /// <summary>
        /// Apply a method as the result.
        /// </summary>
        /// <param name="value">The function that returns the value for the current iteration.</param>
        /// <param name="index">The derivative index to apply to.</param>
        /// <param name="derivative">The value of the derivative to apply to.</param>
        public abstract void Apply(Func<double> value, int index, double derivative);

        /// <summary>
        /// Apply a method as the result.
        /// </summary>
        /// <param name="value">The function that returns the value for the current iteration.</param>
        public virtual void Apply(Func<double> value) => Apply(value, 0, 0.0);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="property">The Spice property.</param>
        public SpicePropertyFoundEventArgs(SpiceProperty property)
        {
            Property = property.ThrowIfNull(nameof(property));
        }
    }
}
