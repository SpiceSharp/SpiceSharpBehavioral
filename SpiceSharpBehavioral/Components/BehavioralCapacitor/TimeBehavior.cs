using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.BehavioralCapacitorBehaviors
{
    /// <summary>
    /// Time-dependent behavior for a <see cref="BehavioralCapacitor"/>.
    /// </summary>
    /// <seealso cref="BiasingBehavior"/>
    /// <seealso cref="ITimeBehavior"/>
    [BehaviorFor(typeof(BehavioralCapacitor), typeof(ITimeBehavior), 1)]
    public class TimeBehavior : BiasingBehavior,
        IBiasingBehavior,
        ITimeBehavior
    {
        private readonly ElementSet<double> _elements;
        private readonly ITimeSimulationState _time;
        private readonly IIntegrationMethod _method;
        private readonly IDerivative _qcap;

        /// <summary>
        /// Gets the instantaneous capacitor.
        /// </summary>
        /// <value>The instantaneous capacitor.</value>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("The instantaneous current")]
        public double Current => _qcap.Derivative;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeBehavior"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public TimeBehavior(BehavioralBindingContext context)
            : base(context)
        {
            var state = context.GetState<IBiasingSimulationState>();
            _method = context.GetState<IIntegrationMethod>();
            _time = context.GetState<ITimeSimulationState>();

            var matLocs = new MatrixLocation[Functions.Length * 2];
            var rhsLocs = Variables.GetRhsIndices(state.Map);
            for (var i = 0; i < Functions.Length; i++)
            {
                matLocs[i * 2] = new MatrixLocation(rhsLocs[0], state.Map[Functions[i].Item2]);
                matLocs[i * 2 + 1] = new MatrixLocation(rhsLocs[1], state.Map[Functions[i].Item2]);
            }

            // Get the matrix elements
            _elements = new ElementSet<double>(state.Solver, matLocs, rhsLocs);

            // Create the derivative
            _qcap = _method.CreateDerivative();
        }

        /// <summary>
        /// Initialization.
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
            _qcap.Value = Value();
        }

        /// <summary>
        /// Load behavior.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            if (_time.UseDc)
                return;

            _qcap.Value = Value();
            _qcap.Integrate();
            var current = _qcap.Derivative;

            // _qcap.Derivative is the current as integrated by the current integration method
            double[] values = new double[Functions.Length * 2 + 2];
            int i;
            for (i = 0; i < Functions.Length; i++)
            {
                var g = Functions[i].Item3.Invoke() * _method.Slope;
                values[i * 2] = g;
                values[i * 2 + 1] = -g;
                current -= g * Functions[i].Item2.Value;
            }
            values[i * 2] = -current;
            values[i * 2 + 1] = current;
            _elements.Add(values);
        }
    }
}
