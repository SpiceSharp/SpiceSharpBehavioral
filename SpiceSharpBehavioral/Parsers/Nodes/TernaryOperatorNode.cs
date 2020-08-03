using SpiceSharp;
using System;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// A ternary operator.
    /// </summary>
    /// <seealso cref="Node" />
    public class TernaryOperatorNode : Node
    {
        /// <include file='docs.xml' path='docs/members/Conditional/*'/>
        public static new TernaryOperatorNode Conditional(Node condition, Node ifTrue, Node ifFalse) => new TernaryOperatorNode(NodeTypes.Conditional, condition, ifTrue, ifFalse);

        /// <summary>
        /// Gets the condition.
        /// </summary>
        /// <value>
        /// The condition.
        /// </value>
        public Node Condition { get; }

        /// <summary>
        /// Gets if true.
        /// </summary>
        /// <value>
        /// If true.
        /// </value>
        public Node IfTrue { get; }

        /// <summary>
        /// Gets if false.
        /// </summary>
        /// <value>
        /// If false.
        /// </value>
        public Node IfFalse { get; }

        /// <inheritdoc/>
        public override NodeProperties Properties { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TernaryOperatorNode"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="ifTrue">If true.</param>
        /// <param name="ifFalse">If false.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="condition"/>, <paramref name="ifTrue"/> or <paramref name="ifFalse"/> is <c>null</c>.</exception>
        protected TernaryOperatorNode(NodeTypes type, Node condition, Node ifTrue, Node ifFalse)
            : base(type)
        {
            Condition = condition.ThrowIfNull(nameof(condition));
            IfTrue = ifTrue.ThrowIfNull(nameof(ifTrue));
            IfFalse = ifFalse.ThrowIfNull(nameof(ifFalse));

            if (((Condition.Properties & NodeProperties.Constant) != 0) &&
                ((IfTrue.Properties & NodeProperties.Constant) != 0) &&
                ((IfFalse.Properties & NodeProperties.Constant) != 0))
            {
                Properties = NodeProperties.Constant;
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => NodeType.GetHashCode() ^ Condition.GetHashCode() ^ (IfTrue.GetHashCode() * 13) ^ (IfFalse.GetHashCode() * 13 * 13);

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
            if (obj is TernaryOperatorNode tn)
            {
                if (!Condition.Equals(tn.Condition))
                    return false;
                if (!IfTrue.Equals(tn.IfTrue))
                    return false;
                if (!IfFalse.Equals(tn.IfFalse))
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
            return "({0})?({1}):({2})".FormatString(Condition, IfTrue, IfFalse);
        }
    }
}
