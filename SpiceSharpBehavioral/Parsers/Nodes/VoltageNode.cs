using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// A voltage node.
    /// </summary>
    /// <seealso cref="Node" />
    public class VoltageNode : Node
    {
        public static VoltageNode Voltage(string node, QuantityTypes qtype = QuantityTypes.Raw) => new VoltageNode(node, null, qtype);
        public static VoltageNode Voltage(string node, string reference, QuantityTypes qtype = QuantityTypes.Raw) => new VoltageNode(node, reference, qtype);

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        /// <value>
        /// The node.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the name of the reference.
        /// </summary>
        /// <value>
        /// The reference.
        /// </value>
        public string Reference { get; }

        /// <summary>
        /// Gets the type of quantity.
        /// </summary>
        /// <value>
        /// The type of quantity.
        /// </value>
        public QuantityTypes QuantityType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageNode"/> class.
        /// </summary>
        /// <param name="name">The node.</param>
        /// <param name="reference">The reference.</param>
        protected VoltageNode(string name, string reference, QuantityTypes qtype)
            : base(NodeTypes.Voltage)
        {
            Name = name.ThrowIfNull(nameof(name));
            Reference = reference;
            QuantityType = qtype;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hs = NodeType.GetHashCode() ^ QuantityType.GetHashCode() ^ Name.GetHashCode();
            if (Reference != null)
                hs ^= (Reference.GetHashCode() * 13);
            return hs;
        }

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
            if (obj is VoltageNode vn)
            {
                if (QuantityType != vn.QuantityType)
                    return false;
                if (!Name.Equals(vn.Name))
                    return false;
                if (Reference != null)
                {
                    if (vn.Reference == null || !Reference.Equals(vn.Reference))
                        return false;
                }
                else if (vn.Reference != null)
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
            if (Reference != null)
                return "V({0},{1})".FormatString(Name, Reference);
            return "V({0})".FormatString(Name);
        }
    }
}
