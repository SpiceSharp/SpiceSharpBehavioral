namespace SpiceSharpBehavioral.Parsers.Operators
{
    public class ClosingTernaryOperator : Operator
    {
        /// <summary>
        /// Determines whether this operator should be executed first
        /// assuming this operator comes after it.
        /// </summary>
        /// <param name="other">The other (previous) operator.</param>
        /// <returns></returns>
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
        /// Allows another operator to cause this
        /// one to execute first even though the other one comes
        /// after it.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public override bool AllowPrecedence(Operator other)
        {
            // Should be removed manually
            return false;
        }
    }
}
