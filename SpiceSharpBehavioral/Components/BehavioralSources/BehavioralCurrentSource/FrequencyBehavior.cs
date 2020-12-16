using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
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
        private readonly Complex[] _values;

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
            var state = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(
                state.GetSharedVariable(context.Nodes[0]),
                state.GetSharedVariable(context.Nodes[1]));

            // Build the functions using our variable
            var rhsLocs = _variables.GetRhsIndices(state.Map);
            var matLocs = new MatrixLocation[Functions.Length * 2];
            _values = new Complex[Functions.Length * 2];
            for (var i = 0; i < Functions.Length; i++)
            {
                var variable = context.MapNode(state, Functions[i].Item1);
                matLocs[i * 2] = new MatrixLocation(rhsLocs[0], state.Map[variable]);
                matLocs[i * 2 + 1] = new MatrixLocation(rhsLocs[1], state.Map[variable]);
            }

            // Get the matrix elements
            _elements = new ElementSet<Complex>(state.Solver, matLocs);
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
            for (var i = 0; i < Functions.Length; i++)
            {
                var value = Functions[i].Item3.Invoke();
                _values[i * 2] = value;
                _values[i * 2 + 1] = -value;
            }
        }

        /// <summary>
        /// Loads the Y-matrix and right hand side vector.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            _elements.Add(_values);
        }
    }
}
