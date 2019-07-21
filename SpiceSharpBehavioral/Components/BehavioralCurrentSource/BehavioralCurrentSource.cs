using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;
using SpiceSharpBehavioral.Components.BehavioralCurrentSourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A behavioral current source.
    /// </summary>
    [Pin(0, "I+"), Pin(1, "I-"), Connected()]
    public class BehavioralCurrentSource : BehavioralComponent
    {
        static BehavioralCurrentSource()
        {
            RegisterBehaviorFactory(typeof(BehavioralCurrentSource), new BehaviorFactoryDictionary()
            {
                { typeof(BiasingBehavior), entity => new BiasingBehavior(entity.Name) },
                { typeof(FrequencyBehavior), entity => new FrequencyBehavior(entity.Name) }
            });
        }

        /// <summary>
        /// The behavioral current source pin count
        /// </summary>
        public const int BehavioralCurrentSourcePinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public BehavioralCurrentSource(string name)
            : base(name, BehavioralCurrentSourcePinCount)
        {
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralCurrentSource"/> class. Current flows from the positive
        /// to the negative node.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="expression">The expression.</param>
        public BehavioralCurrentSource(string name, string pos, string neg, string expression)
            : base(name, BehavioralCurrentSourcePinCount)
        {
            ParameterSets.Add(new BaseParameters { Expression = expression });
            Connect(pos, neg);
        }
    }
}
