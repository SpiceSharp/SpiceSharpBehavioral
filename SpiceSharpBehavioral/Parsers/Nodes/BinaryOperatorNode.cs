using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// A binary operator.
    /// </summary>
    /// <seealso cref="Node" />
    public class BinaryOperatorNode : Node
    {
        /// <summary>
        /// Creates an addition of two arguments.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The node representing the added result.</returns>
        public static new BinaryOperatorNode Add(Node left, Node right) => new BinaryOperatorNode(NodeTypes.Add, left, right);

        /// <summary>
        /// Creates an subtraction of two arguments.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The node representing the subtracted result.</returns>
        public static new BinaryOperatorNode Subtract(Node left, Node right) => new BinaryOperatorNode(NodeTypes.Subtract, left, right);

        /// <summary>
        /// Creates an multiplication of two arguments.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The node representing the multiplied result.</returns>
        public static new BinaryOperatorNode Multiply(Node left, Node right) => new BinaryOperatorNode(NodeTypes.Multiply, left, right);

        /// <summary>
        /// Creates an division of two arguments.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The node representing the divided result.</returns>
        public static new BinaryOperatorNode Divide(Node left, Node right) => new BinaryOperatorNode(NodeTypes.Divide, left, right);

        /// <summary>
        /// Creates an remainder of the division of two arguments.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The node representing the modulo result.</returns>
        public static new BinaryOperatorNode Modulo(Node left, Node right) => new BinaryOperatorNode(NodeTypes.Modulo, left, right);

        /// <summary>
        /// Creates an logical and of two arguments.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The node representing the logical and result.</returns>
        public static new BinaryOperatorNode And(Node left, Node right) => new BinaryOperatorNode(NodeTypes.And, left, right);

        /// <summary>
        /// Creates an logical or of two arguments.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The node representing the logical or result.</returns>
        public static new BinaryOperatorNode Or(Node left, Node right) => new BinaryOperatorNode(NodeTypes.Or, left, right);

        /// <summary>
        /// Creates a equality comparison of two arguments.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The node representing the compared (equality) result.</returns>
        public static new BinaryOperatorNode Equals(Node left, Node right) => new BinaryOperatorNode(NodeTypes.Equals, left, right);

        /// <summary>
        /// Creates an inequality comparison of two arguments.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The node representing the compared (inequality) result.</returns>
        public static new BinaryOperatorNode NotEquals(Node left, Node right) => new BinaryOperatorNode(NodeTypes.NotEquals, left, right);

        /// <summary>
        /// Creates a less than comparison of two arguments.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The node representing the compared (less than) result.</returns>
        public static new BinaryOperatorNode LessThan(Node left, Node right) => new BinaryOperatorNode(NodeTypes.LessThan, left, right);

        /// <summary>
        /// Creates a greater than comparison of two arguments.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The node representing the compared (greater than) result.</returns>
        public static new BinaryOperatorNode GreaterThan(Node left, Node right) => new BinaryOperatorNode(NodeTypes.GreaterThan, left, right);

        /// <summary>
        /// Creates a less than or equal comparison of two arguments.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The node representing the compared (less than or equal) result.</returns>
        public static new BinaryOperatorNode LessThanOrEqual(Node left, Node right) => new BinaryOperatorNode(NodeTypes.LessThanOrEqual, left, right);

        /// <summary>
        /// Creates a less than comparison of two arguments.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The node representing the compared (greater than or equal) result.</returns>
        public static new BinaryOperatorNode GreaterThanOrEqual(Node left, Node right) => new BinaryOperatorNode(NodeTypes.GreaterThanOrEqual, left, right);

        /// <summary>
        /// Creates a power of two arguments.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The node representing the power result.</returns>
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

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public override NodeProperties Properties { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperatorNode"/> class.
        /// </summary>
        /// <param name="type">The node type.</param>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
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
                NodeTypes.And => "&&",
                NodeTypes.Or => "||",
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
