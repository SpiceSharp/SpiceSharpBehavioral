using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.Components.BehavioralComponents
{
    /// <summary>
    /// A base implementation for behavioral components.
    /// </summary>
    /// <seealso cref="Entity" />
    /// <seealso cref="IComponent" />
    public abstract class BehavioralComponent : Entity, IComponent,
        IParameterized<BaseParameters>
    {
        private readonly string[] _connections;

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
        /// Gets the number of nodes.
        /// </summary>
        /// <value>
        /// The number of nodes.
        /// </value>
        public int PinCount => _connections.Length + (_nodes?.Count ?? 0);

        private List<string> _nodes
        {
            get
            {
                var parser = new Parser<List<string>>(new NodeFinder());
                return parser.Parse(Parameters.LexerFactory(Parameters.Expression));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralComponent"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="basePinCount">The base pin count.</param>
        public BehavioralComponent(string name, int basePinCount)
            : base(name)
        {
            _connections = new string[basePinCount];
        }

        /// <summary>
        /// Gets the behavioral description.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <returns>
        /// The functions for evaluating the behavioral component.
        /// </returns>
        protected Derivatives<Func<double>> GetDescription(ISimulation simulation)
        {
            var parser = Parameters.ParserFactory();
            var lexer = Parameters.LexerFactory(Parameters.Expression);
            parser.Simulation = simulation.ThrowIfNull(nameof(simulation));
            return parser.Parse(lexer);
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

        /// <summary>
        /// Gets the node name by pin index.
        /// </summary>
        /// <param name="index">The pin index.</param>
        /// <returns>
        /// The node index.
        /// </returns>
        public string GetNode(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index < 2)
                return _connections[index];
            index -= 2;

            // We'll have to find the node inside our expression
            return _nodes[index];
        }

        /// <summary>
        /// Maps the nodes
        /// </summary>
        /// <param name="variables"></param>
        /// <returns></returns>
        public IReadOnlyList<Variable> MapNodes(IVariableSet variables)
        {
            variables.ThrowIfNull(nameof(variables));
            var nodes = new List<string>(_connections);

            // Parse the expression to find other nodes
            if (_nodes != null)
                nodes.AddRange(_nodes);
            var result = new Variable[nodes.Count];
            for (var i = 0; i < nodes.Count; i++)
                result[i] = variables.MapNode(nodes[i], VariableType.Voltage);
            return result;
        }
    }
}
