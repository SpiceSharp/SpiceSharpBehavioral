using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// An abstract class representing an expression tree node.
    /// </summary>
    /// <remarks>
    /// This framework is loosely based on Linq expressions.
    /// </remarks>
    public abstract class Node
    {
        /// <summary>
        /// A constant that represents zero. Can be used for simplifications.
        /// </summary>
        public static readonly Node Zero = Constant(0);

        /// <summary>
        /// A constant that represents one. Can be used for simplifications.
        /// </summary>
        public static readonly Node One = Constant(1);

        /// <summary>
        /// A constant that represents two. Can be used for simplifications.
        /// </summary>
        public static readonly Node Two = Constant(2);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Add/*'/>
        public static BinaryOperatorNode Add(Node left, Node right) => BinaryOperatorNode.Add(left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Subtract/*'/>
        public static BinaryOperatorNode Subtract(Node left, Node right) => BinaryOperatorNode.Subtract(left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Multiply/*'/>
        public static BinaryOperatorNode Multiply(Node left, Node right) => BinaryOperatorNode.Multiply(left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Divide/*'/>
        public static BinaryOperatorNode Divide(Node left, Node right) => BinaryOperatorNode.Divide(left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Modulo/*'/>
        public static BinaryOperatorNode Modulo(Node left, Node right) => BinaryOperatorNode.Modulo(left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/And/*'/>
        public static BinaryOperatorNode And(Node left, Node right) => BinaryOperatorNode.And(left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Or/*'/>
        public static BinaryOperatorNode Or(Node left, Node right) => BinaryOperatorNode.Or(left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Xor/*'/>
        public static BinaryOperatorNode Xor(Node left, Node right) => BinaryOperatorNode.Xor(left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Equals/*'/>
        public static BinaryOperatorNode Equals(Node left, Node right) => BinaryOperatorNode.Equals(left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/NotEquals/*'/>
        public static BinaryOperatorNode NotEquals(Node left, Node right) => BinaryOperatorNode.NotEquals(left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/LessThan/*'/>
        public static BinaryOperatorNode LessThan(Node left, Node right) => BinaryOperatorNode.LessThan(left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/GreaterThan/*'/>
        public static BinaryOperatorNode GreaterThan(Node left, Node right) => BinaryOperatorNode.GreaterThan(left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/LessThanOrEquals/*'/>
        public static BinaryOperatorNode LessThanOrEqual(Node left, Node right) => BinaryOperatorNode.LessThanOrEqual(left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/GreaterThanOrEquals/*'/>
        public static BinaryOperatorNode GreaterThanOrEqual(Node left, Node right) => BinaryOperatorNode.GreaterThanOrEqual(left, right);

        /// <include file='docs.xml' path='docs/members/Binary/*'/>
        /// <include file='docs.xml' path='docs/members/Power/*'/>
        public static BinaryOperatorNode Power(Node left, Node right) => BinaryOperatorNode.Power(left, right);

        /// <include file='docs.xml' path='docs/members/Unary/*'/>
        /// <include file='docs.xml' path='docs/members/Plus/*'/>
        public static UnaryOperatorNode Plus(Node argument) => UnaryOperatorNode.Plus(argument);

        /// <include file='docs.xml' path='docs/members/Unary/*'/>
        /// <include file='docs.xml' path='docs/members/Minus/*'/>
        public static UnaryOperatorNode Minus(Node argument) => UnaryOperatorNode.Minus(argument);

        /// <include file='docs.xml' path='docs/members/Unary/*'/>
        /// <include file='docs.xml' path='docs/members/Bang/*'/>
        public static UnaryOperatorNode Not(Node argument) => UnaryOperatorNode.Not(argument);

        /// <include file='docs.xml' path='docs/members/Conditional/*'/>
        public static TernaryOperatorNode Conditional(Node condition, Node ifTrue, Node ifFalse) => TernaryOperatorNode.Conditional(condition, ifTrue, ifFalse);

        /// <include file='docs.xml' path='docs/members/Voltage/*'/>
        public static VariableNode Voltage(string name) => VariableNode.Voltage(name);

        /// <include file='docs.xml' path='docs/members/Voltage/*'/>
        /// <param name="reference">The name of the reference node.</param>
        public static BinaryOperatorNode Voltage(string name, string reference) => BinaryOperatorNode.Subtract(VariableNode.Voltage(name), VariableNode.Voltage(reference));

        /// <include file='docs.xml' path='docs/members/Voltage/*'/>
        public static VariableNode Current(string name) => VariableNode.Current(name);

        /// <include file='docs.xml' path='docs/members/Property/*'/>
        public static PropertyNode Property(string name, string property) => PropertyNode.Property(name, property);

        /// <include file='docs.xml' path='docs/members/Function/*'/>
        public static FunctionNode Function(string name, IReadOnlyList<Node> arguments) => FunctionNode.Function(name, arguments);

        /// <include file='docs.xml' path='docs/members/Function/*'/>
        public static FunctionNode Function(string name, params Node[] arguments) => FunctionNode.Function(name, arguments);

        /// <include file='docs.xml' path='docs/members/Variable/*'/>
        public static VariableNode Variable(string name) => VariableNode.Variable(name);

        /// <include file='docs.xml' path='docs/members/Constant/*'/>
        public static ConstantNode Constant(double literal) => ConstantNode.Constant(literal);

        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        /// <value>
        /// The type of the node.
        /// </value>
        public NodeTypes NodeType { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public virtual NodeProperties Properties { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="properties">The properties of the node.</param>
        protected Node(NodeTypes type, NodeProperties properties = NodeProperties.None)
        {
            NodeType = type;
            Properties = properties;
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Node operator +(Node left, Node right)
        {
            if (left == null || left.Equals(Zero))
                return right;
            if (right == null || right.Equals(Zero))
                return left;
            return Add(left, right);
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Node operator -(Node left, Node right)
        {
            if (left == null || left.Equals(Zero))
                return Minus(right);
            if (right == null || right.Equals(Zero))
                return left;
            return Subtract(left, right);
        }

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Node operator *(Node left, Node right)
        {
            if (left == null || right == null || left.Equals(Zero) || right.Equals(Zero))
                return null;
            if (left.Equals(One))
                return right;
            if (right.Equals(One))
                return left;
            return Multiply(left, right);
        }

        /// <summary>
        /// Implements the operator /.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Node operator /(Node left, Node right)
        {
            if (left == null || left.Equals(Zero))
                return Zero;
            if (right == null || right.Equals(Zero))
                throw new DivideByZeroException();
            if (right.Equals(One))
                return left;
            return Divide(left, right);
        }

        /// <summary>
        /// Implements the operator %.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Node operator %(Node left, Node right)
        {
            if (left == null || left.Equals(Zero))
                return null;
            if (right == null || right.Equals(Zero))
                throw new DivideByZeroException();
            if (right.Equals(One))
                return Zero;
            return Modulo(left, right);
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Node operator +(Node arg) => arg == null ? null : Plus(arg);

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Node operator -(Node arg) => arg == null ? null : Minus(arg);

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Double"/> to <see cref="Node"/>.
        /// </summary>
        /// <param name="literal">The literal.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Node(double literal) => Constant(literal);
    }
}
