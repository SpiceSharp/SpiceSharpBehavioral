using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;
using System;

namespace SpiceSharpBehavioral.Components.Parsers.DoubleFunc
{
    /// <summary>
    /// Arithmetic operators.
    /// </summary>
    /// <seealso cref="SpiceSharpBehavioral.Parsers.DoubleFunc.ArithmeticOperator" />
    public class ArithmeticOperator : SpiceSharpBehavioral.Parsers.DoubleFunc.ArithmeticOperator
    {
        private readonly Func<double> _fudgeFactor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArithmeticOperator"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ArithmeticOperator(BehavioralBindingContext context)
        {
            if (context.TryGetSimulationParameterSet(out BiasingParameters bp))
                _fudgeFactor = () => bp.Gmin * 1e-20;
            else
                _fudgeFactor = () => 1e-20;
        }

        /// <summary>
        /// Divides the operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The division (left) / (right).
        /// </returns>
        public override Func<double> Divide(Func<double> left, Func<double> right)
        {
            return () =>
            {
                var b = right();
                if (b >= 0)
                    b += _fudgeFactor();
                else
                    b -= _fudgeFactor();
                if (double.IsPositiveInfinity(b))
                    return double.PositiveInfinity;
                return left() / b;
            };
        }

        /// <summary>
        /// Raises a base to an exponent.
        /// </summary>
        /// <param name="base">The base value.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>
        /// The power (base) ^ (exponent).
        /// </returns>
        public override Func<double> Pow(Func<double> @base, Func<double> exponent)
        {
            return () => Math.Pow(Math.Abs(@base()), exponent());
        }

        /// <summary>
        /// Raises the base to an integer (fixed) exponent.
        /// </summary>
        /// <param name="base">The base.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>
        /// The power (base) ^ exponent
        /// </returns>
        public override Func<double> Pow(Func<double> @base, int exponent)
        {
            return exponent switch
            {
                0 => () => 1.0,
                1 => () => Math.Abs(@base()),
                2 => () => { var x = @base(); return x * x; },
                _ => () => Math.Pow(Math.Abs(@base()), exponent),
            };
        }
    }
}
