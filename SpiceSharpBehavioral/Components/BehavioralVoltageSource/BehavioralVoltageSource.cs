using SpiceSharp.Components;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;
using SpiceSharpBehavioral.Components.BehavioralVoltageSourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A behavioral voltage source.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.Component" />
    [Pin(0, "V+"), Pin(1, "V-"), VoltageDriver(0, 1)]
    public class BehavioralVoltageSource : BehavioralComponent
    {
        static BehavioralVoltageSource()
        {
            RegisterBehaviorFactory(typeof(BehavioralVoltageSource), new BehaviorFactoryDictionary()
                {
                    { typeof(BiasingBehavior), entity => new BiasingBehavior(entity.Name) },
                    { typeof(FrequencyBehavior), entity => new FrequencyBehavior(entity.Name) }
                });
        }

        /// <summary>
        /// The behavioral voltage source pin count
        /// </summary>
        public const int BehavioralVoltageSourcePinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage source.</param>
        public BehavioralVoltageSource(string name) : base(name, BehavioralVoltageSourcePinCount)
        {
            // Parameters
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage source.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="expression">The expression.</param>
        public BehavioralVoltageSource(string name, string pos, string neg, string expression)
            : base(name, BehavioralVoltageSourcePinCount)
        {
            // Parameters
            var p = new BaseParameters {Expression = expression};
            ParameterSets.Add(p);
            Connect(pos, neg);
        }
    }
}
