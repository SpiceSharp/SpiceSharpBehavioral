using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;

namespace SpiceSharp.Components.BehavioralSources
{
    /// <summary>
    /// A specialized comparer for <see cref="VariableNode"/> that can account for
    /// node and entity names.
    /// </summary>
    public class VariableNodeComparer : IEqualityComparer<VariableNode>
    {
        private readonly IEqualityComparer<string> _nodeComparer;
        private readonly IEqualityComparer<string> _entityComparer;
        private readonly IEqualityComparer<string> _variableComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableNodeComparer"/> class.
        /// </summary>
        /// <param name="nodeComparer">The name comparer for voltage nodes.</param>
        /// <param name="entityComparer">The name comparer for entities.</param>
        /// <param name="variableComparer">The name comparer for variables.</param>
        public VariableNodeComparer(IEqualityComparer<string> nodeComparer, IEqualityComparer<string> entityComparer, IEqualityComparer<string> variableComparer)
        {
            _nodeComparer = nodeComparer ?? EqualityComparer<string>.Default;
            _entityComparer = entityComparer ?? EqualityComparer<string>.Default;
            _variableComparer = variableComparer ?? EqualityComparer<string>.Default;
        }

        /// <summary>
        /// Check equality.
        /// </summary>
        /// <param name="x">The first variable node.</param>
        /// <param name="y">The second variable node.</param>
        /// <returns>
        ///     <c>true</c> if they are equal; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(VariableNode x, VariableNode y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x == null || y == null)
                return false;
            if (x.NodeType != y.NodeType)
                return false;

            switch (x.NodeType)
            {
                case NodeTypes.Voltage:
                    return _nodeComparer.Equals(x.Name, y.Name);
                case NodeTypes.Current:
                    return _entityComparer.Equals(x.Name, y.Name);
                case NodeTypes.Variable:
                    return _variableComparer.Equals(x.Name, y.Name);
                default:
                    return true;
            }
        }

        /// <summary>
        /// Get the hash code.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The hash code.</returns>
        public int GetHashCode(VariableNode obj)
        {
            if (obj == null)
                return 0;
            switch (obj.NodeType)
            {
                case NodeTypes.Voltage:
                    if (_nodeComparer.Equals(obj.Name, obj.Name.ToLower()))
                        return obj.NodeType.GetHashCode() ^ obj.Name.ToLower().GetHashCode();
                    else
                        return obj.NodeType.GetHashCode() ^ obj.Name.GetHashCode();

                case NodeTypes.Current:
                    if (_entityComparer.Equals(obj.Name, obj.Name.ToLower()))
                        return obj.NodeType.GetHashCode() ^ obj.Name.ToLower().GetHashCode();
                    else
                        return obj.NodeType.GetHashCode() ^ obj.Name.GetHashCode();

                case NodeTypes.Variable:
                    if (_entityComparer.Equals(obj.Name, obj.Name.ToLower()))
                        return obj.NodeType.GetHashCode() ^ obj.Name.ToLower().GetHashCode();
                    else
                        return obj.NodeType.GetHashCode() ^ obj.Name.GetHashCode();

                default:
                    return obj.NodeType.GetHashCode() ^ obj.Name.GetHashCode();
            }
        }
    }
}
