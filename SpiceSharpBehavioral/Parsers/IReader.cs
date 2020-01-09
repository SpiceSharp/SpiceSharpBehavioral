using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A reader.
    /// </summary>
    public interface IReader
    {
        /// <summary>
        /// Gets the current index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        int Index { get; }

        /// <summary>
        /// Gets the character count.
        /// </summary>
        /// <value>
        /// The character count.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Gets the last read character.
        /// </summary>
        /// <value>
        /// The last character.
        /// </value>
        char Last { get; }

        /// <summary>
        /// Reads a character.
        /// </summary>
        /// <returns>
        /// The character.
        /// </returns>
        char Read();

        /// <summary>
        /// Peeks a character without consuming it.
        /// </summary>
        /// <returns>
        /// The character.
        /// </returns>
        char Peek();
    }
}
