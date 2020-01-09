using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A parser exception
    /// </summary>
    /// <seealso cref="Exception" />
    public class ParserException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParserException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="reader">The reader.</param>
        public ParserException(string message, IReader reader)
            : base(reader.Index >= reader.Count ?
                $"{message} after parsing" :
                $"{message} at character {reader.Index - 1} ('{reader.Last}')")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ParserException(string message)
            : base(message)
        {
        }
    }
}
