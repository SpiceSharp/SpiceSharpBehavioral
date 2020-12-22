using SpiceSharp.Algebra;
using SpiceSharp.ParameterSets;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using SpiceSharp.Attributes;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Builders.Functions;

namespace SpiceSharp.Components.BehavioralVoltageSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="BehavioralVoltageSource"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IBiasingBehavior" />
    /// <seealso cref="IBranchedBehavior{T}"/>
    [BehaviorFor(typeof(BehavioralVoltageSource), typeof(IBiasingBehavior))]
    public class BiasingBehavior : Behavior,
        IBiasingBehavior,
        IBranchedBehavior<double>
    {
        private readonly OnePort<double> _variables;
        private readonly IVariable<double> _branch;
        private readonly ElementSet<double> _elements, _coreElements;
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
        /// Gets the branch equation variable.
        /// </summary>
        /// <value>
        /// The branch equation variable.
        /// </value>
        IVariable<double> IBranchedBehavior<double>.Branch => _branch;

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        /// <value>
        /// The voltage.
        /// </value>
        [ParameterName("v"), ParameterInfo("The instantaneous voltage")]
        public double Voltage { get; private set; }

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("The instantaneous current")]
        public double Current => _branch.Value;

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
            _branch = state.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);

            // Let's build the derivative functions and get their matrix locations/rhs locations
            Function = bp.Function;
            Derivatives = context.CreateDerivatives(Function);
            DerivativeVariables = Derivatives.Keys.ToDictionary(d => d, d => context.MapNode(state, d, _branch), Derivatives.Comparer);
            var builder = new RealFunctionBuilder();
            builder.VariableFound += (sender, args) =>
            {
                if (args.Variable == null && DerivativeVariables.TryGetValue(args.Node, out var variable))
                    args.Variable = variable;
            };
            bp.RegisterBuilder(context, builder);
            _derivatives = new Func<double>[Derivatives.Count];
            _derivativeVariables = new IVariable<double>[Derivatives.Count];
            var matLocs = new MatrixLocation[Derivatives.Count];
            var rhsLocs = state.Map[_branch];
            int index = 0;
            foreach (var pair in Derivatives)
            {
                _derivatives[index] = builder.Build(pair.Value);
                var variable = DerivativeVariables[pair.Key];
                _derivativeVariables[index] = variable;
                matLocs[index] = new MatrixLocation(rhsLocs, state.Map[variable]);
                index++;
            }
            _value = builder.Build(Function);


            // Get the matrix elements
            _elements = new ElementSet<double>(state.Solver, matLocs);
            int br = state.Map[_branch];
            int pos = state.Map[_variables.Positive];
            int neg = state.Map[_variables.Negative];
            _coreElements = new ElementSet<double>(state.Solver, new[] {
                new MatrixLocation(br, pos),
                new MatrixLocation(br, neg),
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br)
            }, new[] { br });
        }

        /// <summary>
        /// Loads the Y-matrix and right hand side vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            double[] values = new double[_derivatives.Length];
            var total = Voltage = _value();
            for (var i = 0; i < _derivatives.Length; i++)
            {
                var df = _derivatives[i]();
                total -= _derivativeVariables[i].Value * df;
                values[i] = -df;
            }
            _elements.Add(values);
            _coreElements.Add(1.0, -1.0, 1.0, -1.0, total);
        }
    }
}
