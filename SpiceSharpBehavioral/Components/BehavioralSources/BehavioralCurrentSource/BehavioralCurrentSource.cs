using System;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System.Collections.Generic;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralCurrentSourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A behavioral current source.
    /// </summary>
    /// <seealso cref="Entity" />
    /// <seealso cref="IComponent" />
    /// <seealso cref="IParameterized{T}" />
    public class BehavioralCurrentSource : Entity, IComponent,
        IParameterized<BaseParameters>
    {
        private readonly string[] _connections = new string[2];

        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        /// <value>
        /// The base parameters.
        /// </value>
        public BaseParameters Parameters { get; } = new BaseParameters();

        private List<string> _nodes
        {
            get
            {
                var parser = new Parser<List<string>>(new NodeFinder());
                return parser.Parse(Parameters.LexerFactory(Parameters.Expression));
            }
        }

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
        public int PinCount => 2 + _nodes.Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="inp">The inp.</param>
        /// <param name="inn">The inn.</param>
        /// <param name="expression">The expression.</param>
        public BehavioralCurrentSource(string name, string inp, string inn, string expression)
            : base(name)
        {
            _connections[0] = inp.ThrowIfNull(nameof(inp));
            _connections[1] = inn.ThrowIfNull(nameof(inn));
            Parameters.Expression = expression;
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

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            // Parse the expression to be used
            var parser = Parameters.ParserFactory();
            var lexer = Parameters.LexerFactory(Parameters.Expression);
            parser.Simulation = simulation;
            var derivatives = parser.Parse(lexer);

            // Create the context, and use it to create our behaviors
            var context = new BehavioralComponentContext(this, simulation, LinkParameters, derivatives);
            var behaviors = new BehaviorContainer(Name);
            behaviors
                .AddIfNo<IBiasingBehavior>(simulation, () => new BiasingBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
