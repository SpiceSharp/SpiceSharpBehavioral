using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

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
        /// The functions that compute the derivatives.
        /// </summary>
        protected Tuple<VariableNode, IVariable<double>, Func<double>>[] Functions;

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
            var df = context.CreateDerivatives(bp.BuilderFactory,
                Node.Multiply(Node.Current(Name), bp.Function),
                _branch);
            _value = df.Item1;
            Functions = df.Item2;
            var matLocs = new MatrixLocation[Functions.Length];
            var rhsLocs = state.Map[_branch];
            for (var i = 0; i < Functions.Length; i++)
                matLocs[i] = new MatrixLocation(rhsLocs, state.Map[Functions[i].Item2]);

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
            double[] values = new double[Functions.Length];
            var total = Voltage = _value();

            int i;
            for (i = 0; i < Functions.Length; i++)
            {
                var df = Functions[i].Item3.Invoke();
                total -= Functions[i].Item2.Value * df;
                values[i] = -df;
            }
            _elements.Add(values);
            _coreElements.Add(1.0, -1.0, 1.0, -1.0, total);
        }
    }
}
