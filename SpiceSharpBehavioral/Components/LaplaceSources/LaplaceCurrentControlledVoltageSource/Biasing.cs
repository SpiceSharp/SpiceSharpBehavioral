using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.LaplaceCurrentControlledVoltageSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="LaplaceCurrentControlledVoltageSource"/>.
    /// </summary>
    public class Biasing : LaplaceBehaviors.Biasing, IBranchedBehavior<double>
    {
        private readonly ElementSet<double> _elements;
        private readonly OnePort<double> _variables;
        private readonly IVariable<double> _control;

        /// <inheritdoc/>
        public IVariable<double> Branch { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public Biasing(ICurrentControlledBindingContext context)
            : base(context)
        {
            var state = context.GetState<IBiasingSimulationState>();
            _variables = new OnePort<double>(state, context);
            _control = context.ControlBehaviors.GetValue<IBranchedBehavior<double>>().Branch;
            Branch = state.CreatePrivateVariable("branch", Units.Ampere);
            int pos = state.Map[_variables.Positive];
            int neg = state.Map[_variables.Negative];
            int cbr = state.Map[_control];
            int br = state.Map[Branch];
            _elements = new ElementSet<double>(state.Solver,
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br),
                new MatrixLocation(br, pos),
                new MatrixLocation(br, neg),
                new MatrixLocation(br, cbr));
        }

        /// <inheritdoc/>
        protected override void Load(double dydu)
        {
            _elements.Add(1, -1, 1, -1, -dydu);
        }
    }
}
