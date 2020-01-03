using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Describes arithmetic operations on a specified type.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface IArithmeticOperator<T> : IParameterSet
    {
        /// <summary>
        /// Negates the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The arithmetic negative -(input).
        /// </returns>
        T Negate(T input);

        /// <summary>
        /// Increments the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The arithmetic increment (input) + unity
        /// </returns>
        T Increment(T input);

        /// <summary>
        /// Decrements the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The arithmetic decrement (input) - unity
        /// </returns>
        T Decrement(T input);

        /// <summary>
        /// Raises a base to an exponent.
        /// </summary>
        /// <param name="base">The base value.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>
        /// The power (base) ^ (exponent).
        /// </returns>
        T Pow(T @base, T exponent);

        /// <summary>
        /// Raises the base to an integer (fixed) exponent.
        /// </summary>
        /// <param name="base">The base.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>The power (base) ^ exponent</returns>
        T Pow(T @base, int exponent);

        /// <summary>
        /// Takes the natural logarithm.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The natural logarithm Ln(value).
        /// </returns>
        T Log(T value);

        /// <summary>
        /// Adds two operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The addition (left) + (right).
        /// </returns>
        T Add(T left, T right);

        /// <summary>
        /// Subtracts the right operand from the left.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The subtraction (left) - (right).
        /// </returns>
        T Subtract(T left, T right);

        /// <summary>
        /// Multiplies the operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The multiplication (left) * (right).
        /// </returns>
        T Multiply(T left, T right);

        /// <summary>
        /// Divides the operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The division (left) / (right).
        /// </returns>
        T Divide(T left, T right);

        /// <summary>
        /// Takes the modulo.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// the modulo (left) % (right).
        /// </returns>
        T Modulo(T left, T right);
    }
}
