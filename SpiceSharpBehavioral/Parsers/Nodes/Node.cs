﻿using System.Collections.Generic;

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
        public static BinaryOperatorNode Add(Node left, Node right) => BinaryOperatorNode.Add(left, right);
        public static BinaryOperatorNode Subtract(Node left, Node right) => BinaryOperatorNode.Subtract(left, right);
        public static BinaryOperatorNode Multiply(Node left, Node right) => BinaryOperatorNode.Multiply(left, right);
        public static BinaryOperatorNode Divide(Node left, Node right) => BinaryOperatorNode.Divide(left, right);
        public static BinaryOperatorNode Modulo(Node left, Node right) => BinaryOperatorNode.Modulo(left, right);
        public static BinaryOperatorNode And(Node left, Node right) => BinaryOperatorNode.And(left, right);
        public static BinaryOperatorNode Or(Node left, Node right) => BinaryOperatorNode.Or(left, right);
        public static BinaryOperatorNode Equals(Node left, Node right) => BinaryOperatorNode.Equals(left, right);
        public static BinaryOperatorNode NotEquals(Node left, Node right) => BinaryOperatorNode.NotEquals(left, right);
        public static BinaryOperatorNode LessThan(Node left, Node right) => BinaryOperatorNode.LessThan(left, right);
        public static BinaryOperatorNode GreaterThan(Node left, Node right) => BinaryOperatorNode.GreaterThan(left, right);
        public static BinaryOperatorNode LessThanOrEqual(Node left, Node right) => BinaryOperatorNode.LessThanOrEqual(left, right);
        public static BinaryOperatorNode GreaterThanOrEqual(Node left, Node right) => BinaryOperatorNode.GreaterThanOrEqual(left, right);
        public static BinaryOperatorNode Power(Node left, Node right) => BinaryOperatorNode.Power(left, right);
        public static UnaryOperatorNode Plus(Node argument) => UnaryOperatorNode.Plus(argument);
        public static UnaryOperatorNode Minus(Node argument) => UnaryOperatorNode.Minus(argument);
        public static UnaryOperatorNode Not(Node argument) => UnaryOperatorNode.Not(argument);
        public static TernaryOperatorNode Conditional(Node condition, Node ifTrue, Node ifFalse) => TernaryOperatorNode.Conditional(condition, ifTrue, ifFalse);
        public static VariableNode Voltage(string name) => VariableNode.Voltage(name);
        public static BinaryOperatorNode Voltage(string name, string reference) => BinaryOperatorNode.Subtract(VariableNode.Voltage(name), VariableNode.Voltage(reference));
        public static VariableNode Current(string name) => VariableNode.Current(name);
        public static PropertyNode Property(string name, string property) => PropertyNode.Property(name, property);
        public static FunctionNode Function(string name, IReadOnlyList<Node> arguments) => FunctionNode.Function(name, arguments);
        public static FunctionNode Function(string name, params Node[] arguments) => FunctionNode.Function(name, arguments);
        public static VariableNode Variable(string name) => VariableNode.Variable(name);
        public static ConstantNode Constant(string name) => ConstantNode.Constant(name);

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
    }
}
