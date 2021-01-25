using SpiceSharp.Attributes;
using SpiceSharp.Components.BehavioralComponents;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A behavioral capacitor.
    /// </summary>
    /// <seealso cref="BehavioralComponent"/>
    [Pin(0, "P"), Pin(1, "N")]
    public class BehavioralCapacitor : BehavioralComponent
    {
        /// <summary>
        /// The behavioral capacitor base pin count.
        /// </summary>
        public const int BehavioralCapacitorPinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralCapacitor"/> class.
        /// </summary>
        /// <param name="name">The name of the entity</param>
        public BehavioralCapacitor(string name)
            : base(name, BehavioralCapacitorPinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralCapacitor"/> class.
        /// </summary>
        /// <param name="name">The name of the resistor.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="expression">The expression that describes the resistance.</param>
        public BehavioralCapacitor(string name, string pos, string neg, string expression)
            : this(name)
        {
            Connect(pos, neg);
            Parameters.Expression = expression;
        }
    }
}
