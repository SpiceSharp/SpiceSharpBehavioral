using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Components.BehavioralComponents
{
    /// <summary>
    /// Base parameters for a behavioral component.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public partial class Parameters : SpiceSharpBehavioral.Components.Parameters<Parameters>
    {
        private readonly NodeFinder _nodeFinder = new NodeFinder();

        /// <summary>
        /// Gets the voltage nodes.
        /// </summary>
        /// <value>
        /// The voltage nodes.
        /// </value>
        public IEnumerable<string> VoltageNodes => _nodeFinder.VoltageNodes(Function).Select(n => n.Name);

        /// <summary>
        /// Gets the current nodes.
        /// </summary>
        /// <value>
        /// The current nodes.
        /// </value>
        public IEnumerable<string> CurrentNodes => _nodeFinder.CurrentNodes(Function).Select(n => n.Name);

        /// <summary>
        /// Gets all variable nodes.
        /// </summary>
        /// <value>
        /// The variable nodes.
        /// </value>
        public IEnumerable<VariableNode> VariableNodes => _nodeFinder.Build(Function);

        /// <summary>
        /// Gets or sets the variable comparer.
        /// </summary>
        /// <value>
        /// The variable comparer.
        /// </value>
        public IEqualityComparer<string> VariableComparer { get; set; } = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parameters"/> class.
        /// </summary>
        public Parameters()
        {
            RealBuilderCreated += BuilderHelper.RegisterDefaultBuilder;
            ComplexBuilderCreated += BuilderHelper.RegisterDefaultBuilder;
        }
    }
}
