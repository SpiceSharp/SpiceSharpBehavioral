using SpiceSharp;
using System;

namespace SpiceSharpBehavioral.Parsers.Double
{
    /// <summary>
    /// Arithmetic implementation for doubles.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IArithmeticOperator{T}" />
    public class ArithmeticOperator : ParameterSet, IArithmeticOperator<double>
    {
        /// <summary>
        /// Adds two operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The addition (left) + (right).
        /// </returns>
        public double Add(double left, double right) => left + right;

        /// <summary>
        /// Subtracts the right operand from the left.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The subtraction (left) - (right).
        /// </returns>
        public double Subtract(double left, double right) => left - right;

        /// <summary>
        /// Multiplies the operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The multiplication (left) * (right).
        /// </returns>
        public double Multiply(double left, double right) => left * right;

        /// <summary>
        /// Divides the operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The division (left) / (right).
        /// </returns>
        public double Divide(double left, double right) => left / right;

        /// <summary>
        /// Takes the modulo.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// the modulo (left) % (right).
        /// </returns>
        public double Modulo(double left, double right) => left % right;

        /// <summary>
        /// Negates the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The arithmetic negative -(input).
        /// </returns>
        public double Negate(double input) => -input;

        /// <summary>
        /// Raises a base to an exponent.
        /// </summary>
        /// <param name="base">The base value.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>
        /// The power (base) ^ (exponent).
        /// </returns>
        public double Pow(double @base, double exponent) => Math.Pow(@base, exponent);

        /// <summary>
        /// Takes the natural logarithm.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The natural logarithm Ln(value).
        /// </returns>
        public double Log(double value) => Math.Log(value);

        /// <summary>
        /// Increments the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The arithmetic increment (input) + unity
        /// </returns>
        public double Increment(double input) => input + 1;

        /// <summary>
        /// Decrements the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The arithmetic decrement (input) - unity
        /// </returns>
        public double Decrement(double input) => input - 1;

        /// <summary>
        /// Raises the base to an integer (fixed) exponent.
        /// </summary>
        /// <param name="base">The base.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>
        /// The power (base) ^ exponent
        /// </returns>
        public double Pow(double @base, int exponent)
        {
            if (exponent == 0)
                return 1.0;
            if (exponent == 1)
                return @base;
            if (exponent == 2)
                return @base * @base;

            // Do fast exponentiation
            if (exponent < 0)
            {
                exponent = -exponent;
                @base = 1.0 / @base;
            }
            var result = 1.0;
            var b = @base;
            while (exponent != 0)
            {
                if ((exponent & 0x01) != 0)
                    result *= b;
                exponent >>= 1;
                b *= @base;
            }
            return result;
        }
    }
}
