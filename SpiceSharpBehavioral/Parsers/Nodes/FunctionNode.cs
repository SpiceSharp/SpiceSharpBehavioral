using SpiceSharp;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// A function node.
    /// </summary>
    /// <seealso cref="Node" />
    public class FunctionNode : Node
    {
        /// <include file='docs.xml' path='docs/members/Function/*'/>
        public static new FunctionNode Function(string name, IReadOnlyList<Node> arguments) => new FunctionNode(name, arguments);

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        public IReadOnlyList<Node> Arguments { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public override NodeProperties Properties { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionNode"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="arguments">The arguments.</param>
        protected FunctionNode(string name, IReadOnlyList<Node> arguments)
            : base(NodeTypes.Function)
        {
            Name = name.ThrowIfNull(nameof(name));
            Arguments = arguments.ThrowIfNull(nameof(arguments));

            // Check for constant arguments
            var isConstant = true;
            foreach (var arg in arguments)
            {
                arg.ThrowIfNull(nameof(arg));
                if ((arg.Properties & NodeProperties.Constant) == 0)
                {
                    isConstant = false;
                    break;
                }
            }
            Properties = isConstant ? NodeProperties.Constant : NodeProperties.None;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hc = NodeType.GetHashCode() ^ Name.GetHashCode();
            foreach (var arg in Arguments)
                hc = (hc * 13) ^ arg.GetHashCode();
            return hc;
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
            if (obj is FunctionNode fn)
            {
                if (!Name.Equals(fn.Name))
                    return false;
                if (Arguments.Count != fn.Arguments.Count)
                    return false;
                for (var i = 0; i < Arguments.Count; i++)
                {
                    if (!Arguments[i].Equals(fn.Arguments[i]))
                        return false;
                }
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
            return "{0}({1})".FormatString(Name, string.Join(",", Arguments));
        }
    }
}
