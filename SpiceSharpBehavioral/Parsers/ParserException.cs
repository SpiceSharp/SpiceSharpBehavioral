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
        /// <param name="index">The index.</param>
        /// <param name="invalid">The invalid character.</param>
        public ParserException(string message, int index, char invalid = '\0')
            : base(invalid == '\0' ?
                $"{message} after parsing" :
                $"{message} at character {index} ({invalid})")
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
