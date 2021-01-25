using SpiceSharp;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;

namespace SpiceSharpBehavioral.Builders.Direct
{
    /// <summary>
    /// Event arguments for when a function has been found.
    /// </summary>
    /// <typeparam param="T">The value type.</typeparam>
    public class FunctionFoundEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>
        /// The builder.
        /// </value>
        public IDirectBuilder<T> Builder { get; }

        /// <summary>
        /// The function.
        /// </summary>
        public FunctionNode Function { get; }

        /// <summary>
        /// Gets or sets the result of the function.
        /// </summary>
        public T Result
        {
            get => _result;
            set
            {
                _result = value;
                Created = true;
            }
        }
        private T _result;

        /// <summary>
        /// Gets a flag whether ojr not the function result has been assigned.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the function result was assigned; otherwise, <c>false</c>.
        /// </value>
        public bool Created { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionFoundEventArgs{T}"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="function">The function node.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="function"/> is <c>null</c>.</exception>
        public FunctionFoundEventArgs(IDirectBuilder<T> builder, FunctionNode function)
        {
            Builder = builder.ThrowIfNull(nameof(builder));
            Function = function.ThrowIfNull(nameof(function));
            _result = default;
            Created = false;
        }
    }
}
