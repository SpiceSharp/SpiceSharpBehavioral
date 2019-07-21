namespace SpiceSharpBehavioral
{
    /// <summary>
    /// Operator used by parsers.
    /// </summary>
    public abstract class Operator
    {
        /// <summary>
        /// Determines whether this operator should be executed first
        /// assuming this operator comes after it.
        /// </summary>
        /// <param name="other">The other (previous) operator.</param>
        /// <returns></returns>
        public abstract bool HasPrecedenceOver(Operator other);

        /// <summary>
        /// Allows another operator to cause this
        /// one to execute first even though the other one comes
        /// after it.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public abstract bool AllowPrecedence(Operator other);
    }
}
