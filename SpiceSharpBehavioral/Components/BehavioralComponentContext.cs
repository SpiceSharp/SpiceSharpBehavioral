using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Builders;
using System;

namespace SpiceSharp.Components.BehavioralComponents
{
    /// <summary>
    /// A context for behavioral components.
    /// </summary>
    /// <seealso cref="ComponentBindingContext" />
    public class BehavioralComponentContext : ComponentBindingContext
    {
        /// <summary>
        /// Gets the model description.
        /// </summary>
        /// <value>
        /// The model description.
        /// </value>
        public Derivatives<Func<double>> ModelDescription { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralComponentContext"/> class.
        /// </summary>
        /// <param name="component">The component creating the behavior.</param>
        /// <param name="simulation">The simulation for which a behavior is created.</param>
        /// <param name="linkParameters">Flag indicating that parameters should be linked. If false, only cloned parameters are returned by the context.</param>
        /// <param name="modelDescription">The model description.</param>
        public BehavioralComponentContext(IComponent component, ISimulation simulation, bool linkParameters, Derivatives<Func<double>> modelDescription) 
            : base(component, simulation, linkParameters)
        {
            ModelDescription = modelDescription.ThrowIfNull(nameof(modelDescription));
        }
    }
}
