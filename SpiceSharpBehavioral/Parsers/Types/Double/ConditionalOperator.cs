using SpiceSharp;
using System;

namespace SpiceSharpBehavioral.Parsers.Double
{
    /// <summary>
    /// Conditional operators for doubles.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IConditionalOperator{Double}" />
    public class ConditionalOperator : ParameterSet, IConditionalOperator<double>
    {
        /// <summary>
        /// Applies the logical and (&amp;&amp;) operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The logical and (left) &amp;&amp; (right).
        /// </returns>
        public double And(double left, double right) => left.Equals(0.0) || right.Equals(0.0) ? 0.0 : 1.0;

        /// <summary>
        /// The conditional result.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="ifTrue">If true.</param>
        /// <param name="ifFalse">If false.</param>
        /// <returns>
        /// The conditional result (condition) ? (ifTrue) : (ifFalse).
        /// </returns>
        public double IfThenElse(double condition, double ifTrue, double ifFalse) => condition.Equals(0.0) ? ifFalse : ifTrue;

        /// <summary>
        /// Applies the logical not (!) operator.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The logical not !(input).
        /// </returns>
        public double Not(double input) => input.Equals(0.0) ? 1.0 : 0.0;

        /// <summary>
        /// Applies the logical or (||) operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The logical or (left) || (right).
        /// </returns>
        public double Or(double left, double right) => left.Equals(0.0) && right.Equals(0.0) ? 0.0 : 1.0;
    }
}
