using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
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
        /// <param name="builderFactory">The builder factory for creating the functions.</param>
        /// <param name="function">The function.</param>
        /// <param name="ownBranch">Any own branch current that might be defined.</param>
        /// <returns>The derivatives with respect to all variables.</returns>
        public Tuple<Func<double>, Tuple<VariableNode, IVariable<double>, Func<double>>[]> CreateDerivatives(Parameters.BuilderFactoryMethod builderFactory, Node function, IVariable<double> ownBranch = null)
        {
            var state = GetState<IBiasingSimulationState>();
            var comparer = new VariableNodeComparer(state.Comparer, Simulation.EntityBehaviors.Comparer);

            // Derive the function
            var nf = new NodeFinder();
            var variables = new Dictionary<VariableNode, IVariable<double>>();
            foreach (var variable in nf.Build(function))
            {
                if (variables.ContainsKey(variable))
                    continue;
                variables.Add(variable, MapNode(state, variable, ownBranch));
            }

            // Get the derivatives
            var deriver = new Derivatives(variables.Keys.ToArray());
            var df = deriver.Derive(function);

            // Build
            var builder = builderFactory(variables);
            var value = builder.Build(function);
            var derivatives = new Tuple<VariableNode, IVariable<double>, Func<double>>[df.Count];
            int index = 0;
            foreach (var pair in df)
                derivatives[index++] = Tuple.Create(pair.Key, variables[pair.Key], builder.Build(pair.Value));
            return Tuple.Create(value, derivatives);
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
                    throw new SpiceSharpException($"Could not determine the variable {Entity.Name}");
            }
        }
    }
}
