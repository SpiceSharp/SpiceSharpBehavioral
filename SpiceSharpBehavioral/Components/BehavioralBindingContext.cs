using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SpiceSharp.Components.BehavioralComponents
{
    /// <summary>
    /// A context for behavioral components.
    /// </summary>
    /// <seealso cref="ComponentBindingContext" />
    [BindingContextFor(typeof(BehavioralCapacitor))]
    [BindingContextFor(typeof(BehavioralResistor))]
    [BindingContextFor(typeof(BehavioralVoltageSource))]
    [BindingContextFor(typeof(BehavioralCurrentSource))]
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
                    IBehaviorContainer container;
                    IBiasingBehavior tmpb;
                    IFrequencyBehavior tmpf;
                    
                    // Get the relevant behaviors
                    if (Simulation.EntityBehaviors.Comparer.Equals(node.Name, Behaviors.Name))
                        container = Behaviors;
                    else
                        container = Simulation.EntityBehaviors[node.Name];

                    if (container == Behaviors)
                    {
                        if (ownBranch == null)
                            throw new SpiceSharpException($"The behavior for {Entity.Name} does not define a branch current.");
                        return ownBranch;
                    }
                    else if (container.TryGetValue<IBranchedBehavior<T>>(out var branched))
                    {
                        // Best-case scenario! Our behaviors define a branched behavior!
                        return branched.Branch;
                    }
                    else if (typeof(T) == typeof(double) && container.TryGetValue(out tmpb) && tmpb is CurrentSources.Biasing biasing)
                    {
                        // If whatever is being is asked is the current from a current source, then we also don't have a problem
                        var result = new FuncVariable<double>($"I({biasing.Name})", () => biasing.Current, Units.Ampere);
                        return result as IVariable<T>;
                    }
                    else if (typeof(T) == typeof(Complex) && container.TryGetValue(out tmpf) && tmpf is CurrentSources.Frequency frequency)
                    {
                        // Current source = no problem
                        var result = new FuncVariable<Complex>($"I({frequency.Name})", () => frequency.ComplexCurrent, Units.Ampere);
                        return result as IVariable<T>;
                    }
                    else
                    {
                        var result = container.CreatePropertyGetter<T>("i");
                        if (result == null)
                            goto default;
                        SpiceSharpWarning.Warning(this, SpiceSharpBehavioral.Properties.Resources.LooseLinkCurrent.FormatString(container.Name));
                        return new FuncVariable<T>($"@{container.Name}[i]", result, Units.Ampere);
                    }

                default:
                    throw new SpiceSharpException($"Could not determine the variable {node.Name}");
            }
        }
    }
}
