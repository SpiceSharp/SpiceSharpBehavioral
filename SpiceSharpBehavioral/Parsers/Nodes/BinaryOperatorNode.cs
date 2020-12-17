using SpiceSharp;
using System;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// A binary operator.
    /// </summary>
    /// <seealso cref="Node" />
    public class BinaryOperatorNode : Node
    {
        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Add/*'/>
        public static new BinaryOperatorNode Add(Node left, Node right) => new BinaryOperatorNode(NodeTypes.Add, left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Subtract/*'/>
        public static new BinaryOperatorNode Subtract(Node left, Node right) => new BinaryOperatorNode(NodeTypes.Subtract, left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Multiply/*'/>
        public static new BinaryOperatorNode Multiply(Node left, Node right) => new BinaryOperatorNode(NodeTypes.Multiply, left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Divide/*'/>
        public static new BinaryOperatorNode Divide(Node left, Node right) => new BinaryOperatorNode(NodeTypes.Divide, left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Modulo/*'/>
        public static new BinaryOperatorNode Modulo(Node left, Node right) => new BinaryOperatorNode(NodeTypes.Modulo, left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/And/*'/>
        public static new BinaryOperatorNode And(Node left, Node right) => new BinaryOperatorNode(NodeTypes.And, left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Or/*'/>
        public static new BinaryOperatorNode Or(Node left, Node right) => new BinaryOperatorNode(NodeTypes.Or, left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Xor/*'/>
        public static new BinaryOperatorNode Xor(Node left, Node right) => new BinaryOperatorNode(NodeTypes.Xor, left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Equals/*'/>
        public static new BinaryOperatorNode Equals(Node left, Node right) => new BinaryOperatorNode(NodeTypes.Equals, left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/NotEquals/*'/>
        public static new BinaryOperatorNode NotEquals(Node left, Node right) => new BinaryOperatorNode(NodeTypes.NotEquals, left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/LessThan/*'/>
        public static new BinaryOperatorNode LessThan(Node left, Node right) => new BinaryOperatorNode(NodeTypes.LessThan, left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/GreaterThan/*'/>
        public static new BinaryOperatorNode GreaterThan(Node left, Node right) => new BinaryOperatorNode(NodeTypes.GreaterThan, left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/LessThanOrEquals/*'/>
        public static new BinaryOperatorNode LessThanOrEqual(Node left, Node right) => new BinaryOperatorNode(NodeTypes.LessThanOrEqual, left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/GreaterThanOrEquals/*'/>
        public static new BinaryOperatorNode GreaterThanOrEqual(Node left, Node right) => new BinaryOperatorNode(NodeTypes.GreaterThanOrEqual, left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Power/*'/>
        public static new BinaryOperatorNode Power(Node left, Node right) => new BinaryOperatorNode(NodeTypes.Pow, left, right);

        /// <summary>
        /// Gets the left argument.
        /// </summary>
        /// <value>
        /// The left argument.
        /// </value>
        public Node Left { get; }

        /// <summary>
        /// Gets the right argument.
        /// </summary>
        /// <value>
        /// The right argument.
        /// </value>
        public Node Right { get; }

        /// <inheritdoc/>
        public override NodeProperties Properties { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperatorNode"/> class.
        /// </summary>
        /// <param name="type">The node type.</param>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="left"/> or <paramref name="right"/> is <c>null</c>.</exception>
        protected BinaryOperatorNode(NodeTypes type, Node left, Node right)
            : base(type)
        {
            Left = left.ThrowIfNull(nameof(left));
            Right = right.ThrowIfNull(nameof(right));

            // If both are constant, the result is also constant
            if ((Left.Properties & NodeProperties.Constant) != 0 && (Right.Properties & NodeProperties.Constant) != 0)
                Properties |= NodeProperties.Constant;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => NodeType.GetHashCode() ^ Left.GetHashCode() ^ (Right.GetHashCode() * 13);

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is BinaryOperatorNode bn)
            {
                if (NodeType != bn.NodeType)
                    return false;
                if (!Left.Equals(bn.Left))
                    return false;
                if (!Right.Equals(bn.Right))
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
            string op = NodeType switch
            {
                NodeTypes.Add => "+",
                NodeTypes.Subtract => "-",
                NodeTypes.Multiply => "*",
                NodeTypes.Divide => "/",
                NodeTypes.Modulo => "%",
                NodeTypes.And => "&",
                NodeTypes.Or => "|",
                NodeTypes.Xor => "^",
                NodeTypes.GreaterThan => ">",
                NodeTypes.LessThan => "<",
                NodeTypes.GreaterThanOrEqual => ">=",
                NodeTypes.LessThanOrEqual => "<=",
                NodeTypes.Equals => "==",
                NodeTypes.NotEquals => "!=",
                NodeTypes.Pow => "^",
                _ => "???"
            };
            return "({0}){1}({2})".FormatString(Left, op, Right);
        }
    }
}
