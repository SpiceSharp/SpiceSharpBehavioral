using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    /// <summary>
    /// A template for behavioral sources.
    /// </summary>
    public abstract class BehavioralBehavior : Behavior
    {
        /// <summary>
        /// Gets the state of the biasing.
        /// </summary>
        /// <value>
        /// The state of the biasing.
        /// </value>
        protected IBiasingSimulationState BiasingState { get; }

        /// <summary>
        /// Gets the current evaluated value.
        /// </summary>
        protected double CurrentValue { get; private set; }

        /// <summary>
        /// Gets the parsed expressions.
        /// </summary>
        public BehavioralFunction Function { get; private set; }

        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        public BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BehavioralBehavior(string name, BehavioralBindingContext context) 
            : base(name) 
        {
            // Create a parser
            var bp = context.GetParameterSet<BaseParameters>();
            BiasingState = context.GetState<IBiasingSimulationState>();
            var parser = context.GetParameterSet<BaseParameters>().Parser.Create();
            var variables = context.Variables;

            // We want to keep track of derivatives, and which column they map to
            var derivatives = new Dictionary<Variable, int>();
            var map = BiasingState.Map;
            int Derivative(Variable variable)
            {
                if (variable == variables.Ground)
                    return -1;
                if (!derivatives.TryGetValue(variable, out var d))
                    derivatives.Add(variable, d = derivatives.Count + 1);
                return d;
            };

            // Catch the derivatives for Y-matrix loading
            void SpicePropertyFound(object sender, SpicePropertyFoundEventArgs<double> e)
            {
                var property = e.Property;
                if (BaseParameters.PropertyComparer.Equals(property.Identifier, "V"))
                {
                    // Only recognize V(a) or V(a, b)
                    if (property.ArgumentCount != 1 && property.ArgumentCount != 2)
                        return;

                    // Get the nodes
                    var node = variables.MapNode(property[0], VariableType.Voltage);
                    var reference = property.ArgumentCount > 1 ? variables.MapNode(property[1], VariableType.Voltage) : variables.Ground;
                    if (node != reference)
                    {
                        var nodeIndex = map[node];
                        var referenceIndex = map[reference];
                        if (nodeIndex > 0 && referenceIndex > 0)
                        {
                            e.Apply(null, Derivative(node), 1.0);
                            e.Apply(() => BiasingState.Solution[nodeIndex] - BiasingState.Solution[referenceIndex], Derivative(reference), -1.0);
                        }
                        else if (nodeIndex > 0)
                            e.Apply(() => BiasingState.Solution[nodeIndex], Derivative(node), 1.0);
                        else if (referenceIndex > 0)
                            e.Apply(() => -BiasingState.Solution[referenceIndex], Derivative(reference), -1.0);
                    }
                }
                else if (BaseParameters.PropertyComparer.Equals(property.Identifier, "I"))
                {
                    // Only recognized I(xxx)
                    if (property.ArgumentCount != 1)
                        return;

                    var component = property[0];
                    if (context.References.TryGetValue(property[0], out var container))
                    {
                        if (container.TryGetValue<IBranchedBehavior>(out var behavior))
                        {
                            int index = BiasingState.Map[behavior.Branch];
                            e.Apply(() => BiasingState.Solution[index], Derivative(behavior.Branch), 1.0);
                        }
                        else
                        {
                            var getter = container.CreatePropertyGetter<double>("i");
                            if (getter != null)
                                e.Apply(getter);
                        }
                    }
                }
            };
            parser.SpicePropertyFound += SpicePropertyFound;
            var parsedResult = parser.Parse(BaseParameters.Expression);
            parser.SpicePropertyFound -= SpicePropertyFound;
            Function = new BehavioralFunction(derivatives, parsedResult);
        }

        /// <summary>
        /// Builds the action that can fill up the matrix
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="positive">The positive.</param>
        /// <param name="negative">The negative.</param>
        /// <returns>The action.</returns>
        protected Action BuildAction(ISparseSolver<double> solver, Variable positive, Variable negative)
        {
            var posIndex = BiasingState.Map[positive];
            var negIndex = BiasingState.Map[negative];

            // If the contributions cancel each other out anyway, don't bother compiling
            if (posIndex == negIndex)
                return null;

            // Initialize contributions
            double cumulated = 0.0;
            List<Action> _contributions = new List<Action>();
            {
                var func = Function.Value;
                _contributions.Add(() =>
                {
                    cumulated = func();
                    CurrentValue = cumulated;
                });
            }

            // Fill Y-matrix
            foreach (var item in Function.Derivatives)
            {
                var index = item.Key;
                var func = item.Value;

                // Ignore 0-contributions
                if (func == null)
                    continue;

                Element<double> posElt = null, negElt = null;
                if (posIndex > 0)
                    posElt = solver.GetElement(posIndex, index);
                if (negIndex > 0)
                    negElt = solver.GetElement(negIndex, index);

                if (posElt != null && negElt != null)
                {
                    _contributions.Add(() =>
                    {
                        var derivative = func();
                        posElt.Value += derivative;
                        negElt.Value -= derivative;
                        cumulated -= BiasingState.Solution[index] * derivative;
                    });
                }
                else if (posElt != null)
                {
                    _contributions.Add(() =>
                    {
                        var derivative = func();
                        posElt.Value += derivative;
                        cumulated -= BiasingState.Solution[index] * derivative;
                    });
                }
                else if (negElt != null)
                {
                    _contributions.Add(() =>
                    {
                        var derivative = func();
                        negElt.Value -= derivative;
                        cumulated -= BiasingState.Solution[index] * derivative;
                    });
                }
            }

            // Fill Rhs-vectors
            Element<double> posRhs = null, negRhs = null;
            if (posIndex > 0)
                posRhs = solver.GetElement(posIndex);
            if (negIndex > 0)
                negRhs = solver.GetElement(negIndex);
            if (posRhs != null && negRhs != null)
                _contributions.Add(() =>
                {
                    posRhs.Value -= cumulated;
                    negRhs.Value += cumulated;
                });
            else if (posRhs != null)
                _contributions.Add(() => posRhs.Value -= cumulated);
            else if (negRhs != null)
                _contributions.Add(() => negRhs.Value += cumulated);

            // Build the total behavioral method, ignore if nothing has been done
            if (_contributions.Count == 0)
                return null;
            else
            {
                var actions = _contributions.ToArray();
                return () =>
                {
                    for (var i = 0; i < actions.Length; i++)
                        actions[i]();
                };
            }
        }
    }
}
