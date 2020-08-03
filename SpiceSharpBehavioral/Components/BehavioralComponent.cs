using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.BehavioralComponents
{
    /// <summary>
    /// A base implementation for behavioral components.
    /// </summary>
    /// <seealso cref="Entity" />
    /// <seealso cref="IComponent" />
    public abstract class BehavioralComponent : Entity<BehavioralBindingContext>,
        IComponent,
        IParameterized<Parameters>
    {
        private readonly string[] _connections;
        

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public Parameters Parameters { get; } = new Parameters();

        /// <summary>
        /// Gets or sets the model of the component.
        /// </summary>
        /// <value>
        /// The model of the component.
        /// </value>
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

        /// <summary>
        /// Creates the behaviors and stores them in the specified container.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="simulation"/> is <c>null</c>.</exception>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            simulation.EntityBehaviors.Add(behaviors);
            DI.Resolve(simulation, this, behaviors);
        }
    }
}
