using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.BehavioralCapacitorBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="BehavioralCapacitor"/>
    /// </summary>
    public class BiasingBehavior : Behavior
    {
        /// <summary>
        /// The variables.
        /// </summary>
        protected readonly OnePort<double> Variables;

        /// <summary>
        /// The function.
        /// </summary>
        protected readonly Func<double> Value;

        /// <summary>
        /// The functions that compute the derivatives.
        /// </summary>
        protected readonly Tuple<VariableNode, IVariable<double>, Func<double>>[] Functions;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public BiasingBehavior(BehavioralBindingContext context)
            : base(context)
        {
            // Make sure that we have access to the voltage over the behavior
            var bp = context.GetParameterSet<Parameters>();
            var state = context.GetState<IBiasingSimulationState>();
            Variables = new OnePort<double>(
                state.GetSharedVariable(context.Nodes[0]),
                state.GetSharedVariable(context.Nodes[1]));

            // Create the derivatives, while also giving access to the voltage across the capacitor
            var replacer = new NodeReplacer
            {
                Map = new Dictionary<VariableNode, Node>(new VariableNodeComparer(null, null, bp.VariableComparer))
                {
                    { Node.Variable("x"), Node.Voltage(context.Nodes[0]) - Node.Voltage(context.Nodes[1]) }
                }
            };
            var df = context.CreateDerivatives(bp.BuilderFactory, replacer.Build(bp.Function));
            Value = df.Item1;
            Functions = df.Item2;
        }
    }
}
