using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A standard reader for text.
    /// </summary>
    /// <seealso cref="IReader" />
    public class Reader : IReader
    {
        private readonly string _input;

        /// <summary>
        /// Gets the current index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the character count.
        /// </summary>
        /// <value>
        /// The character count.
        /// </value>
        public int Count { get; }

        /// <summary>
        /// Gets the last read character.
        /// </summary>
        /// <value>
        /// The last character.
        /// </value>
        public char Last { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reader"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        public Reader(string input)
        {
            _input = input.ThrowIfNull(nameof(input));
            Index = 0;
            Last = '\0';
            Count = input.Length;
        }

        /// <summary>
        /// Peeks a character without consuming it.
        /// </summary>
        /// <returns>
        /// The character.
        /// </returns>
        public char Peek()
        {
            if (Index >= Count)
                return '\0';
            return _input[Index];
        }

        /// <summary>
        /// Reads a character.
        /// </summary>
        /// <returns>
        /// The character.
        /// </returns>
        public char Read()
        {
            if (Index >= Count)
                return '\0';
            Last = _input[Index];
            Index++;
            return Last;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="string"/> to <see cref="Reader"/>.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Reader(string input)
        {
            return new Reader(input);
        }
    }
}
