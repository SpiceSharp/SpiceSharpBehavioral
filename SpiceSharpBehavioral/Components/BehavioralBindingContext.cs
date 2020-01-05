using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Components.BehavioralBehaviors
{
    /// <summary>
    /// A binding context for behavioral components.
    /// </summary>
    /// <seealso cref="ComponentBindingContext" />
    public class BehavioralBindingContext : ComponentBindingContext
    {
        /// <summary>
        /// Gets the behaviors that the behavioral component uses.
        /// </summary>
        /// <value>
        /// The behaviors that the behavioral component uses.
        /// </value>
        public IDictionary<string, IBehaviorContainer> Behaviors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralBindingContext"/> class.
        /// </summary>
        /// <param name="component">The component creating the behavior.</param>
        /// <param name="simulation">The simulation for which a behavior is created.</param>
        /// <param name="behaviors">The behaviors that need to be included.</param>
        public BehavioralBindingContext(Component component, ISimulation simulation, IEnumerable<string> behaviors)
            : base(component, simulation)
        {
            var dict = new Dictionary<string, IBehaviorContainer>(simulation.EntityBehaviors.Comparer);
            foreach (var behavior in behaviors)
                dict.Add(behavior, simulation.EntityBehaviors[behavior]);
        }
    }
}
