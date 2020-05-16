using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Components.BehavioralComponents
{
    /// <summary>
    /// A base implementation for behavioral components.
    /// </summary>
    /// <seealso cref="Entity" />
    /// <seealso cref="IComponent" />
    public abstract class BehavioralComponent : Entity,
        IComponent,
        IParameterized<BaseParameters>
    {
        private readonly string[] _connections;
        private readonly NodeFinder _nodeFinder = new NodeFinder();

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public BaseParameters Parameters { get; } = new BaseParameters();

        /// <summary>
        /// Gets or sets the model of the component.
        /// </summary>
        /// <value>
        /// The model of the component.
        /// </value>
        public string Model { get; set; }

        /// <summary>
        /// Gets the voltage nodes.
        /// </summary>
        /// <value>
        /// The voltage nodes.
        /// </value>
        protected IEnumerable<string> VoltageNodes => _nodeFinder.VoltageNodes(Parameters.Function).Select(n => n.Name);

        /// <summary>
        /// Gets the current nodes.
        /// </summary>
        /// <value>
        /// The current nodes.
        /// </value>
        protected IEnumerable<string> CurrentNodes => _nodeFinder.CurrentNodes(Parameters.Function).Select(n => n.Name);

        /// <summary>
        /// Gets all variable nodes.
        /// </summary>
        /// <value>
        /// The variable nodes.
        /// </value>
        protected IEnumerable<VariableNode> VariableNodes => _nodeFinder.Build(Parameters.Function);

        /// <summary>
        /// Gets the nodes.
        /// </summary>
        /// <value>
        /// The nodes.
        /// </value>
        public IReadOnlyList<string> Nodes
        {
            get
            {
                var nodes = new List<string>();
                nodes.AddRange(_connections);
                nodes.AddRange(VoltageNodes);
                return nodes.AsReadOnly();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralComponent"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="basePinCount">The base pin count.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        protected BehavioralComponent(string name, int basePinCount)
            : base(name)
        {
            _connections = new string[basePinCount];
        }

        /// <summary>
        /// Connects the component in the circuit.
        /// </summary>
        /// <param name="nodes">The node indices.</param>
        /// <returns>
        /// The instance calling the method for chaining.
        /// </returns>
        public IComponent Connect(params string[] nodes)
        {
            if (nodes == null || nodes.Length != _connections.Length)
                throw new NodeMismatchException(Name, _connections.Length, nodes?.Length ?? 0);
            for (var i = 0; i < nodes.Length; i++)
            {
                nodes[i].ThrowIfNull("node{0}".FormatString(i + 1));
                _connections[i] = nodes[i];
            }
            return this;
        }
    }
}
