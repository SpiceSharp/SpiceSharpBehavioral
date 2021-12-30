using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;
using System.Numerics;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Attributes;
using System.Collections.Generic;
using SpiceSharpBehavioral.Builders.Functions;
using SpiceSharp.Components.BehavioralSources;
using System.Linq;

namespace SpiceSharp.Components.BehavioralVoltageSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="BehavioralVoltageSource"/>.
    /// </summary>
    /// <seealso cref="Biasing" />
    /// <seealso cref="IFrequencyBehavior" />
    /// <seealso cref="IBranchedBehavior{T}"/>
    [BehaviorFor(typeof(BehavioralVoltageSource)), AddBehaviorIfNo(typeof(IFrequencyBehavior))]
    [GeneratedParameters]
    public partial class Frequency : Biasing,
        IFrequencyBehavior,
        IBranchedBehavior<Complex>
    {
        private readonly OnePort<Complex> _variables;
        private readonly IVariable<Complex> _branch;
        private readonly ElementSet<Complex> _coreElements;
        private readonly BehavioralContributions<Complex> _contributions;

        /// <summary>
        /// Gets the branch equation variable.
        /// </summary>
        /// <value>
        /// The branch equation variable.
        /// </value>
        IVariable<Complex> IBranchedBehavior<Complex>.Branch => _branch;

        /// <summary>
        /// Gets the complex voltage.
        /// </summary>
        /// <value>
        /// The complex voltage.
        /// </value>
        [ParameterName("v"), ParameterName("v_c"), ParameterInfo("The complex voltage", Units = "V")]
        public Complex ComplexVoltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Gets the complex current.
        /// </summary>
        /// <value>
        /// The complex current.
        /// </value>
        [ParameterName("i"), ParameterName("i_c"), ParameterInfo("The complex current", Units = "A")]
        public Complex ComplexCurrent => _branch.Value;

        /// <summary>
        /// Gets the complex power.
        /// </summary>
        /// <value>
        /// The complex power.
        /// </value>
        [ParameterName("p"), ParameterName("p_c"), ParameterInfo("The complex power", Units = "W")]
        public Complex ComplexPower => -ComplexVoltage * Complex.Conjugate(_branch.Value);

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Frequency(BehavioralBindingContext context)
            : base(context)
        {
            var bp = context.GetParameterSet<Parameters>();
            var state = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(
                state.GetSharedVariable(context.Nodes[0]),
                state.GetSharedVariable(context.Nodes[1]));
            _branch = state.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);

            // Make the builder
            var builder = new ComplexFunctionBuilder();
            context.ConvertVariables(VariableNodes, builder);
            bp.RegisterBuilder(context, builder);

            // Get the contributions
            _contributions = new(state, null, _branch, Derivatives
                .Select(d => (Variable: context.GetVariableNode(state, d.Key, _branch), Derivative: d.Value))
                .Where(d => state.Map.Contains(d.Variable))
                .Select(d => (d.Variable, builder.Build(d.Derivative)))
                .ToList(), null);
            int br = state.Map[_branch];
            int pos = state.Map[_variables.Positive];
            int neg = state.Map[_variables.Negative];
            _coreElements = new ElementSet<Complex>(state.Solver, new[] {
                new MatrixLocation(br, pos),
                new MatrixLocation(br, neg),
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br)
            });
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
            _contributions.Calculate();
        }

        /// <summary>
        /// Loads the Y-matrix and right hand side vector.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            _contributions.Apply();
            _coreElements.Add(1.0, -1.0, 1.0, -1.0);
        }
    }
}
