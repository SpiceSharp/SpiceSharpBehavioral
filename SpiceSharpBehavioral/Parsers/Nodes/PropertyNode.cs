using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// A property node.
    /// </summary>
    /// <seealso cref="Node" />
    public class PropertyNode : Node
    {
        /// <include file='docs.xml' path='docs/members/Property/*'/>
        public static new PropertyNode Property(string name, string property) => new PropertyNode(name, property);

        /// <summary>
        /// Gets the name of the entity.
        /// </summary>
        /// <value>
        /// The name of the entity.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string PropertyName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNode"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="property">The property.</param>
        protected PropertyNode(string name, string property)
            : base(NodeTypes.Property, NodeProperties.Terminal)
        {
            Name = name.ThrowIfNull(nameof(name));
            PropertyName = property.ThrowIfNull(nameof(property));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => NodeType.GetHashCode() ^ Name.GetHashCode() ^ (PropertyName.GetHashCode() * 13);

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
            if (obj is PropertyNode pn)
            {
                if (!Name.Equals(pn.Name))
                    return false;
                if (!PropertyName.Equals(pn.PropertyName))
                    return false;
                return true;
            }
            return false;
        }
    }
}
