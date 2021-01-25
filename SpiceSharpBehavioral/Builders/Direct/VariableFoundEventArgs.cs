using SpiceSharp;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;

namespace SpiceSharpBehavioral.Builders.Direct
{
    /// <summary>
    /// Event arguments used when a variable was found.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public class VariableFoundEventArgs<T> : EventArgs
    {
        private T _result;

        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>
        /// The builder.
        /// </value>
        public IDirectBuilder<T> Builder { get; }

        /// <summary>
        /// Gets the variable.
        /// </summary>
        /// <value>
        /// The variable.
        /// </value>
        public VariableNode Node { get; }

        /// <summary>
        /// Gets or sets the result of the variable.
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
                Created = true;
            }
        }

        /// <summary>
        /// Gets whether or not a variable has received a value.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the variable has received a value; otherwise, <c>false</c>.
        /// </value>
        public bool Created { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableFoundEventArgs{T}"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="node">The variable node.</param>
        /// <exception cref="ArgumentNullException">Throw if <paramref name="builder"/> or <paramref name="node"/> is <c>null</c>.</exception>
        public VariableFoundEventArgs(IDirectBuilder<T> builder, VariableNode node)
        {
            Builder = builder.ThrowIfNull(nameof(builder));
            Node = node.ThrowIfNull(nameof(node));
            Created = false;
            _result = default;
        }
    }
}
