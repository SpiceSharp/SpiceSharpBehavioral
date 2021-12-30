using SpiceSharpBehavioral.Builders;
using System;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// A constant node.
    /// </summary>
    /// <seealso cref="Node" />
    public class ConstantNode : Node, IFormattable
    {
        /// <include file='docs.xml' path='docs/members/Constant/*'/>
        public static new ConstantNode Constant(double literal) => new ConstantNode(literal);

        /// <summary>
        /// Gets or sets the relative tolerance.
        /// </summary>
        /// <value>
        /// The relative tolerance.
        /// </value>
        public static double RelativeTolerance { get; set; } = 1e-6;

        /// <summary>
        /// Gets or sets the absolute tolerance.
        /// </summary>
        /// <value>
        /// The absolute tolerance.
        /// </value>
        public static double AbsoluteTolerance { get; set; } = 1e-12;

        /// <summary>
        /// Gets the literal.
        /// </summary>
        /// <value>
        /// The literal.
        /// </value>
        public double Literal { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantNode"/> class.
        /// </summary>
        /// <param name="literal">The literal.</param>
        protected ConstantNode(double literal)
            : base(NodeTypes.Constant, NodeProperties.Constant | NodeProperties.Terminal)
        {
            Literal = literal;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => NodeType.GetHashCode() ^ Literal.GetHashCode();

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
            if (obj is ConstantNode cn)
            {
                if (!HelperFunctions.Equals(Literal, cn.Literal, RelativeTolerance, AbsoluteTolerance))
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
        public override string ToString() => Literal.ToString();

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="provider">The provider.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider provider) => Literal.ToString(format, provider);
    }
}
