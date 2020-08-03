using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Components.BehavioralComponents
{
    /// <summary>
    /// A context for behavioral components.
    /// </summary>
    /// <seealso cref="ComponentBindingContext" />
    public class BehavioralBindingContext : ComponentBindingContext
    {
        /// <summary>
        /// Gets the derivatives w.r.t. to any voltage and current nodes.
        /// </summary>
        /// <value>
        /// The derivatives.
        /// </value>
        public Dictionary<VariableNode, Node> Derivatives { get; }

        /// <summary>
        /// Gets the current references.
        /// </summary>
        /// <value>
        /// The current references.
        /// </value>
        public Dictionary<VariableNode, IBehaviorContainer> Branches { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralBindingContext" /> class.
        /// </summary>
        /// <param name="component">The component creating the behavior.</param>
        /// <param name="simulation">The simulation for which a behavior is created.</param>
        /// <param name="behaviors">The created behaviors.</param>
        public BehavioralBindingContext(IComponent component, ISimulation simulation, IBehaviorContainer behaviors) 
            : base(component, simulation, behaviors)
        {
            Branches = new Dictionary<VariableNode, IBehaviorContainer>();

            var parameters = component.GetParameterSet<Parameters>();
            var varr = parameters.VariableNodes.ToArray();
            foreach (var variable in varr.Where(vn => vn.NodeType == NodeTypes.Current))
                Branches.Add(variable, simulation.EntityBehaviors[variable.Name]);
            var p = component.GetParameterSet<Parameters>();
            var deriver = new Derivatives(varr);
            Derivatives = deriver.Derive(p.Function) ?? new Dictionary<VariableNode, Node>();
        }
    }
}
