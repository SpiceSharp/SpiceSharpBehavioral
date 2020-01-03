using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Describes relational operations on a specified type.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface IRelationalOperator<T> : IParameterSet
    {
        /// <summary>
        /// Returns a value based on whether or not the operands are equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) == (right).
        /// </returns>
        T Equal(T left, T right);

        /// <summary>
        /// Returns a value based on whether or not the operands are equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) != (right).
        /// </returns>
        T NotEqual(T left, T right);

        /// <summary>
        /// Returns a value based on whether or not the left operand is smaller than the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) less than (right).
        /// </returns>
        T LessThan(T left, T right);

        /// <summary>
        /// Returns a value based on whether or not the left operand is greater than the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) greater or equal to (right).
        /// </returns>
        T GreaterThan(T left, T right);

        /// <summary>
        /// Returns a value based on whether or not the left operand is smaller or equal than the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) less or equal to (right).
        /// </returns>
        T LessOrEqual(T left, T right);

        /// <summary>
        /// Returns a value based on whether or not the left operand is greater or equal than the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) greater or equal to (right).
        /// </returns>
        T GreaterOrEqual(T left, T right);
    }
}
