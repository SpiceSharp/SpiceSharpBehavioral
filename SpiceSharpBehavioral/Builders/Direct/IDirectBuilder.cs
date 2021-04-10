using System;

namespace SpiceSharpBehavioral.Builders.Direct
{
    /// <summary>
    /// A direct builder.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public interface IDirectBuilder<T> : IBuilder<T>
    {
        /// <summary>
        /// Gets or sets the relative tolerance.
        /// </summary>
        /// <value>
        /// The relative tolerance.
        /// </value>
        double RelativeTolerance { get; set; }

        /// <summary>
        /// Gets or sets the absolute tolerance.
        /// </summary>
        /// <value>
        /// The absolute tolerance.
        /// </value>
        double AbsoluteTolerance { get; set; }

        /// <summary>
        /// Occurs when a function is encountered.
        /// </summary>
        event EventHandler<FunctionFoundEventArgs<T>> FunctionFound;

        /// <summary>
        /// Occurs when a variable is encountered.
        /// </summary>
        event EventHandler<VariableFoundEventArgs<T>> VariableFound;
    }
}
