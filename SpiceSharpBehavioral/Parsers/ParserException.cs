using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A parser exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class ParserException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParserException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="input">The input.</param>
        /// <param name="index">The index.</param>
        public ParserException(string message, string input, int index)
            : base(
                index >= input.Length ?
                    $"{message} after parsing" :
                    $"{message} at character {index} ('{input[index]}')")
        {
        }
    }
}
