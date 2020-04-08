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
        public static Node Add(Node left, Node right) => BinaryOperatorNode.Add(left, right);
        public static Node Subtract(Node left, Node right) => BinaryOperatorNode.Subtract(left, right);
        public static Node Multiply(Node left, Node right) => BinaryOperatorNode.Multiply(left, right);
        public static Node Divide(Node left, Node right) => BinaryOperatorNode.Divide(left, right);
        public static Node Modulo(Node left, Node right) => BinaryOperatorNode.Modulo(left, right);
        public static Node And(Node left, Node right) => BinaryOperatorNode.And(left, right);
        public static Node Or(Node left, Node right) => BinaryOperatorNode.Or(left, right);
        public static Node Equals(Node left, Node right) => BinaryOperatorNode.Equals(left, right);
        public static Node NotEquals(Node left, Node right) => BinaryOperatorNode.NotEquals(left, right);
        public static Node LessThan(Node left, Node right) => BinaryOperatorNode.LessThan(left, right);
        public static Node GreaterThan(Node left, Node right) => BinaryOperatorNode.GreaterThan(left, right);
        public static Node LessThanOrEqual(Node left, Node right) => BinaryOperatorNode.LessThanOrEqual(left, right);
        public static Node GreaterThanOrEqual(Node left, Node right) => BinaryOperatorNode.GreaterThanOrEqual(left, right);
        public static Node Power(Node left, Node right) => BinaryOperatorNode.Power(left, right);
        public static Node Plus(Node argument) => UnaryOperatorNode.Plus(argument);
        public static Node Minus(Node argument) => UnaryOperatorNode.Minus(argument);
        public static Node Conditional(Node condition, Node ifTrue, Node ifFalse) => TernaryOperatorNode.Conditional(condition, ifTrue, ifFalse);
        public static Node Voltage(string name, QuantityTypes qtype = QuantityTypes.Raw) => VoltageNode.Voltage(name, qtype);
        public static Node Voltage(string name, string reference, QuantityTypes qtype = QuantityTypes.Raw) => VoltageNode.Voltage(name, reference, qtype);
        public static Node Current(string name, QuantityTypes qtype = QuantityTypes.Raw) => CurrentNode.Current(name, qtype);
        public static Node Property(string name, string property, QuantityTypes qtype = QuantityTypes.Raw) => PropertyNode.Property(name, property, qtype);
        public static Node Function(string name, IReadOnlyList<Node> arguments) => FunctionNode.Function(name, arguments);
        public static Node Variable(string name) => VariableNode.Variable(name);
        public static Node Constant(string name) => ConstantNode.Constant(name);

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
        public virtual NodeProperties Properties { get; } = NodeProperties.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        protected Node(NodeTypes type)
        {
            NodeType = type;
        }
    }
}
