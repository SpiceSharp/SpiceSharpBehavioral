using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.LaplaceBehaviors
{
    /// <summary>
    /// Time-dependent behavior for Laplace-based components.
    /// </summary>
    public abstract class Time : Behavior, IBiasingBehavior, ITimeBehavior
    {
        private readonly Parameters _bp;
        private readonly IDerivative[] _dNumerator, _dDenominator;
        private readonly ITimeSimulationState _state;
        private readonly IIntegrationMethod _method;

        /// <summary>
        /// Gets the input value.
        /// </summary>
        protected abstract double Input { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Time"/> class.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public Time(IBindingContext context)
            : base(context)
        {
            _bp = context.GetParameterSet<Parameters>();

            // Create derivatives for the numerator and denominator
            _state = context.GetState<ITimeSimulationState>();
            _method = context.GetState<IIntegrationMethod>();
            if (_bp.Numerator.Length > 1)
            {
                _dNumerator = new IDerivative[_bp.Numerator.Length - 1];
                for (int i = 0; i < _dNumerator.Length; i++)
                    _dNumerator[i] = _method.CreateDerivative();
            }
            else
                _dNumerator = null;
            if (_bp.Denominator.Length > 1)
            {
                _dDenominator = new IDerivative[_bp.Denominator.Length - 1];
                for (int i = 0; i < _dDenominator.Length; i++)
                    _dDenominator[i] = _method.CreateDerivative();
            }
            else
                _dDenominator = null;
        }

        /// <inheritdoc/>
        public void InitializeStates()
        {
            if (_dNumerator != null)
                _dNumerator[0].Value = Input;
            if (_dDenominator != null)
                _dDenominator[0].Value = Input / _bp.Denominator[0] * _bp.Numerator[0];
        }

        /// <summary>
        /// Loads the Jacobian and right-hand side vector.
        /// </summary>
        /// <param name="dydu">The derivative of the output with respect to the input.</param>
        /// <param name="fy">The value of the output.</param>
        protected abstract void Load(double dydu, double fy);

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            double du, dy, fu, fy;
            if (_state.UseDc)
            {
                du = _bp.Numerator[0] / _bp.Denominator[0];
                Load(du, du * Input);
                return;
            }

            du = _bp.Numerator[0];
            fu = _bp.Numerator[0] * Input;
            if (_dNumerator != null)
            {
                // There are derivatives
                _dNumerator[0].Value = Input;
                _dNumerator[0].Derive();
                double g = _method.Slope; // Jacobian for derivative in time
                du += _bp.Numerator[1] * g;
                fu += _bp.Numerator[1] * _dNumerator[0].Derivative;

                for (int i = 1; i < _dNumerator.Length; i++)
                {
                    _dNumerator[i].Value = _dNumerator[i - 1].Derivative;
                    _dNumerator[i].Derive();
                    g *= _method.Slope;
                    du += _bp.Numerator[i + 1] * g;
                    fu += _bp.Numerator[i + 1] * _dNumerator[i - 1].Derivative;
                }
            }

            // The denominator is a bit less straight-forward
            // We need the derivative of the output, but the output
            // is solved by the solver. We need to assume that the
            // output Y = dy*y + b(y_previous)
            // "a" can be calculated relatively easily.
            // The second term is independent of the solution, but to
            // get it, we need a "fake" solution.
            // Unfortunately, this also means we need to derive twice,
            // once for the "fake" solution, and once for the "real"
            // solution.
            dy = _bp.Denominator[0];
            double b = 0.0;
            if (_dDenominator != null)
            {
                // There are derivatives
                double fake_solution = _dDenominator[0].GetPreviousValue(1);
                _dDenominator[0].Value = fake_solution;
                _dDenominator[0].Derive();
                double g = _method.Slope; // Jacobian for derivative in time
                dy += _bp.Denominator[1] * g;
                b += _bp.Denominator[1] * (_dDenominator[0].Derivative - g * fake_solution);
                for (int i = 1; i < _dDenominator.Length; i++)
                {
                    _dDenominator[i].Value = _dDenominator[i - 1].Derivative;
                    _dDenominator[i].Derive();
                    g *= _method.Slope;
                    dy += _bp.Denominator[i + 1] * g;
                    b += _bp.Denominator[i + 1] * (_dDenominator[i].Derivative - g * fake_solution);
                }
            }

            // We should now be able to calculate the actual output
            fy = (fu - b) / dy;

            // Update the actual derivatives and stuff
            if (_dDenominator != null)
            {
                _dDenominator[0].Value = fy;
                _dDenominator[0].Derive();
                for (int i = 1; i < _dDenominator.Length; i++)
                {
                    _dDenominator[i].Value = _dDenominator[i - 1].Derivative;
                    _dDenominator[i].Derive();
                }
            }

            // Apply to jacobian and rhs-vector
            du /= dy;
            Load(du, fy);
        }
    }
}
