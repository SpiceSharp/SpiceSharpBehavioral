using System;
using SpiceSharp;
using SpiceSharpBehavioral.Parsers;

namespace SpiceSharpBehavioral.Components.DoubleFunc
{
    /// <summary>
    /// Relational operator implementations for functions of doubles.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IRelationalOperator{T}" />
    public class RelationalOperator : ParameterSet, IRelationalOperator<Func<double>>
    {
        /// <summary>
        /// Gets or sets the absolute tolerance.
        /// </summary>
        /// <value>
        /// The absolute tolerance.
        /// </value>
        public double AbsoluteTolerance { get; set; }

        /// <summary>
        /// Gets or sets the relative tolerance.
        /// </summary>
        /// <value>
        /// The relative tolerance.
        /// </value>
        public double RelativeTolerance { get; set; }

        /// <summary>
        /// Returns a value based on whether or not the operands are equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) == (right).
        /// </returns>
        public Func<double> Equal(Func<double> left, Func<double> right)
        {
            return () =>
            {
                double l = left(), r = right();
                var tol = Math.Max(Math.Abs(l), Math.Abs(r)) * RelativeTolerance + AbsoluteTolerance;
                return Math.Abs(l - r) <= tol ? 1.0 : 0.0;
            };
        }

        /// <summary>
        /// Returns a value based on whether or not the left operand is greater or equal than the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) greater or equal to (right).
        /// </returns>
        public Func<double> GreaterOrEqual(Func<double> left, Func<double> right)
            => () => left() >= right() ? 1.0 : 0.0;

        /// <summary>
        /// Returns a value based on whether or not the left operand is greater than the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) greater or equal to (right).
        /// </returns>
        public Func<double> GreaterThan(Func<double> left, Func<double> right)
            => () => left() > right() ? 1.0 : 0.0;

        /// <summary>
        /// Returns a value based on whether or not the left operand is smaller or equal than the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) less or equal to (right).
        /// </returns>
        public Func<double> LessOrEqual(Func<double> left, Func<double> right)
            => () => left() <= right() ? 1.0 : 0.0;

        /// <summary>
        /// Returns a value based on whether or not the left operand is smaller than the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) less than (right).
        /// </returns>
        public Func<double> LessThan(Func<double> left, Func<double> right)
            => () => left() < right() ? 1.0 : 0.0;

        /// <summary>
        /// Returns a value based on whether or not the operands are equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) != (right).
        /// </returns>
        public Func<double> NotEqual(Func<double> left, Func<double> right)
        {
            return () =>
            {
                double l = left(), r = right();
                var tol = Math.Max(Math.Abs(l), Math.Abs(r)) * RelativeTolerance + AbsoluteTolerance;
                return Math.Abs(l - r) <= tol ? 0.0 : 1.0;
            };
        }
    }
}
