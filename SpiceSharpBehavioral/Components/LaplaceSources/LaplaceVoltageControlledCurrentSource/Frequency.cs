using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.LaplaceVoltageControlledCurrentSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="LaplaceVoltageControlledCurrentSource"/>.
    /// </summary>
    public class Frequency : LaplaceBehaviors.Frequency
    {
        private readonly TwoPort<double> _dblVariables;
        private readonly ElementSet<double> _dblElements;
        private readonly TwoPort<Complex> _cplxVariables;
        private readonly ElementSet<Complex> _cplxElements;

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public Frequency(IComponentBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(LaplaceVoltageControlledVoltageSource.PinCount);

            var bstate = context.GetState<IBiasingSimulationState>();
            _dblVariables = new TwoPort<double>(bstate, context);
            var pos = bstate.Map[_dblVariables.Right.Positive];
            var neg = bstate.Map[_dblVariables.Right.Negative];
            var contPos = bstate.Map[_dblVariables.Left.Positive];
            var contNeg = bstate.Map[_dblVariables.Left.Negative];
            _dblElements = new ElementSet<double>(bstate.Solver, new[] {
                new MatrixLocation(pos, contPos),
                new MatrixLocation(pos, contNeg),
                new MatrixLocation(neg, contPos),
                new MatrixLocation(neg, contNeg)
            });

            var cstate = context.GetState<IComplexSimulationState>();
            _cplxVariables = new TwoPort<Complex>(cstate, context);
            pos = cstate.Map[_cplxVariables.Right.Positive];
            neg = cstate.Map[_cplxVariables.Right.Negative];
            contPos = cstate.Map[_cplxVariables.Left.Positive];
            contNeg = cstate.Map[_cplxVariables.Left.Negative];
            _cplxElements = new ElementSet<Complex>(cstate.Solver, new[] {
                new MatrixLocation(pos, contPos),
                new MatrixLocation(pos, contNeg),
                new MatrixLocation(neg, contPos),
                new MatrixLocation(neg, contNeg)
            });
        }

        /// <inheritdoc/>
        protected override void Load(Complex dydu) 
            => _cplxElements.Add(dydu, -dydu, -dydu, dydu);

        /// <inheritdoc/>
        protected override void Load(double dydu)
            => _dblElements.Add(dydu, -dydu, -dydu, dydu);
    }
}
