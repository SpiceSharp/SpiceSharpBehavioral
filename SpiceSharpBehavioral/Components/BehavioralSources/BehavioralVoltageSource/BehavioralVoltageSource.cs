using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Components.BehavioralVoltageSourceBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A behavioral voltage source.
    /// </summary>
    /// <seealso cref="BehavioralComponent" />
    public class BehavioralVoltageSource : BehavioralComponent
    {
        /// <summary>
        /// The behavioral voltage source base pin count
        /// </summary>
        public const int BehavioralVoltageSourceBasePinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        public BehavioralVoltageSource(string name)
            : base(name, BehavioralVoltageSourceBasePinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="expression">The expression.</param>
        public BehavioralVoltageSource(string name, string pos, string neg, string expression)
            : this(name)
        {
            Connect(pos, neg);
            Parameters.Expression = expression;
        }

        /// <summary>
        /// Creates the behaviors.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            // We have to add the behavior container immediately to allow self-referencing
            var behaviors = new BehaviorContainer(Name);
            behaviors.Add<IBranchedBehavior>(new BranchedBehavior(Name, simulation.Variables));
            simulation.EntityBehaviors.Add(behaviors);

            // Parse the expression to be used
            var derivatives = GetDescription(simulation);

            // Create the context, and use it to create our behaviors
            var context = new BehavioralComponentContext(this, simulation, LinkParameters, derivatives);
            behaviors
                .AddIfNo<IFrequencyBehavior>(simulation, () => new FrequencyBehavior(Name, context))
                .AddIfNo<IBiasingBehavior>(simulation, () => new BiasingBehavior(Name, context));
        }
    }
}
