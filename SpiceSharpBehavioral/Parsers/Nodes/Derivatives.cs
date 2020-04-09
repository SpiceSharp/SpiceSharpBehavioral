using SpiceSharp;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// A structure describing derivatives.
    /// </summary>
    public struct Derivatives
    {
        /// <summary>
        /// An empty derivatives.
        /// </summary>
        public static readonly Derivatives Empty = new Derivatives(null, null);

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        /// <value>
        /// The voltage.
        /// </value>
        public Dictionary<string, Node> Voltage { get; }

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        public Dictionary<string, Node> Current { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Derivatives"/> struct.
        /// </summary>
        /// <param name="voltage">The voltage.</param>
        /// <param name="current">The current.</param>
        public Derivatives(Dictionary<string, Node> voltage, Dictionary<string, Node> current)
        {
            Voltage = voltage;
            Current = current;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var lst = new List<string>();
            if (Voltage != null)
            {
                foreach (var pair in Voltage)
                    lst.Add("df/dV({0}) = {1}".FormatString(pair.Key, pair.Value));
            }
            if (Current != null)
            {
                foreach (var pair in Current)
                    lst.Add("df/dI({0}) = {1}".FormatString(pair.Key, pair.Value));
            }
            return string.Join("; ", lst);
        }
    }
}
