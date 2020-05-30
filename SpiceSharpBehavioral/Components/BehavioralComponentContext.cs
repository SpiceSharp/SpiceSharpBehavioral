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
    public class BehavioralComponentContext : ComponentBindingContext
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
        /// Initializes a new instance of the <see cref="BehavioralComponentContext" /> class.
        /// </summary>
        /// <param name="component">The component creating the behavior.</param>
        /// <param name="simulation">The simulation for which a behavior is created.</param>
        /// <param name="behaviors">The created behaviors.</param>
        /// <param name="linkParameters">Flag indicating that parameters should be linked. If false, only cloned parameters are returned by the context.</param>
        /// <param name="variables">The variables that need to be derived to.</param>
        public BehavioralComponentContext(IComponent component, ISimulation simulation, IBehaviorContainer behaviors, bool linkParameters, IEnumerable<VariableNode> variables) 
            : base(component, simulation, behaviors, linkParameters)
        {
            Branches = new Dictionary<VariableNode, IBehaviorContainer>();

            var varr = variables.ToArray();
            foreach (var variable in varr.Where(vn => vn.NodeType == NodeTypes.Current))
                Branches.Add(variable, simulation.EntityBehaviors[variable.Name]);
            var p = component.GetParameterSet<BaseParameters>();
            var deriver = new Derivatives(varr);
            Derivatives = deriver.Derive(p.Function) ?? new Dictionary<VariableNode, Node>();
        }
    }
}
