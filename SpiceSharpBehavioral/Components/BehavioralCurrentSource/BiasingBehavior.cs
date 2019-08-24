using SpiceSharp;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;

namespace SpiceSharpBehavioral.Components.BehavioralCurrentSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="BehavioralCurrentSource"/>.
    /// </summary>
    public class BiasingBehavior : BehavioralBiasingBehavior
    {
        /// <summary>
        /// Gets the current applied by the source.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        [ParameterName("i"), ParameterInfo("Instantaneous current")]
        public double Current => CurrentValue;

        /// <summary>
        /// Gets the voltage over the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("v"), ParameterInfo("Instantaneous voltage")]
        public double GetVoltage() => State.ThrowIfNotBound(this).Solution[PosIndex] - State.Solution[NegIndex];

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("p"), ParameterInfo("Instantaneous power")]
        public double GetPower() => (State.ThrowIfNotBound(this).Solution[PosIndex] - State.Solution[NegIndex]) * CurrentValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public BiasingBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            // Get node connections
            if (context is ComponentBindingContext cc)
            {
                PosIndex = cc.Pins[0];
                NegIndex = cc.Pins[1];
            }

            // Do other behavioral stuff.
            base.Bind(simulation, context);
        }
    }
}
