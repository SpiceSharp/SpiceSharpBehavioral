using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.LaplaceBehaviors
{
    /// <summary>
    /// Frequency behavior for Laplace-based components.
    /// </summary>
    public abstract class Frequency : Biasing, IFrequencyBehavior
    {
        private readonly IComplexSimulationState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="context">The binding context.</param>
        protected Frequency(IBindingContext context)
            : base(context)
        {
            _state = context.GetState<IComplexSimulationState>();
        }

        /// <inheritdoc/>
        public void InitializeParameters()
        {
        }

        /// <summary>
        /// Loads the Jacobian and right-hand side vector.
        /// </summary>
        /// <param name="dydu">The derivative of the output with respect to the input.</param>
        protected abstract void Load(Complex dydu);

        /// <inheritdoc/>
        void IFrequencyBehavior.Load()
        {
            var s = _state.Laplace;

            // Evaluate the transfer function
            int index = Parameters.Numerator.Length - 1;
            Complex numerator = Parameters.Numerator[index];
            while (index > 0)
            {
                index--;
                numerator = numerator * s + Parameters.Numerator[index];
            }
            index = Parameters.Denominator.Length - 1;
            Complex denominator = Parameters.Denominator[index];
            while (index > 0)
            {
                index--;
                denominator = denominator * s + Parameters.Denominator[index];
            }

            Load(numerator / denominator);
        }
    }
}
