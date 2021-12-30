using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralSources;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Builders.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SpiceSharp.Components.BehavioralCapacitorBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="BehavioralCapacitor"/>.
    /// </summary>
    [BehaviorFor(typeof(BehavioralCapacitor)), AddBehaviorIfNo(typeof(IFrequencyBehavior))]
    [GeneratedParameters]
    public partial class Frequency : Biasing,
        IFrequencyBehavior
    {
        private readonly IComplexSimulationState _state;
        private readonly OnePort<Complex> _variables;
        private readonly BehavioralContributions<Complex> _contributions;

        /// <summary>
        /// Gets the complex voltage.
        /// </summary>
        /// <value>The complex voltage.</value>
        [ParameterName("v"), ParameterInfo("The complex voltage", Units = "V")]
        public Complex ComplexVoltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Gets the complex current.
        /// </summary>
        [ParameterName("i"), ParameterInfo("The complex current", Units = "A")]
        public Complex ComplexCurrent => _contributions.Current;

        /// <summary>
        /// Gets the complex power.
        /// </summary>
        [ParameterName("p"), ParameterInfo("The complex power", Units = "W")]
        public Complex ComplexPower => ComplexVoltage * Complex.Conjugate(ComplexCurrent);

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public Frequency(BehavioralBindingContext context)
            : base(context)
        {
            var bp = context.GetParameterSet<Parameters>();
            _state = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(
                _state.GetSharedVariable(context.Nodes[0]),
                _state.GetSharedVariable(context.Nodes[1]));

            // Make the builder
            var builder = new ComplexFunctionBuilder();
            context.ConvertVariables(VariableNodes, builder);
            bp.RegisterBuilder(context, builder);

            // Get the contributions
            _contributions = new(_state, _variables, Derivatives
                .Select(d => (Variable: context.GetVariableNode(_state, d.Key), Derivative: d.Value))
                .Where(d => _state.Map.Contains(d.Variable))
                .Select(d =>
                {
                    var f = builder.Build(d.Derivative);
                    return (d.Variable, new Func<Complex>(() => _state.Laplace * f()));
                }).ToList(), null);
        }

        /// <summary>
        /// The frequency-dependent initialization.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// The frequency-dependent load behavior.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            _contributions.Load();
        }
    }
}
