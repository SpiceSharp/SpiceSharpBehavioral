using SpiceSharp;
using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// A builder for replacing nodes.
    /// </summary>
    public class NodeReplacer : IBuilder<Node>
    {
        /// <summary>
        /// Gets or sets the variable map that is used for replacing variables.
        /// </summary>
        /// <value>
        /// The variable map.
        /// </value>
        public Dictionary<VariableNode, Node> Map { get; set; }

        /// <inheritdoc/>
        public Node Build(Node expression)
        {
            switch (expression)
            {
                case UnaryOperatorNode un:
                    switch (un.NodeType)
                    {
                        case NodeTypes.Plus: return Node.Plus(Build(un.Argument));
                        case NodeTypes.Minus: return Node.Minus(Build(un.Argument));
                        case NodeTypes.Not: return Node.Not(Build(un.Argument));
                    }
                    break;

                case BinaryOperatorNode bn:
                    var left = Build(bn.Left);
                    var right = Build(bn.Right);
                    switch (bn.NodeType)
                    {
                        case NodeTypes.Add: return Node.Add(left, right);
                        case NodeTypes.Subtract: return Node.Subtract(left, right);
                        case NodeTypes.Multiply: return Node.Multiply(left, right);
                        case NodeTypes.Divide: return Node.Divide(left, right);
                        case NodeTypes.Modulo: return Node.Modulo(left, right);
                        case NodeTypes.LessThan: return Node.LessThan(left, right);
                        case NodeTypes.GreaterThan: return Node.GreaterThan(left, right);
                        case NodeTypes.LessThanOrEqual: return Node.LessThanOrEqual(left, right);
                        case NodeTypes.GreaterThanOrEqual: return Node.GreaterThanOrEqual(left, right);
                        case NodeTypes.Equals: return Node.Equals(left, right);
                        case NodeTypes.NotEquals: return Node.NotEquals(left, right);
                        case NodeTypes.And: return Node.And(left, right);
                        case NodeTypes.Or: return Node.Or(left, right);
                        case NodeTypes.Xor: return Node.Xor(left, right);
                        case NodeTypes.Pow: return Node.Power(left, right);
                    }
                    break;

                case TernaryOperatorNode tn:
                    return Node.Conditional(Build(tn.Condition), Build(tn.IfTrue), Build(tn.IfFalse));

                case FunctionNode fn:
                    var args = new Node[fn.Arguments.Count];
                    for (var i = 0; i < args.Length; i++)
                        args[i] = Build(fn.Arguments[i]);
                    return Node.Function(fn.Name, args);

                case ConstantNode cn:
                    return cn;

                case VariableNode vn:
                    if (Map != null && Map.TryGetValue(vn, out var mapped))
                        return mapped;
                    else
                        return vn;
            }

            // Could not 
            return BuildNode(expression);
        }

        /// <summary>
        /// Builds the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The built value.</returns>
        /// <exception cref="SpiceSharpException">Unrecognized node</exception>
        protected virtual double BuildNode(Node node)
        {
            throw new SpiceSharpException("Unrecognized expression node {0}".FormatString(node));
        }
    }
}
