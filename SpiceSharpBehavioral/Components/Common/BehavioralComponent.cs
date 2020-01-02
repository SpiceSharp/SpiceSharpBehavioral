using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    /// <summary>
    /// Template for a behavioral component.
    /// </summary>
    public abstract class BehavioralComponent : Component,
        IParameterized<BaseParameters>
    {
        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public BaseParameters Parameters { get; } = new BaseParameters();

        /// <summary>
        /// Create a new instance of the <see cref="BehavioralComponent"/> class.
        /// </summary>
        /// <param name="name">Name of the component.</param>
        /// <param name="nodeCount">Number of pins.</param>
        protected BehavioralComponent(string name, int nodeCount)
            : base(name, nodeCount)
        {
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            // Allocate behaviors of existing
            var parser = new SimpleParser();
            parser.VariableFound += (sender, e) => e.Result = 0.0;
            parser.FunctionFound += (sender, e) => e.Result = 0.0;
            parser.SpicePropertyFound += (sender, e) =>
            {
                if (Parameters.PropertyComparer.Equals(e.Property.Identifier, "I"))
                {
                    // Get the component name
                    var source = e.Property[0];
                    var behavior = simulation.EntityBehaviors[source]; // Will make sure these behaviors are created first
                }
                e.Apply(() => 0.0);
            };
            parser.Parse(Parameters.Expression);
        }
    }
}
