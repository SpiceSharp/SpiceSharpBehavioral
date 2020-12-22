using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using SpiceSharpBehavioral;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Builders.Functions;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

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
        /// A default method for registering variables with a real function builder.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        public void RegisterDefaultRealBuilder(object sender, BuilderCreatedEventArgs<double> args)
        {
            var context = args.Context;
            var builder = args.Builder;
            var variables = new Dictionary<string, IVariable<double>>(VariableComparer);
            var scalar = new SIUnitDefinition("scalar", new SIUnits());

            // Register the regular functions
            builder.RegisterDefaultFunctions();

            // Temperature
            if (context.TryGetState<ITemperatureSimulationState>(out var tempState))
            {
                variables.Add("temperature", new FuncVariable<double>("temperature", () => tempState.Temperature, new SIUnitDefinition("K", new SIUnits(0, 0, 0, 0, 1, 0, 0))));
            }

            // Time variable
            if (context.TryGetState<IIntegrationMethod>(out var method))
            {
                variables.Add("time", new FuncVariable<double>("time", () => method.Time, new SIUnitDefinition("s", new SIUnits(1, 0, 0, 0, 0, 0, 0))));
            }
            
            // Iteration control
            if (context.TryGetState<IIterationSimulationState>(out var iterState))
            {
                variables.Add("gmin", new FuncVariable<double>("gmin", () => iterState.Gmin, new SIUnitDefinition("Mho", new SIUnits(3, -2, -1, 2, 0, 0, 0))));
                variables.Add("sourcefactor", new FuncVariable<double>("sourcefactor", () => iterState.SourceFactor, scalar));
            }

            // Some standard constants
            variables.Add("pi", new ConstantVariable<double>("pi", Math.PI, scalar));
            variables.Add("e", new ConstantVariable<double>("e", Math.Exp(1.0), scalar));
            variables.Add("boltz", new ConstantVariable<double>("boltz", Constants.Boltzmann, new SIUnitDefinition("J/K", new SIUnits(-2, 2, 1, 0, -1, 0, 0))));
            variables.Add("planck", new ConstantVariable<double>("planck", 6.626207004e-34, new SIUnitDefinition("Js", new SIUnits(-1, 2, 1, 0, 0, 0, 0))));
            variables.Add("echarge", new ConstantVariable<double>("echarge", Constants.Charge, Units.Coulomb));
            variables.Add("kelvin", new ConstantVariable<double>("kelvin", -Constants.CelsiusKelvin, new SIUnitDefinition("K", new SIUnits(0, 0, 0, 0, 1, 0, 0))));

            // Register the variables
            builder.VariableFound += (sender, args) =>
            {
                if (args.Variable == null && variables.TryGetValue(args.Node.Name, out var variable))
                    args.Variable = variable;
            };
        }

        /// <summary>
        /// A default method for registering variables with a complex function builder.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        public void RegisterDefaultComplexBuilder(object sender, BuilderCreatedEventArgs<Complex> args)
        {
            var context = args.Context;
            var builder = args.Builder;
            var variables = new Dictionary<string, IVariable<Complex>>(VariableComparer);
            var scalar = new SIUnitDefinition("scalar", new SIUnits());

            // Register the default functions
            builder.RegisterDefaultFunctions();

            // Temperature
            if (context.TryGetState<ITemperatureSimulationState>(out var tempState))
            {
                variables.Add("temperature", new FuncVariable<Complex>("temperature", () => tempState.Temperature, new SIUnitDefinition("K", new SIUnits(0, 0, 0, 0, 1, 0, 0))));
            }

            // Iteration control
            if (context.TryGetState<IIterationSimulationState>(out var iterState))
            {
                variables.Add("gmin", new FuncVariable<Complex>("gmin", () => iterState.Gmin, new SIUnitDefinition("Mho", new SIUnits(3, -2, -1, 2, 0, 0, 0))));
                variables.Add("sourcefactor", new FuncVariable<Complex>("sourcefactor", () => iterState.SourceFactor, scalar));
            }

            // Some standard constants
            variables.Add("pi", new ConstantVariable<Complex>("pi", Math.PI, scalar));
            variables.Add("e", new ConstantVariable<Complex>("e", Math.Exp(1.0), scalar));
            variables.Add("boltz", new ConstantVariable<Complex>("boltz", Constants.Boltzmann, new SIUnitDefinition("J/K", new SIUnits(-2, 2, 1, 0, -1, 0, 0))));
            variables.Add("planck", new ConstantVariable<Complex>("planck", 6.626207004e-34, new SIUnitDefinition("Js", new SIUnits(-1, 2, 1, 0, 0, 0, 0))));
            variables.Add("echarge", new ConstantVariable<Complex>("echarge", Constants.Charge, Units.Coulomb));
            variables.Add("kelvin", new ConstantVariable<Complex>("kelvin", -Constants.CelsiusKelvin, new SIUnitDefinition("K", new SIUnits(0, 0, 0, 0, 1, 0, 0))));

            // Register these variables
            builder.VariableFound += (sender, args) =>
            {
                if (args.Variable == null && variables.TryGetValue(args.Node.Name, out var variable))
                    args.Variable = variable;
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
        /// Occurs when a builder has been created that uses real values.
        /// </summary>
        public event EventHandler<BuilderCreatedEventArgs<double>> RealBuilderCreated;

        /// <summary>
        /// Occurs when a builder has been created that uses complex values.
        /// </summary>
        public event EventHandler<BuilderCreatedEventArgs<Complex>> ComplexBuilderCreated;

        /// <summary>
        /// Registers a new function builder.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="builder">The builder.</param>
        public void RegisterBuilder(IComponentBindingContext context, IFunctionBuilder<double> builder) 
            => RealBuilderCreated?.Invoke(this, new BuilderCreatedEventArgs<double>(context, builder));

        /// <summary>
        /// Register a new function builder.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="builder">The builder.</param>
        public void RegisterBuilder(IComponentBindingContext context, IFunctionBuilder<Complex> builder)
            => ComplexBuilderCreated?.Invoke(this, new BuilderCreatedEventArgs<Complex>(context, builder));

        /// <summary>
        /// Initializes a new instance of the <see cref="Parameters"/> class.
        /// </summary>
        public Parameters()
        {
            RealBuilderCreated += RegisterDefaultRealBuilder;
            ComplexBuilderCreated += RegisterDefaultComplexBuilder;
        }
    }
}
