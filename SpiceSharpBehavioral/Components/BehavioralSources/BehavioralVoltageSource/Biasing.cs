using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using SpiceSharp.Attributes;
using System.Collections.Generic;
using SpiceSharpBehavioral.Builders.Functions;
using SpiceSharp.Components.BehavioralSources;
using System.Linq;

namespace SpiceSharp.Components.BehavioralVoltageSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="BehavioralVoltageSource"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IBiasingBehavior" />
    /// <seealso cref="IBranchedBehavior{T}"/>
    [BehaviorFor(typeof(BehavioralVoltageSource)), AddBehaviorIfNo(typeof(IBiasingBehavior))]
    [GeneratedParameters]
    public partial class Biasing : Behavior,
        IBiasingBehavior,
        IBranchedBehavior<double>
    {
        private readonly OnePort<double> _variables;
        private readonly IVariable<double> _branch;
        private readonly ElementSet<double> _coreElements;
        private readonly BehavioralContributions<double> _contributions;

        /// <summary>
        /// Gets the variables that are associated with each variable node.
        /// </summary>
        protected Dictionary<VariableNode, IVariable<double>> VariableNodes { get; }

        /// <summary>
        /// The function that computes the value.
        /// </summary>
        protected readonly Node Function;

        /// <summary>
        /// The functions that compute the derivatives.
        /// </summary>
        protected readonly Dictionary<VariableNode, Node> Derivatives;

        /// <summary>
        /// Gets the branch equation variable.
        /// </summary>
        /// <value>
        /// The branch equation variable.
        /// </value>
        IVariable<double> IBranchedBehavior<double>.Branch => _branch;

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        /// <value>
        /// The voltage.
        /// </value>
        [ParameterName("v"), ParameterInfo("Instantaneous voltage", Units = "V")]
        public double Voltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Instantaneous current", Units = "A")]
        public double Current => _branch.Value;

        /// <summary>
        /// Gets the instantaneous power.
        /// </summary>
        /// <value>
        /// The power.
        /// </value>
        [ParameterName("p"), ParameterInfo("Instantaneous power", Units = "W")]
        public double Power => Voltage * -Current;

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Biasing(BehavioralBindingContext context) 
            : base(context)
        {
            var bp = context.GetParameterSet<Parameters>();
            var state = context.GetState<IBiasingSimulationState>();
            _variables = new OnePort<double>(
                state.GetSharedVariable(context.Nodes[0]),
                state.GetSharedVariable(context.Nodes[1]));
            _branch = state.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);

            // Let's build the derivative functions and get their matrix locations/rhs locations
            Function = bp.Function;
            Derivatives = context.CreateDerivatives(Function);
            var builder = new RealFunctionBuilder();
            VariableNodes = context.MapVariableNodes(state, Function, builder, _branch);
            bp.RegisterBuilder(context, builder);

            // Get the elements
            _contributions = new(state, null, _branch, Derivatives
                .Select(d => (Variable: VariableNodes[d.Key], Derivative: d.Value))
                .Where(d => state.Map.Contains(d.Variable))
                .Select(d => (d.Variable, builder.Build(d.Derivative)))
                .ToList(), builder.Build(Function));
            int br = state.Map[_branch];
            int pos = state.Map[_variables.Positive];
            int neg = state.Map[_variables.Negative];
            _coreElements = new ElementSet<double>(state.Solver, new[] {
                new MatrixLocation(br, pos),
                new MatrixLocation(br, neg),
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br)
            });
        }

        /// <summary>
        /// Loads the Y-matrix and right hand side vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            _contributions.Load();
            _coreElements.Add(1.0, -1.0, 1.0, -1.0);
        }
    }
}
