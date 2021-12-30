using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralSources;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Builders.Functions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Components.BehavioralCapacitorBehaviors
{
    /// <summary>
    /// Time-dependent behavior for a <see cref="BehavioralCapacitor"/>.
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="ITimeBehavior"/>
    [BehaviorFor(typeof(BehavioralCapacitor)), AddBehaviorIfNo(typeof(ITimeBehavior))]
    [GeneratedParameters]
    public partial class Time : Biasing,
        IBiasingBehavior,
        ITimeBehavior
    {
        private readonly ITimeSimulationState _time;
        private readonly IIntegrationMethod _method;
        private readonly IDerivative _qcap;
        private readonly Func<double> _value;
        private readonly BehavioralContributions<double> _contributions;
        private double _g;

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Instantaneous current", Units = "A")]
        public double Current => _qcap.Derivative;

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        /// <value>
        /// The voltage.
        /// </value>
        [ParameterName("v"), ParameterInfo("Instantaneous voltage", Units = "V")]
        public double Voltage => Variables.Positive.Value - Variables.Negative.Value;

        /// <summary>
        /// Gets the power.
        /// </summary>
        /// <value>
        /// The power.
        /// </value>
        [ParameterName("p"), ParameterInfo("Instantaneous power", Units = "W")]
        public double Power => -Voltage * Current;

        /// <summary>
        /// Initializes a new instance of the <see cref="Time"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Time(BehavioralBindingContext context)
            : base(context)
        {
            var bp = context.GetParameterSet<Parameters>();
            var state = context.GetState<IBiasingSimulationState>();
            _method = context.GetState<IIntegrationMethod>();
            _time = context.GetState<ITimeSimulationState>();

            // Make the builder
            var builder = new RealFunctionBuilder();
            context.MapVariableNodes(VariableNodes, builder);
            bp.RegisterBuilder(context, builder);

            // Get the contributions
            _value = builder.Build(Function);
            _qcap = _method.CreateDerivative();
            _contributions = new(state, Variables, Derivatives
                .Select(d => (Variable: VariableNodes[d.Key], Derivative: d.Value))
                .Where(d => state.Map.Contains(d.Variable))
                .Select(d =>
                {
                    var f = builder.Build(d.Derivative);
                    return (d.Variable, new Func<double>(() => _g * f()));
                }).ToList(), () => _qcap.Derivative);
        }

        /// <summary>
        /// Initialization.
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
            _qcap.Value = _value();
        }

        /// <summary>
        /// Load behavior.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            if (_time.UseDc)
                return;

            _qcap.Value = _value();
            _qcap.Derive();
            _g = _method.Slope;
            _contributions.Load();
        }
    }
}
