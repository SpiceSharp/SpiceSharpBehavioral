using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.LaplaceVoltageControlledCurrentSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="LaplaceVoltageControlledCurrentSource"/>.
    /// </summary>
    public class Biasing : LaplaceBehaviors.Biasing,
        IBranchedBehavior<double>
    {
        private readonly TwoPort<double> _variables;
        private readonly ElementSet<double> _elements;

        /// <inheritdoc/>
        public IVariable<double> Branch { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public Biasing(IComponentBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(LaplaceVoltageControlledVoltageSource.PinCount);

            var state = context.GetState<IBiasingSimulationState>();
            _variables = new TwoPort<double>(state, context);
            var pos = state.Map[_variables.Right.Positive];
            var neg = state.Map[_variables.Right.Negative];
            var contPos = state.Map[_variables.Left.Positive];
            var contNeg = state.Map[_variables.Left.Negative];
            _elements = new ElementSet<double>(state.Solver, new[] {
                new MatrixLocation(pos, contPos),
                new MatrixLocation(pos, contNeg),
                new MatrixLocation(neg, contPos),
                new MatrixLocation(neg, contNeg)
            });
        }

        /// <inheritdoc/>
        protected override void Load(double dydu)
        {
            _elements.Add(dydu, -dydu, -dydu, dydu);
        }
    }
}
