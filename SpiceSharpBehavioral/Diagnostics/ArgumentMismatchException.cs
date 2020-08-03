using SpiceSharp;

namespace SpiceSharpBehavioral.Diagnostics
{
    /// <summary>
    /// An exception thrown when the arguments don't match what is expected.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    public class ArgumentMismatchException : SpiceSharpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentMismatchException"/> class.
        /// </summary>
        /// <param name="expected">The expected number of arguments.</param>
        /// <param name="actual">The actual number of arguments.</param>
        public ArgumentMismatchException(int expected, int actual) 
            : base(Properties.Resources.ArgumentMismatch.FormatString(expected, actual))
        {
        }
    }
}
