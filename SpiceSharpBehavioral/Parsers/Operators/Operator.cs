namespace SpiceSharpBehavioral
{
    /// <summary>
    /// Operator used by parsers.
    /// </summary>
    public abstract class Operator
    {
        /// <summary>
        /// Determines whether the operator has precedence over another operator.
        /// </summary>
        /// <param name="other">The other operator.</param>
        /// <returns>
        ///   <c>true</c> if this operator has precedence over the other operator; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool HasPrecedenceOver(Operator other);

        /// <summary>
        /// Determines whether the operator allows precedence of another operator.
        /// </summary>
        /// <param name="other">The other operator.</param>
        /// <returns>
        ///     <c>true</c> if this operator allows precedence from another operator; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool AllowPrecedence(Operator other);
    }
}
