using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.LaplaceCurrentControlledCurrentSourceBehaviors
{
    /// <summary>
    /// A frequency behavior for a <see cref="LaplaceCurrentControlledCurrentSource"/>.
    /// </summary>
    public class Frequency : LaplaceBehaviors.Frequency
    {
        private readonly ElementSet<double> _dblElements;
        private readonly ElementSet<Complex> _cplxElements;
        private readonly IVariable<double> _dblControl;
        private readonly IVariable<Complex> _cplxControl;
        private readonly OnePort<double> _dblVariables;
        private readonly OnePort<Complex> _cplxVariables;

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="context"></param>
        public Frequency(ICurrentControlledBindingContext context)
            : base(context)
        {
            // Biasing part
            var bstate = context.GetState<IBiasingSimulationState>();
            _dblVariables = new OnePort<double>(bstate, context);
            _dblControl = context.ControlBehaviors.GetValue<IBranchedBehavior<double>>().Branch;
            int r1 = bstate.Map[_dblVariables.Positive];
            int r2 = bstate.Map[_dblVariables.Negative];
            int rc = bstate.Map[_dblControl];
            _dblElements = new ElementSet<double>(bstate.Solver, new MatrixLocation(r1, rc), new MatrixLocation(r2, rc));

            // Complex part
            var cstate = context.GetState<IComplexSimulationState>();
            _cplxVariables = new OnePort<Complex>(cstate, context);
            _cplxControl = context.ControlBehaviors.GetValue<IBranchedBehavior<Complex>>().Branch;
            r1 = cstate.Map[_cplxVariables.Positive];
            r2 = cstate.Map[_cplxVariables.Negative];
            rc = cstate.Map[_cplxControl];
            _cplxElements = new ElementSet<Complex>(cstate.Solver, new MatrixLocation(r1, rc), new MatrixLocation(r2, rc));
        }

        /// <inheritdoc/>
        protected override void Load(Complex dydu)
        {
            _cplxElements.Add(dydu, -dydu);
        }

        /// <inheritdoc/>
        protected override void Load(double dydu)
        {
            _dblElements.Add(dydu, -dydu);
        }
    }
}
