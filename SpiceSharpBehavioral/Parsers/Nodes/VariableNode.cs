using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// A variable node.
    /// </summary>
    /// <seealso cref="Node" />
    public class VariableNode : Node
    {
        public static VariableNode Voltage(string name) => new VariableNode(NodeTypes.Voltage, name);
        public static VariableNode Current(string name) => new VariableNode(NodeTypes.Current, name);
        public static VariableNode Variable(string name) => new VariableNode(NodeTypes.Variable, name);

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>
        /// The name of the variable.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableNode"/> class.
        /// </summary>
        /// <param name="type">The node type.</param>
        /// <param name="name">The name.</param>
        protected VariableNode(NodeTypes type, string name)
            : base(type, NodeProperties.Terminal)
        {
            Name = name.ThrowIfNull(nameof(name));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => NodeType.GetHashCode() ^ Name.GetHashCode();

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
            if (obj is VariableNode vn)
            {
                if (!Name.Equals(vn.Name))
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
            switch (NodeType)
            {
                case NodeTypes.Voltage: return "V({0})".FormatString(Name);
                case NodeTypes.Current: return "I({0})".FormatString(Name);
                default: return Name;
            }
        }
    }
}
