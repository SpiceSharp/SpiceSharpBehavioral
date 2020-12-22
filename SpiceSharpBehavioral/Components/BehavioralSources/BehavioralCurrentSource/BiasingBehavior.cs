using SpiceSharp.Behaviors;
using SpiceSharp.ParameterSets;
using System;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharp.Attributes;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Builders.Functions;

namespace SpiceSharp.Components.BehavioralCurrentSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="BehavioralCurrentSource"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IBiasingBehavior" />
    [BehaviorFor(typeof(BehavioralCurrentSource), typeof(IBiasingBehavior))]
    public class BiasingBehavior : Behavior,
        IBiasingBehavior
    {
        private readonly OnePort<double> _variables;
        private readonly ElementSet<double> _elements;
        private readonly Func<double> _value;
        private readonly Func<double>[] _derivatives;
        private readonly IVariable<double>[] _derivativeVariables;

        /// <summary>
        /// Gets the variables that are associated with each variable node.
        /// </summary>
        protected Dictionary<VariableNode, IVariable<double>> DerivativeVariables { get; }

        /// <summary>
        /// The function that computes the value.
        /// </summary>
        protected readonly Node Function;

        /// <summary>
        /// The functions that compute the derivatives.
        /// </summary>
        protected readonly Dictionary<VariableNode, Node> Derivatives;

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("The instantaneous current")]
        public double Current { get; private set; }

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        /// <value>
        /// The voltage.
        /// </value>
        [ParameterName("v"), ParameterInfo("The instantaneous voltage")]
        public double Voltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public BiasingBehavior(BehavioralBindingContext context)
            : base(context)
        {
            var bp = context.GetParameterSet<Parameters>();
            var state = context.GetState<IBiasingSimulationState>();
            _variables = new OnePort<double>(
                state.GetSharedVariable(context.Nodes[0]),
                state.GetSharedVariable(context.Nodes[1]));

            // Let's build the derivative functions and get their matrix locations/rhs locations
            Function = bp.Function;
            Derivatives = context.CreateDerivatives(Function);
            DerivativeVariables = Derivatives.Keys.ToDictionary(d => d, d => context.MapNode(state, d), Derivatives.Comparer);
            _derivatives = new Func<double>[Derivatives.Count];
            _derivativeVariables = new IVariable<double>[Derivatives.Count];
            var builder = new RealFunctionBuilder();
            builder.VariableFound += (sender, args) =>
            {
                if (args.Variable == null && DerivativeVariables.TryGetValue(args.Node, out var variable))
                    args.Variable = variable;
            };
            bp.RegisterBuilder(context, builder);
            var matLocs = new MatrixLocation[Derivatives.Count * 2];
            var rhsLocs = _variables.GetRhsIndices(state.Map);
            int index = 0;
            foreach (var pair in Derivatives)
            {
                _derivatives[index] = builder.Build(pair.Value);
                var variable = DerivativeVariables[pair.Key];
                _derivativeVariables[index] = variable;
                matLocs[index * 2] = new MatrixLocation(rhsLocs[0], state.Map[variable]);
                matLocs[index * 2 + 1] = new MatrixLocation(rhsLocs[1], state.Map[variable]);
                index++;
            }
            _value = builder.Build(Function);

            // Get the matrix elements
            _elements = new ElementSet<double>(state.Solver, matLocs, rhsLocs);
        }

        /// <summary>
        /// Loads the Y-matrix and right hand side vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            double[] values = new double[_derivatives.Length * 2 + 2];
            var total = Current = _value();

            int i;
            for (i = 0; i < _derivatives.Length; i++)
            {
                var df = _derivatives[i]();
                total -= _derivativeVariables[i].Value * df;
                values[i * 2] = df;
                values[i * 2 + 1] = -df;
            }
            values[i * 2] = -total;
            values[i * 2 + 1] = total;
            _elements.Add(values);
        }
    }
}
