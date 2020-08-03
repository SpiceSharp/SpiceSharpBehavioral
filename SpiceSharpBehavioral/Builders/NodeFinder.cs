using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// A builder that evaluates which nodes are in the expression.
    /// </summary>
    /// <seealso cref="IBuilder{T}" />
    public class NodeFinder : IBuilder<IEnumerable<VariableNode>>
    {
        /// <summary>
        /// Builds the specified value from the specified expression node.
        /// </summary>
        /// <param name="expression">The expression node.</param>
        /// <returns>
        /// The variable nodes.
        /// </returns>
        public IEnumerable<VariableNode> Build(Node expression)
        {
            switch (expression)
            {
                case UnaryOperatorNode un:
                    foreach (var n in Build(un.Argument))
                        yield return n;
                    break;

                case BinaryOperatorNode bn:
                    foreach (var n in Build(bn.Left))
                        yield return n;
                    foreach (var n in Build(bn.Right))
                        yield return n;
                    break;

                case TernaryOperatorNode tn:
                    foreach (var n in Build(tn.Condition))
                        yield return n;
                    foreach (var n in Build(tn.IfTrue))
                        yield return n;
                    foreach (var n in Build(tn.IfFalse))
                        yield return n;
                    break;

                case FunctionNode fn:
                    foreach (var arg in fn.Arguments)
                    {
                        foreach (var n in Build(arg))
                            yield return n;
                    }
                    break;

                case VariableNode vn:
                    yield return vn;
                    break;
            }
        }

        /// <summary>
        /// Finds all voltage nodes.
        /// </summary>
        /// <param name="expression">The expression node.</param>
        /// <returns>The voltage nodes.</returns>
        public IEnumerable<VariableNode> VoltageNodes(Node expression) => Build(expression).Where(n => n.NodeType == NodeTypes.Voltage);

        /// <summary>
        /// Finds all current nodes.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The current nodes.</returns>
        public IEnumerable<VariableNode> CurrentNodes(Node expression) => Build(expression).Where(n => n.NodeType == NodeTypes.Current);
    }
}
