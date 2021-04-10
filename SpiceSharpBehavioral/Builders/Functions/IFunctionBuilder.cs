using System;

namespace SpiceSharpBehavioral.Builders.Functions
{
    /// <summary>
    /// A function builder.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public interface IFunctionBuilder<T> : IBuilder<Func<T>>
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
