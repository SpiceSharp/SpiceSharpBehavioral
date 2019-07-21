using System;

namespace SpiceSharpBehavioral.Parsers.Operators
{
    /// <summary>
    /// Indicates an argument for a function.
    /// </summary>
    /// <seealso cref="SpiceSharpBehavioral.Parsers.Operators.BracketOperator" />
    public class ArgumentOperator : Operator
    {
        /// <summary>
        /// Determines whether this operator should be executed first
        /// assuming the operator comes after it.
        /// </summary>
        /// <param name="other">The other (previous) operator.</param>
        /// <returns></returns>
        public override bool HasPrecedenceOver(Operator other)
        {
            // We should reduce until function operators.
            return false;
        }

        /// <summary>
        /// Allows another operator to cause this
        /// one to execute first even though the other one comes
        /// after it.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Invalid argument usage</exception>
        public override bool AllowPrecedence(Operator other)
        {
            // If we ever have another operator trying to pass this one,
            // then the parser didn't do its job correctly.
            throw new Exception("Invalid argument usage");
        }
    }
}
