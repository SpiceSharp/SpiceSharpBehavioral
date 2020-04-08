using SpiceSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// A current node.
    /// </summary>
    /// <seealso cref="Node" />
    public class CurrentNode : Node
    {
        public static CurrentNode Current(string name, QuantityTypes qtype = QuantityTypes.Raw) => new CurrentNode(name, qtype);

        /// <summary>
        /// Gets the name of the entity of which the current is requested.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the type of the quantity.
        /// </summary>
        /// <value>
        /// The type of the quantity.
        /// </value>
        public QuantityTypes QuantityType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentNode"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="qtype">The quantity type.</param>
        protected CurrentNode(string name, QuantityTypes qtype)
            : base(NodeTypes.Current)
        {
            Name = name.ThrowIfNull(nameof(name));
            QuantityType = qtype;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => NodeType.GetHashCode() ^ QuantityType.GetHashCode() ^ Name.GetHashCode();

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is CurrentNode cn)
            {
                if (QuantityType != cn.QuantityType)
                    return false;
                if (!Name.Equals(cn.Name))
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
            return "I({0})".FormatString(Name);
        }
    }
}
