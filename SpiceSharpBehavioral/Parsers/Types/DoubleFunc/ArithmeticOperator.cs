using SpiceSharp;
using System;

namespace SpiceSharpBehavioral.Parsers.DoubleFunc
{
    /// <summary>
    /// Arithmetic implementation for functions that return doubles.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IArithmeticOperator{T}" />
    public class ArithmeticOperator : ParameterSet, IArithmeticOperator<Func<double>>
    {
        /// <summary>
        /// Adds two operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The addition (left) + (right).
        /// </returns>
        public Func<double> Add(Func<double> left, Func<double> right)
            => () => left() + right();

        /// <summary>
        /// Decrements the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The arithmetic decrement (input) - unity
        /// </returns>
        public Func<double> Decrement(Func<double> input)
            => () => input() - 1;

        /// <summary>
        /// Divides the operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The division (left) / (right).
        /// </returns>
        public virtual Func<double> Divide(Func<double> left, Func<double> right)
            => () => left() / right();

        /// <summary>
        /// Increments the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The arithmetic increment (input) + unity
        /// </returns>
        public Func<double> Increment(Func<double> input)
            => () => input() + 1;

        /// <summary>
        /// Takes the natural logarithm.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The natural logarithm Ln(value).
        /// </returns>
        public Func<double> Log(Func<double> value)
            => () => Math.Log(value());

        /// <summary>
        /// Takes the modulo.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// the modulo (left) % (right).
        /// </returns>
        public Func<double> Modulo(Func<double> left, Func<double> right)
            => () => left() % right();

        /// <summary>
        /// Multiplies the operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The multiplication (left) * (right).
        /// </returns>
        public Func<double> Multiply(Func<double> left, Func<double> right)
            => () => left() * right();

        /// <summary>
        /// Negates the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The arithmetic negative -(input).
        /// </returns>
        public Func<double> Negate(Func<double> input)
            => () => -input();

        /// <summary>
        /// Raises a base to an exponent.
        /// </summary>
        /// <param name="base">The base value.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>
        /// The power (base) ^ (exponent).
        /// </returns>
        public virtual Func<double> Pow(Func<double> @base, Func<double> exponent)
            => () => Math.Pow(@base(), exponent());

        /// <summary>
        /// Raises the base to an integer (fixed) exponent.
        /// </summary>
        /// <param name="base">The base.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>
        /// The power (base) ^ exponent.
        /// </returns>
        public virtual Func<double> Pow(Func<double> @base, int exponent)
        {
            if (exponent == 0)
                return () => 1.0;
            if (exponent == 1)
                return @base;
            if (exponent == 2)
                return () =>
                {
                    var x = @base();
                    return x * x;
                };
            return () => Math.Pow(@base(), exponent);
        }

        /// <summary>
        /// Subtracts the right operand from the left.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The subtraction (left) - (right).
        /// </returns>
        public Func<double> Subtract(Func<double> left, Func<double> right)
            => () => left() - right();
    }
}
