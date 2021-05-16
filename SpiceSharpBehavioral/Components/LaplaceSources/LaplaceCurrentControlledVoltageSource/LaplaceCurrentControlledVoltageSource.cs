using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Components.LaplaceCurrentControlledVoltageSourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled current source that is described by a transfer function.
    /// </summary>
    public class LaplaceCurrentControlledVoltageSource : Component<LaplaceBehaviors.Parameters>,
        ICurrentControllingComponent,
        IRuleSubject
    {
        /// <summary>
        /// The pin count for a current-controlled current source.
        /// </summary>
        public const int PinCount = 2;

        /// <summary>
        /// Gets or sets the controlling source.
        /// </summary>
        public string ControllingSource { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaplaceCurrentControlledCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public LaplaceCurrentControlledVoltageSource(string name)
            : base(name, PinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaplaceCurrentControlledCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="voltageSource">The controlling voltage source.</param>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        /// <param name="delay">The delay.</param>
        public LaplaceCurrentControlledVoltageSource(string name, string pos, string neg, string voltageSource,
            double[] numerator, double[] denominator, double delay = 0.0)
            : this(name)
        {
            Connect(pos, neg);
            ControllingSource = voltageSource;
            Parameters.Numerator = numerator;
            Parameters.Denominator = denominator;
            Parameters.Delay = delay;
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            var context = new CurrentControlledBindingContext(this, simulation, behaviors);
            behaviors.Build(simulation, context)
                .AddIfNo<ITimeBehavior>(context => new Time(context))
                .AddIfNo<IFrequencyBehavior>(context => new Frequency(context))
                .AddIfNo<IBiasingBehavior>(context => new Biasing(context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
