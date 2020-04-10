using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralCurrentSourceBehaviors;
using SpiceSharp.Attributes;
using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A behavioral current source.
    /// </summary>
    /// <seealso cref="BehavioralComponent" />
    [Pin(0, "I+"), Pin(1, "I-"), Connected]
    public class BehavioralCurrentSource : BehavioralComponent
    {
        /// <summary>
        /// The behavioral current source base pin count
        /// </summary>
        public const int BehavioralCurrentSourceBasePinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        public BehavioralCurrentSource(string name)
            : base(name, BehavioralCurrentSourceBasePinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralCurrentSource"/> class.
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

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            simulation.EntityBehaviors.Add(behaviors);

            // Create the context, and use it to create our behaviors
            var context = new BehavioralComponentContext(this, simulation, LinkParameters, VariableNodes);
            behaviors
                // .AddIfNo<IFrequencyBehavior>(simulation, () => new FrequencyBehavior(Name, context))
                .AddIfNo<IBiasingBehavior>(simulation, () => new BiasingBehavior(Name, context));
        }
    }
}
