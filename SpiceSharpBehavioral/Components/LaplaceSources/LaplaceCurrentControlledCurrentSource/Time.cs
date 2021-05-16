using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.LaplaceCurrentControlledCurrentSourceBehaviors
{
    /// <summary>
    /// The time-dependent behavior for a <see cref="LaplaceCurrentControlledCurrentSource"/>.
    /// </summary>
    public class Time : LaplaceBehaviors.Time
    {
        private readonly OnePort<double> _variables;
        private readonly ElementSet<double> _elements;
        private readonly IVariable<double> _control;

        /// <inheritdoc/>
        protected override double Input => _control.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Time"/> class.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public Time(ICurrentControlledBindingContext context)
            : base(context)
        {
            // Create the variables needed
            var state = context.GetState<IBiasingSimulationState>();
            _control = context.ControlBehaviors.GetValue<IBranchedBehavior<double>>().Branch;
            _variables = new OnePort<double>(state, context);
            int r1 = state.Map[_variables.Positive];
            int r2 = state.Map[_variables.Negative];
            int rc = state.Map[_control];
            _elements = new ElementSet<double>(state.Solver,
                new[] {
                    new MatrixLocation(r1, rc),
                    new MatrixLocation(r2, rc)
                },
                new[] {
                    r1,
                    r2
                });
            
        }

        /// <inheritdoc/>
        protected override void Load(double dydu, double fy)
        {
            fy -= dydu * _control.Value;
            _elements.Add(dydu, -dydu, -fy, fy);
        }
    }
}
