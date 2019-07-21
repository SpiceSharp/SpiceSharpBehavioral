using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using System;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    public abstract class BehavioralComponent : Component
    {
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
        /// Create the behaviors.
        /// </summary>
        /// <param name="types">The behavior types.</param>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entities">The circuit entities.</param>
        public override void CreateBehaviors(Type[] types, Simulation simulation, EntityCollection entities)
        {
            // Get the expression
            var expression = ParameterSets.Get<BaseParameters>().Expression;

            // We first want to create the behaviors of sources for which we will use the current branches
            var parser = new SimpleParser();

            // We want to avoid throwing exceptions on variables, functions or spice properties, we just want to find the
            // voltage sources our component depends on
            parser.VariableFound += (sender, e) => e.Result = 0.0;
            parser.FunctionFound += (sender, e) => e.Result = 0.0;
            parser.SpicePropertyFound += (sender, e) =>
            {
                if (e.Property.Identifier == "i")
                {
                    var source = e.Property[0];
                    entities[source].CreateBehaviors(types, simulation, entities);
                }
                e.Result = 0.0;
            };
            parser.Parse(expression);

            // Now we can continue with out own behavior creation
            base.CreateBehaviors(types, simulation, entities);
        }
    }
}
