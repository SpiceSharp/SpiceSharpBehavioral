using SpiceSharp;
using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Event arguments used when a voltage was found.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="EventArgs" />
    public class VoltageFoundEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the node name.
        /// </summary>
        /// <value>
        /// The node name.
        /// </value>
        public string Node { get; }

        /// <summary>
        /// Gets the reference node name.
        /// </summary>
        /// <value>
        /// The reference node name.
        /// </value>
        public string Reference { get; }

        /// <summary>
        /// Gets the type of the quantity.
        /// </summary>
        /// <value>
        /// The type of the quantity.
        /// </value>
        public QuantityTypes QuantityType { get; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
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
        /// Gets a value indicating whether this <see cref="VoltageFoundEventArgs{T}"/> is found.
        /// </summary>
        /// <value>
        ///   <c>true</c> if found; otherwise, <c>false</c>.
        /// </value>
        public bool Found { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageFoundEventArgs{T}"/> class.
        /// </summary>
        /// <param name="node">The node name.</param>
        /// <param name="reference">The reference node name.</param>
        /// <param name="type">The quantity type.</param>
        public VoltageFoundEventArgs(string node, string reference, QuantityTypes type)
        {
            Node = node.ThrowIfNull(nameof(node));
            Reference = reference;
            QuantityType = type;
        }
    }
}
