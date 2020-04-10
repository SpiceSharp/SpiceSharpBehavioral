using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// A constant node.
    /// </summary>
    /// <seealso cref="Node" />
    public class ConstantNode : Node
    {
        public static ConstantNode Constant(string literal) => new ConstantNode(NodeTypes.Constant, literal);

        /// <summary>
        /// Gets the literal.
        /// </summary>
        /// <value>
        /// The literal.
        /// </value>
        public string Literal { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantNode"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="literal">The literal.</param>
        protected ConstantNode(NodeTypes type, string literal)
            : base(type, NodeProperties.Constant | NodeProperties.Terminal)
        {
            Literal = literal.ThrowIfNull(nameof(literal));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => NodeType.GetHashCode() ^ Literal.GetHashCode();

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            if (obj is ConstantNode cn)
            {
                if (!Literal.Equals(cn.Literal))
                    return false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => Literal;
    }
}
