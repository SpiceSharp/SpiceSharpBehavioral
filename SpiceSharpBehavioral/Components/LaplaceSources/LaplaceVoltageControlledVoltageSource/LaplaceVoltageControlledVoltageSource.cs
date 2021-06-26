using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Components.LaplaceVoltageControlledVoltageSourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled voltage source that is described by a transfer function.
    /// </summary>
    public class LaplaceVoltageControlledVoltageSource : Component<LaplaceBehaviors.Parameters>
    {
        /// <summary>
        /// The pin count for a voltage-controlled voltage source.
        /// </summary>
        public const int PinCount = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaplaceVoltageControlledVoltageSource"/>.
        /// </summary>
        /// <param name="name"></param>
        public LaplaceVoltageControlledVoltageSource(string name)
            : base(name, PinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaplaceVoltageControlledVoltageSource"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">the negative node.</param>
        /// <param name="controlPos">The controlling positive node.</param>
        /// <param name="controlNeg">The controlling negative node.</param>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        /// <param name="delay">The delay.</param>
        public LaplaceVoltageControlledVoltageSource(string name, string pos, string neg, string controlPos, string controlNeg,
            double[] numerator, double[] denominator, double delay = 0.0)
            : this(name)
        {
            Connect(pos, neg, controlPos, controlNeg);
            Parameters.Numerator = numerator;
            Parameters.Denominator = denominator;
            Parameters.Delay = delay;
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            var context = new ComponentBindingContext(this, simulation, behaviors);
            behaviors.Build(simulation, context)
                .AddIfNo<ITimeBehavior>(context => new Time(context))
                .AddIfNo<IFrequencyBehavior>(context => new Frequency(context))
                .AddIfNo<IBiasingBehavior>(context => new Biasing(context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
