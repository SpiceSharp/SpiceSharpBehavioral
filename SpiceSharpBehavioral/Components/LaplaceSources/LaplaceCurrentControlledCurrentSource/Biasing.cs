using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.LaplaceCurrentControlledCurrentSourceBehaviors
{
    /// <summary>
    /// A biasing behavior for a <see cref="LaplaceCurrentControlledCurrentSource"/>.
    /// </summary>
    public partial class Biasing : LaplaceBehaviors.Biasing
    {
        private readonly ElementSet<double> _elements;
        private readonly OnePort<double> _variables;
        private readonly IVariable<double> _control;

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

            int r1 = state.Map[_variables.Positive];
            int r2 = state.Map[_variables.Negative];
            int rc = state.Map[_control];
            _elements = new ElementSet<double>(state.Solver,
                new[]
                {
                    new MatrixLocation(r1, rc),
                    new MatrixLocation(r2, rc)
                });
        }

        /// <inheritdoc/>
        protected override void Load(double dydu)
        {
            _elements.Add(dydu, -dydu);
        }
    }
}
