using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    /// <summary>
    /// A binding context for a <see cref="BehavioralComponent"/>.
    /// </summary>
    /// <seealso cref="ComponentBindingContext" />
    public class BehavioralBindingContext : ComponentBindingContext
    {
        /// <summary>
        /// Gets the references.
        /// </summary>
        /// <value>
        /// The references.
        /// </value>
        public Dictionary<string, BehaviorContainer> References { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralBindingContext"/> class.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="simulation">The simulation.</param>
        /// <param name="comparer">The comparer.</param>
        public BehavioralBindingContext(Component component, ISimulation simulation, IEqualityComparer<string> comparer)
            : base(component, simulation, component.LinkParameters)
        {
            References = new Dictionary<string, BehaviorContainer>(comparer);
        }
    }
}
