using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
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
        /// Gets the current references.
        /// </summary>
        /// <value>
        /// The current references.
        /// </value>
        /// <remarks>
        /// The reason we are tracking branches, is because they reference other components.
        /// </remarks>
        public Dictionary<VariableNode, IBehaviorContainer> Branches { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralBindingContext" /> class.
        /// </summary>
        /// <param name="component">The component creating the behavior.</param>
        /// <param name="simulation">The simulation for which a behavior is created.</param>
        /// <param name="behaviors">The created behaviors.</param>
        public BehavioralBindingContext(IComponent component, ISimulation simulation, IBehaviorContainer behaviors) 
            : base(component, simulation, behaviors)
        {
        }

        /// <summary>
        /// Creates the derivatives for the specified function.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns>The derivatives with respect to all variables.</returns>
        public Dictionary<VariableNode, Node> CreateDerivatives(Node function)
        {
            var state = GetState<IBiasingSimulationState>();
            var bp = GetParameterSet<Parameters>();
            var comparer = new VariableNodeComparer(state.Comparer, Simulation.EntityBehaviors.Comparer, bp.VariableComparer);
            
            // Derive the function
            var derivatives = new Derivatives()
            {
                Variables = new HashSet<VariableNode>(comparer)
            };
            var nf = new NodeFinder();
            foreach (var variable in nf.Build(function).Where(v => v.NodeType == NodeTypes.Voltage || v.NodeType == NodeTypes.Current))
            {
                if (derivatives.Variables.Contains(variable))
                    continue;
                derivatives.Variables.Add(variable);
            }
            return derivatives.Derive(function) ?? new Dictionary<VariableNode, Node>(comparer);
        }
                
        /// <summary>
        /// Create a builder.
        /// </summary>
        /// <typeparam name="T">The value.</typeparam>
        /// <param name="builderFactory">A factory for creating a builder.</param>
        /// <param name="variables">The variables.</param>
        /// <returns>The function builder.</returns>
        public IFunctionBuilder<T> CreateBuilder<T>(Parameters.BuilderFactoryMethod<T> builderFactory, Dictionary<VariableNode, IVariable<T>> variables)
        {
            return builderFactory(Simulation, variables);
        }

        /// <summary>
        /// Remap a variable node to an <see cref="IVariable{T}"/>.
        /// </summary>
        /// <typeparam name="T">the variable type.</typeparam>
        /// <param name="factory">The variable factory.</param>
        /// <param name="node">The node.</param>
        /// <param name="ownBranch">The branch.</param>
        /// <returns>The variable.</returns>
        public IVariable<T> MapNode<T>(IVariableFactory<IVariable<T>> factory, VariableNode node, IVariable<T> ownBranch = null)
        {
            switch (node.NodeType)
            {
                case NodeTypes.Voltage:
                    return factory.GetSharedVariable(node.Name);

                case NodeTypes.Current:
                    var container = Simulation.EntityBehaviors[node.Name];
                    if (container == Behaviors)
                    {
                        if (ownBranch == null)
                            throw new SpiceSharpException($"The behavior for {Entity.Name} does not define a branch current.");
                        return ownBranch;
                    }
                    else
                        return container.GetValue<IBranchedBehavior<T>>().Branch;

                default:
                    throw new SpiceSharpException($"Could not determine the variable {node.Name}");
            }
        }
    }
}
