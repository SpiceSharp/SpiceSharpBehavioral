using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.LaplaceCurrentControlledVoltageSourceBehaviors
{
    /// <summary>
    /// Time behavior for a <see cref="LaplaceCurrentControlledVoltageSource"/>.
    /// </summary>
    public class Time : LaplaceBehaviors.Time
    {
        private readonly OnePort<double> _variables;
        private readonly ElementSet<double> _elements;
        private readonly IVariable<double> _control;

        /// <inheritdoc/>
        protected override double Input => _control.Value;

        /// <inheritdoc/>
        public IVariable<double> Branch { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Time"/> class.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public Time(ICurrentControlledBindingContext context)
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
                new[] {
                    new MatrixLocation(pos, br),
                    new MatrixLocation(neg, br),
                    new MatrixLocation(br, pos),
                    new MatrixLocation(br, neg),
                    new MatrixLocation(br, cbr)
                },
                new[] { br });
        }

        /// <inheritdoc/>
        protected override void Load(double dydu, double fy)
        {
            fy -= dydu * _control.Value;
            _elements.Add(1, -1, 1, -1, -dydu, fy);
        }
    }
}
