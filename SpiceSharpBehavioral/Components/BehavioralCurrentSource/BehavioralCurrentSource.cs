using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
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
            : this(name)
        {
            Connect(pos, neg);
            Parameters.Expression = expression;
        }

        public override void CreateBehaviors(ISimulation simulation)
        {
            base.CreateBehaviors(simulation);

            var container = new BehaviorContainer(Name);
            var context = new ComponentBindingContext(this, simulation);
            container
                .AddIfNo<IFrequencyBehavior>(simulation, () => new FrequencyBehavior(Name, context))
                .AddIfNo<IBiasingBehavior>(simulation, () => new BiasingBehavior(Name, context));
            simulation.EntityBehaviors.Add(container);
        }
    }
}
