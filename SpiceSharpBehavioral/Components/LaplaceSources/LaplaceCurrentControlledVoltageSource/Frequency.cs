using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.LaplaceCurrentControlledVoltageSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="LaplaceCurrentControlledVoltageSource"/>.
    /// </summary>
    public class Frequency : LaplaceBehaviors.Frequency,
        IBranchedBehavior<double>,
        IBranchedBehavior<Complex>
    {
        private readonly ElementSet<double> _dblElements;
        private readonly OnePort<double> _dblVariables;
        private readonly IVariable<double> _dblControl, _dblBranch;
        private readonly ElementSet<Complex> _cplxElements;
        private readonly OnePort<Complex> _cplxVariables;
        private readonly IVariable<Complex> _cplxControl, _cplxBranch;

        /// <inheritdoc/>
        IVariable<double> IBranchedBehavior<double>.Branch => _dblBranch;

        /// <inheritdoc/>
        IVariable<Complex> IBranchedBehavior<Complex>.Branch => _cplxBranch;

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public Frequency(ICurrentControlledBindingContext context)
            : base(context)
        {
            var bstate = context.GetState<IBiasingSimulationState>();
            _dblVariables = new OnePort<double>(bstate, context);
            _dblControl = context.ControlBehaviors.GetValue<IBranchedBehavior<double>>().Branch;
            _dblBranch = bstate.CreatePrivateVariable("branch", Units.Ampere);
            int pos = bstate.Map[_dblVariables.Positive];
            int neg = bstate.Map[_dblVariables.Negative];
            int cbr = bstate.Map[_dblControl];
            int br = bstate.Map[_dblBranch];
            _dblElements = new ElementSet<double>(bstate.Solver,
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br),
                new MatrixLocation(br, pos),
                new MatrixLocation(br, neg),
                new MatrixLocation(br, cbr));

            var cstate = context.GetState<IComplexSimulationState>();
            _cplxVariables = new OnePort<Complex>(cstate, context);
            _cplxControl = context.ControlBehaviors.GetValue<IBranchedBehavior<Complex>>().Branch;
            _cplxBranch = cstate.CreatePrivateVariable("branch", Units.Ampere);
            pos = cstate.Map[_cplxVariables.Positive];
            neg = cstate.Map[_cplxVariables.Negative];
            cbr = cstate.Map[_cplxControl];
            br = cstate.Map[_cplxBranch];
            _cplxElements = new ElementSet<Complex>(cstate.Solver,
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br),
                new MatrixLocation(br, pos),
                new MatrixLocation(br, neg),
                new MatrixLocation(br, cbr));
        }

        /// <inheritdoc/>
        protected override void Load(double dydu)
        {
            _dblElements.Add(1, -1, 1, -1, -dydu);
        }

        /// <inheritdoc/>
        protected override void Load(Complex dydu)
        {
            _cplxElements.Add(1, -1, 1, -1, -dydu);
        }
    }
}
