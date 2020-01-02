namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Describes conditional operations on a specified type.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface IConditionalOperator<T>
    {
        /// <summary>
        /// Applies the logical not (!) operator.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The logical not !(input).
        /// </returns>
        T Not(T input);

        /// <summary>
        /// Applies the logical or (||) operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The logical or (left) || (right).
        /// </returns>
        T Or(T left, T right);

        /// <summary>
        /// Applies the logical and (&amp;&amp;) operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The logical and (left) &amp;&amp; (right).
        /// </returns>
        T And(T left, T right);

        /// <summary>
        /// The conditional result.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="ifTrue">If true.</param>
        /// <param name="ifFalse">If false.</param>
        /// <returns>
        /// The conditional result (condition) ? (ifTrue) : (ifFalse).
        /// </returns>
        T IfThenElse(T condition, T ifTrue, T ifFalse);
    }
}
