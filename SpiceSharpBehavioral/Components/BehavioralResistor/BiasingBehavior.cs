using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Builders.Functions;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Components.BehavioralResistorBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="BehavioralResistor"/>.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IBranchedBehavior{T}"/>
    [BehaviorFor(typeof(BehavioralResistor), typeof(IBiasingBehavior))]
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
        public double Voltage { get; private set; }

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
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
            Function = Node.Multiply(Node.Current(Name), bp.Function);
            Derivatives = context.CreateDerivatives(Function);
            DerivativeVariables = Derivatives.Keys.ToDictionary(d => d, d => context.MapNode(state, d, _branch), Derivatives.Comparer);
            var derivatives = new List<Func<double>>(Derivatives.Count);
            var derivativeVariables = new List<IVariable<double>>(Derivatives.Count);
            var builder = new RealFunctionBuilder();
            builder.VariableFound += (sender, args) =>
            {
                if (args.Variable == null && DerivativeVariables.TryGetValue(args.Node, out var variable))
                    args.Variable = variable;
            };
            bp.RegisterBuilder(context, builder);
            var matLocs = new List<MatrixLocation>(Derivatives.Count);
            var rhsLocs = state.Map[_branch];
            foreach (var pair in Derivatives)
            {
                var variable = DerivativeVariables[pair.Key];
                if (state.Map.Contains(variable))
                {
                    derivatives.Add(builder.Build(pair.Value));
                    derivativeVariables.Add(variable);
                    matLocs.Add(new MatrixLocation(rhsLocs, state.Map[variable]));
                }
            }
            _value = builder.Build(Function);

            // Get the matrix elements
            _derivatives = derivatives.ToArray();
            _derivativeVariables = derivativeVariables.ToArray();
            _elements = new ElementSet<double>(state.Solver, matLocs.ToArray());
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

            int i;
            for (i = 0; i < values.Length; i++)
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
