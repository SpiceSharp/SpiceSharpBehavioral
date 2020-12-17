using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using SpiceSharpBehavioral;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers;
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
    public class Parameters : ParameterSet
    {
        private bool _isDirty;
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
        /// Gets the default builder for building expressions.
        /// </summary>
        /// <param name="simulation">The simulation for which to create variables.</param>
        /// <param name="variables">The variables that are defined by the behavioral component. You should probably not touch these...</param>
        /// <value>
        /// The default builder.
        /// </value>
        public static IBuilder<Func<double>> DefaultBuilderFactory(ISimulation simulation, Dictionary<VariableNode, IVariable<double>> variables)
        {
            var scalar = new SIUnitDefinition("scalar", new SIUnits());

            // Temperature
            if (simulation.TryGetState<ITemperatureSimulationState>(out var tempState))
            {
                variables.Add(Node.Variable("temperature"), new FuncVariable<double>("temperature", () => tempState.Temperature, new SIUnitDefinition("K", new SIUnits(0, 0, 0, 0, 1, 0, 0))));
            }

            // Time variable
            if (simulation.TryGetState<IIntegrationMethod>(out var method))
            {
                variables.Add(Node.Variable("time"), new FuncVariable<double>("time", () => method.Time, new SIUnitDefinition("s", new SIUnits(1, 0, 0, 0, 0, 0, 0))));
            }
            
            // Iteration control
            if (simulation.TryGetState<IIterationSimulationState>(out var iterState))
            {
                variables.Add(Node.Variable("gmin"), new FuncVariable<double>("gmin", () => iterState.Gmin, new SIUnitDefinition("Mho", new SIUnits(3, -2, -1, 2, 0, 0, 0))));
                variables.Add(Node.Variable("sourcefactor"), new FuncVariable<double>("sourcefactor", () => iterState.SourceFactor, scalar));
            }

            // Some standard constants
            variables.Add(Node.Variable("pi"), new ConstantVariable("pi", Math.PI, scalar));
            variables.Add(Node.Variable("e"), new ConstantVariable("e", Math.Exp(1.0), scalar));
            variables.Add(Node.Variable("boltz"), new ConstantVariable("boltz", Constants.Boltzmann, new SIUnitDefinition("J/K", new SIUnits(-2, 2, 1, 0, -1, 0, 0))));
            variables.Add(Node.Variable("planck"), new ConstantVariable("planck", 6.626207004e-34, new SIUnitDefinition("Js", new SIUnits(-1, 2, 1, 0, 0, 0, 0))));
            variables.Add(Node.Variable("echarge"), new ConstantVariable("echarge", Constants.Charge, Units.Coulomb));
            variables.Add(Node.Variable("kelvin"), new ConstantVariable("kelvin", -Constants.CelsiusKelvin, new SIUnitDefinition("K", new SIUnits(0, 0, 0, 0, 1, 0, 0))));
            
            return new FunctionBuilder()
            {
                FunctionDefinitions = FunctionBuilderHelper.Defaults,
                Variables = variables
            };
        }

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>
        /// The expression.
        /// </value>
        [ParameterName("expression"), ParameterName("e"), ParameterInfo("The expression describing the component")]
        public string Expression 
        { 
            get => _expression;
            set
            {
                if (!ReferenceEquals(_expression, value))
                    _isDirty = true;
                _expression = value;
            }
        }
        private string _expression;

        /// <summary>
        /// Gets the function.
        /// </summary>
        /// <value>
        /// The function.
        /// </value>
        [ParameterName("node"), ParameterInfo("The node that represents the expression")]
        public Node Function
        {
            get
            {
                if (_isDirty)
                {
                    if (_parseAction == null || _expression == null)
                        _function = null;
                    else
                        _function = _parseAction(_expression);
                }
                return _function;
            }
        }
        private Node _function;

        /// <summary>
        /// Gets or sets the parse action.
        /// </summary>
        /// <value>
        /// The parse action.
        /// </value>
        public Func<string, Node> ParseAction
        {
            get => _parseAction;
            set
            {
                if (_parseAction != value)
                    _isDirty = true;
                _parseAction = value;
            }
        }
        private Func<string, Node> _parseAction = e => new Parser().Parse(e);

        /// <summary>
        /// Gets or sets the builder factory.
        /// </summary>
        /// <value>
        /// The builder factory.
        /// </value>
        public BuilderFactoryMethod BuilderFactory { get; set; } = DefaultBuilderFactory;

        /// <summary>
        /// A delegate for creating an <see cref="IBuilder{T}"/>.
        /// </summary>
        /// <param name="simulation">The simulation for which the variables need to be created.</param>
        /// <param name="variables">The variables needed for building.</param>
        /// <returns>The value.</returns>
        public delegate IBuilder<Func<double>> BuilderFactoryMethod(ISimulation simulation, Dictionary<VariableNode, IVariable<double>> variables);
    }
}
