using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.LaplaceVoltageControlledVoltageSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="LaplaceVoltageControlledVoltageSource"/>.
    /// </summary>
    public class Frequency : LaplaceBehaviors.Frequency,
        IBranchedBehavior<double>,
        IBranchedBehavior<Complex>
    {
        private readonly TwoPort<double> _dblVariables;
        private readonly ElementSet<double> _dblElements;
        private readonly TwoPort<Complex> _cplxVariables;
        private readonly ElementSet<Complex> _cplxElements;
        private readonly IVariable<double> _dblBranch;
        private readonly IVariable<Complex> _cplxBranch;

        /// <inheritdoc/>
        IVariable<double> IBranchedBehavior<double>.Branch => _dblBranch;

        /// <inheritdoc/>
        IVariable<Complex> IBranchedBehavior<Complex>.Branch => _cplxBranch;

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public Frequency(IComponentBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(4);

            var state = context.GetState<IBiasingSimulationState>();
            _dblVariables = new TwoPort<double>(state, context);
            _dblBranch = state.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);
            var pos = state.Map[_dblVariables.Right.Positive];
            var neg = state.Map[_dblVariables.Right.Negative];
            var contPos = state.Map[_dblVariables.Left.Positive];
            var contNeg = state.Map[_dblVariables.Left.Negative];
            var br = state.Map[_dblBranch];
            _dblElements = new ElementSet<double>(state.Solver,
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br),
                new MatrixLocation(br, pos),
                new MatrixLocation(br, neg),
                new MatrixLocation(br, contPos),
                new MatrixLocation(br, contNeg));

            var cstate = context.GetState<IComplexSimulationState>();
            _cplxVariables = new TwoPort<Complex>(cstate, context);
            _cplxBranch = cstate.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);
            pos = cstate.Map[_cplxVariables.Right.Positive];
            neg = cstate.Map[_cplxVariables.Right.Negative];
            contPos = cstate.Map[_cplxVariables.Left.Positive];
            contNeg = cstate.Map[_cplxVariables.Left.Negative];
            br = cstate.Map[_cplxBranch];
            _cplxElements = new ElementSet<Complex>(cstate.Solver,
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br),
                new MatrixLocation(br, pos),
                new MatrixLocation(br, neg),
                new MatrixLocation(br, contPos),
                new MatrixLocation(br, contNeg));
        }

        /// <inheritdoc/>
        protected override void Load(double dydu)
            => _dblElements.Add(1, -1, 1, -1, -dydu, dydu);

        /// <inheritdoc/>
        protected override void Load(Complex dydu)
            => _cplxElements.Add(1, -1, 1, -1, -dydu, dydu);
    }
}
