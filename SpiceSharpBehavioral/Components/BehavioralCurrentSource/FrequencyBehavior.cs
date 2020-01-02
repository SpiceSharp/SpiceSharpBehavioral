using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;

namespace SpiceSharpBehavioral.Components.BehavioralCurrentSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="BehavioralCurrentSource"/>.
    /// </summary>
    public class FrequencyBehavior : BehavioralFrequencyBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        public FrequencyBehavior(string name, BehavioralBindingContext context)
            : base(name)
        {
        }

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

            // Do other behavioral stuff
            base.Bind(simulation, context);
        }
    }
}
