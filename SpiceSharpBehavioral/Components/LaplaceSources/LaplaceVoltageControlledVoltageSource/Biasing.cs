using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.LaplaceVoltageControlledVoltageSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="LaplaceVoltageControlledVoltageSource"/>.
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
            context.Nodes.CheckNodes(4);

            var _biasing = context.GetState<IBiasingSimulationState>();
            _variables = new TwoPort<double>(_biasing, context);
            Branch = _biasing.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);
            var pos = _biasing.Map[_variables.Right.Positive];
            var neg = _biasing.Map[_variables.Right.Negative];
            var contPos = _biasing.Map[_variables.Left.Positive];
            var contNeg = _biasing.Map[_variables.Left.Negative];
            var br = _biasing.Map[Branch];

            _elements = new ElementSet<double>(_biasing.Solver,
                new MatrixLocation(pos, br),
                new MatrixLocation(neg, br),
                new MatrixLocation(br, pos),
                new MatrixLocation(br, neg),
                new MatrixLocation(br, contPos),
                new MatrixLocation(br, contNeg));
        }

        /// <inheritdoc/>
        protected override void Load(double dydu)
        {
            _elements.Add(1, -1, 1, -1, -dydu, dydu);
        }
    }
}
