namespace SpiceSharpBehavioral.Parsers.Operators
{
    /// <summary>
    /// A left bracket operator '('.
    /// </summary>
    /// <seealso cref="SpiceSharpBehavioral.Operator" />
    public class BracketOperator : Operator
    {
        /// <summary>
        /// Determines whether this operator should be executed first
        /// assuming this operator comes after it.
        /// </summary>
        /// <param name="other">The other (previous) operator.</param>
        /// <returns></returns>
        public override bool HasPrecedenceOver(Operator other)
        {
            // A bracket should just be added on the operator stack.
            return true;
        }

        /// <summary>
        /// Allows another operator to cause this
        /// one to execute first even though the other one comes
        /// after it.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public override bool AllowPrecedence(Operator other)
        {
            // No other operator can pass this operator
            return false;
        }
    }
}
