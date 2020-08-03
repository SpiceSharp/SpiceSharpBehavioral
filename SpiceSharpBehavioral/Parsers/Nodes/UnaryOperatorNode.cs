using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// A unary operator.
    /// </summary>
    /// <seealso cref="Node" />
    public class UnaryOperatorNode : Node
    {
        /// <include file='docs.xml' path='docs/members/Unary/*'/>
        /// <include file='docs.xml' path='docs/members/Plus/*'/>
        public static new UnaryOperatorNode Plus(Node argument) => new UnaryOperatorNode(NodeTypes.Plus, argument);

        /// <include file='docs.xml' path='docs/members/Unary/*'/>
        /// <include file='docs.xml' path='docs/members/Minus/*'/>
        public static new UnaryOperatorNode Minus(Node argument) => new UnaryOperatorNode(NodeTypes.Minus, argument);

        /// <include file='docs.xml' path='docs/members/Unary/*'/>
        /// <include file='docs.xml' path='docs/members/Bang/*'/>
        public static new UnaryOperatorNode Not(Node argument) => new UnaryOperatorNode(NodeTypes.Not, argument);

        /// <summary>
        /// Gets the argument.
        /// </summary>
        /// <value>
        /// The argument.
        /// </value>
        public Node Argument { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public override NodeProperties Properties { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnaryOperatorNode"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="argument">The argument.</param>
        protected UnaryOperatorNode(NodeTypes type, Node argument)
            : base(type)
        {
            Argument = argument.ThrowIfNull(nameof(argument));

            if ((Argument.Properties & NodeProperties.Constant) != 0)
                Properties = NodeProperties.Constant;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => NodeType.GetHashCode() ^ Argument.GetHashCode();

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
            if (obj is UnaryOperatorNode un)
            {
                if (NodeType != un.NodeType)
                    return false;
                if (!Argument.Equals(un.Argument))
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
        public override string ToString()
        {
            var op = NodeType switch
            {
                NodeTypes.Plus => "+",
                NodeTypes.Minus => "-",
                NodeTypes.Not => "!",
                _ => "???"
            };
            return "{0}({1})".FormatString(op, Argument);
        }
    }
}
