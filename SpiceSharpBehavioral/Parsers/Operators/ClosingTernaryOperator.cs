namespace SpiceSharpBehavioral.Parsers.Operators
{
    /// <summary>
    /// Operator for closing the ternary condition.
    /// </summary>
    /// <seealso cref="SpiceSharpBehavioral.Operator" />
    public class ClosingTernaryOperator : Operator
    {
        /// <summary>
        /// Determines whether the operator has precedence over another operator.
        /// </summary>
        /// <param name="other">The other operator.</param>
        /// <returns>
        ///   <c>true</c> if this operator has precedence over the other operator; otherwise, <c>false</c>.
        /// </returns>
        public override bool HasPrecedenceOver(Operator other)
        {
            // Cases where we want the other operator to be executed first
            if (!(other is TernaryOperator to))
                return false;
            if (to.Closed)
                return false;

            // This closing ternary operator closes the ternary operator
            to.Close();
            return true;
        }

        /// <summary>
        /// Determines whether the operator allows precedence of another operator.
        /// </summary>
        /// <param name="other">The other operator.</param>
        /// <returns>
        ///   <c>true</c> if this operator allows precedence from another operator; otherwise, <c>false</c>.
        /// </returns>
        public override bool AllowPrecedence(Operator other)
        {
            // Should be removed manually
            return false;
        }
    }
}
