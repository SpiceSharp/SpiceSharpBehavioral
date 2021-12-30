using SpiceSharp;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;

namespace SpiceSharpBehavioral.Builders.Functions
{
    /// <summary>
    /// Event arguments for when a function has been found.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public class FunctionFoundEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the function node.
        /// </summary>
        /// <value>
        /// The function node.
        /// </value>
        public FunctionNode Function { get; }

        /// <summary>
        /// Gets the IL state for creating the function.
        /// </summary>
        /// <value>
        /// The IL state.
        /// </value>
        public IILState<T> ILState { get; }

        /// <summary>
        /// Gets or sets a flag whether or not the function has been created.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the function was created; otherwise, <c>false</c>.
        /// </value>
        public bool Created { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionFoundEventArgs{T}"/> class.
        /// </summary>
        /// <param name="function">The function node.</param>
        /// <param name="ilState">The IL state.</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is <c>null</c>.</exception>
        public FunctionFoundEventArgs(FunctionNode function, IILState<T> ilState)
        {
            Function = function.ThrowIfNull(nameof(function));
            ILState = ilState.ThrowIfNull(nameof(ilState));
        }
    }
}
