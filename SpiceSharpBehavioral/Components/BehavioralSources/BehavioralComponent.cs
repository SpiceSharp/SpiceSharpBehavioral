using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.BehavioralSources
{
    /// <summary>
    /// A base implementation for behavioral components.
    /// </summary>
    /// <seealso cref="Entity" />
    /// <seealso cref="IComponent" />
    public abstract class BehavioralComponent : Entity<Parameters>,
        IComponent,
        IParameterized<Parameters>
    {
        private readonly string[] _connections;

        /// <inheritdoc/>
        public string Model { get; set; }

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
                nodes.AddRange(Parameters.VoltageNodes);
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
        /// <exception cref="NodeMismatchException">Thrown if the the number of nodes does not match that of the component.</exception>
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
