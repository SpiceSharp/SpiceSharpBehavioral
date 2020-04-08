using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// A builder that evaluates which nodes are in the expression.
    /// </summary>
    /// <seealso cref="IBuilder{T}" />
    public class NodeFinder : IBuilder<List<string>>
    {
        /// <summary>
        /// Builds the specified value from the specified expression node.
        /// </summary>
        /// <param name="expression">The expression node.</param>
        /// <returns>
        /// The value.
        /// </returns>
        public List<string> Build(Node expression)
        {
            var result = new List<string>(3);
            Build(result, expression);
            return result;
        }

        /// <summary>
        /// Builds the specified list with nodes.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="node">The node.</param>
        protected virtual void Build(List<string> list, Node node)
        {
            if (node is UnaryOperatorNode un)
                Build(list, un.Argument);
            else if (node is BinaryOperatorNode bn)
            {
                Build(list, bn.Left);
                Build(list, bn.Right);
            }
            else if (node is TernaryOperatorNode tn)
            {
                Build(list, tn.Condition);
                Build(list, tn.IfTrue);
                Build(list, tn.IfFalse);
            }
            else if (node is VoltageNode vn)
            {
                list.Add(vn.Name);
                if (vn.Reference != null)
                    list.Add(vn.Reference);
            }
        }
    }
}
