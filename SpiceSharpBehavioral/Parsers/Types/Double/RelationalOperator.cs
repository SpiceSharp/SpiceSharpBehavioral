using System;

namespace SpiceSharpBehavioral.Parsers.Double
{
    /// <summary>
    /// Relational operators for doubles.
    /// </summary>
    /// <seealso cref="IRelationalOperator{Double}" />
    public class RelationalOperator : IRelationalOperator<double>
    {
        /// <summary>
        /// Gets or sets the absolute tolerance for checking equality.
        /// </summary>
        /// <value>
        /// The absolute tolerance for equality.
        /// </value>
        public double AbsoluteTolerance { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the relative tolerance for checking equality.
        /// </summary>
        /// <value>
        /// The relative tolerance for equality.
        /// </value>
        public double RelativeTolerance { get; set; } = 0.0;

        /// <summary>
        /// Returns a value based on whether or not the operands are equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) == (right).
        /// </returns>
        public double Equal(double left, double right)
        {
            var tol = Math.Max(Math.Abs(left), Math.Abs(right)) * RelativeTolerance + AbsoluteTolerance;
            if (Math.Abs(left - right) <= tol)
                return 1.0;
            return 0.0;
        }

        /// <summary>
        /// Returns a value based on whether or not the left operand is greater or equal than the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) greater or equal to (right).
        /// </returns>
        public double GreaterOrEqual(double left, double right) => left >= right ? 1.0 : 0.0;

        /// <summary>
        /// Returns a value based on whether or not the left operand is greater than the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) greater or equal to (right).
        /// </returns>
        public double GreaterThan(double left, double right) => left > right ? 1.0 : 0.0;

        /// <summary>
        /// Returns a value based on whether or not the left operand is smaller or equal than the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) less or equal to (right).
        /// </returns>
        public double LessOrEqual(double left, double right) => left <= right ? 1.0 : 0.0;

        /// <summary>
        /// Returns a value based on whether or not the left operand is smaller than the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) less than (right).
        /// </returns>
        public double LessThan(double left, double right) => left < right ? 1.0 : 0.0;

        /// <summary>
        /// Returns a value based on whether or not the operands are equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) != (right).
        /// </returns>
        public double NotEqual(double left, double right)
        {
            var tol = Math.Max(Math.Abs(left), Math.Abs(right)) * RelativeTolerance + AbsoluteTolerance;
            if (Math.Abs(left - right) <= tol)
                return 0.0;
            return 1.0;
        }
    }
}
