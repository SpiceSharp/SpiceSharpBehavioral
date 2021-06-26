using SpiceSharp.Behaviors;
using SpiceSharp.Entities;

namespace SpiceSharp.Components.LaplaceBehaviors
{
    /// <summary>
    /// A generic biasing behavior for Laplace components.
    /// </summary>
    public abstract class Biasing : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Gets the parameters.
        /// </summary>
        protected Parameters Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="context">The binding context.</param>
        protected Biasing(IBindingContext context)
            : base(context)
        {
            Parameters = context.GetParameterSet<Parameters>();
        }

        /// <summary>
        /// Loads the solver and RHS-vector.
        /// </summary>
        /// <remarks>
        /// Since a transfer function is linear, there is no RHS value.
        /// </remarks>
        /// <param name="dydu">The derivative with respect to the input.</param>
        protected abstract void Load(double dydu);

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            double dydu = Parameters.Numerator[0] / Parameters.Denominator[0];
            Load(dydu);
        }
    }
}
