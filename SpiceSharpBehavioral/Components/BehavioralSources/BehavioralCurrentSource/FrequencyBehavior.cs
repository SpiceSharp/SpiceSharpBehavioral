using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharp.Components.BehavioralCurrentSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="BehavioralCurrentSource"/>.
    /// </summary>
    /// <seealso cref="BiasingBehavior" />
    /// <seealso cref="IFrequencyBehavior" />
    [BehaviorFor(typeof(BehavioralCurrentSource), typeof(IFrequencyBehavior), 1)]
    public class FrequencyBehavior : BiasingBehavior,
        IFrequencyBehavior
    {
        private readonly OnePort<Complex> _variables;
        private readonly ElementSet<Complex> _elements;
        private readonly Func<Complex>[] _derivatives;

        /// <summary>
        /// Gets the complex current.
        /// </summary>
        /// <value>
        /// The complex current.
        /// </value>
        public Complex ComplexCurrent { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public FrequencyBehavior(BehavioralBindingContext context)
            : base(context)
        {
            var bp = context.GetParameterSet<Parameters>();
            var state = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(
                state.GetSharedVariable(context.Nodes[0]),
                state.GetSharedVariable(context.Nodes[1]));

            // Build the functions
            _derivatives = new Func<Complex>[Derivatives.Count];
            var builder = new ComplexFunctionBuilder();
            builder.VariableFound += (sender, args) =>
            {
                if (args.Variable == null && DerivativeVariables.TryGetValue(args.Node, out var variable))
                    args.Variable = new FuncVariable<Complex>(variable.Name, () => variable.Value, variable.Unit);
            };
            var rhsLocs = _variables.GetRhsIndices(state.Map);
            var matLocs = new MatrixLocation[Derivatives.Count * 2];
            int index = 0;
            foreach (var pair in Derivatives)
            {
                _derivatives[index] = builder.Build(pair.Value);
                var variable = context.MapNode(state, pair.Key);
                matLocs[index * 2] = new MatrixLocation(rhsLocs[0], state.Map[variable]);
                matLocs[index * 2 + 1] = new MatrixLocation(rhsLocs[1], state.Map[variable]);
                index++;
            }

            // Get the matrix elements
            _elements = new ElementSet<Complex>(state.Solver, matLocs);
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Loads the Y-matrix and right hand side vector.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var values = new Complex[_derivatives.Length * 2];
            for (var i = 0; i < _derivatives.Length; i++)
            {
                var g = _derivatives[i]();
                values[i * 2] = g;
                values[i * 2 + 1] = -g;
            }
            _elements.Add(values);
        }
    }
}
