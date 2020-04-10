using System;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// Node properties.
    /// </summary>
    [Flags]
    public enum NodeProperties
    {
        /// <summary>
        /// No special properties.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// The result is constant.
        /// </summary>
        Constant = 0x01,

        /// <summary>
        /// The node generates a value without taking parameters.
        /// </summary>
        Terminal = 0x02,
    }
}
