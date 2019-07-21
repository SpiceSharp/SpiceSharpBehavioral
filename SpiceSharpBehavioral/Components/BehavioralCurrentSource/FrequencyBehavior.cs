using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;

namespace SpiceSharpBehavioral.Components.BehavioralCurrentSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="BehavioralCurrentSource"/>.
    /// </summary>
    public class FrequencyBehavior : BehavioralFrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Creates a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        public FrequencyBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Connects the specified pins.
        /// </summary>
        /// <param name="pins">The pins.</param>
        public void Connect(params int[] pins)
        {
            pins.ThrowIfNot(nameof(pins), 2);
            NegIndex = pins[0];
            PosIndex = pins[1];
        }
    }
}
