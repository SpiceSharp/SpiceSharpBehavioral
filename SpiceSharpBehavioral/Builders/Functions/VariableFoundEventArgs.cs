using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;

namespace SpiceSharpBehavioral.Builders.Functions
{
    /// <summary>
    /// Event arguments for when a variable was found.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public class VariableFoundEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the variable node.
        /// </summary>
        /// <value>
        /// The variable node.
        /// </value>
        public VariableNode Node { get; }

        /// <summary>
        /// Gets or sets the variable.
        /// </summary>
        /// <value>
        /// The variable.
        /// </value>
        public IVariable<T> Variable { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableFoundEventArgs{T}"/> class.
        /// </summary>
        /// <param name="node">The variable node.</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is <c>null</c>.</exception>
        public VariableFoundEventArgs(VariableNode node)
        {
            Node = node.ThrowIfNull(nameof(node));
            Variable = null;
        }
    }
}
