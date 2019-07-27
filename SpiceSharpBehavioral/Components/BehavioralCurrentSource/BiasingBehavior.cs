using SpiceSharp;
using SpiceSharp.Attributes;
using SpiceSharp.Components;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;

namespace SpiceSharpBehavioral.Components.BehavioralCurrentSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="BehavioralCurrentSource"/>.
    /// </summary>
    public class BiasingBehavior : BehavioralBiasingBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Gets the current applied by the source.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        [ParameterName("i"), ParameterInfo("Instantaneous current")]
        public double Current { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public BiasingBehavior(string name) : base(name) { }

        /// <summary>
        /// Connects the specified pins.
        /// </summary>
        /// <param name="pins">The pins.</param>
        public void Connect(params int[] pins)
        {
            pins.ThrowIfNot(nameof(pins), 2);
            PosIndex = pins[0];
            NegIndex = pins[1];
        }
    }
}
