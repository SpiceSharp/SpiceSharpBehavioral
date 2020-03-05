using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Describes a lexer for Spice# expressions.
    /// </summary>
    public interface ILexer
    {
        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        TokenType Token { get; }

        /// <summary>
        /// Gets the last token.
        /// </summary>
        /// <value>
        /// The last token.
        /// </value>
        TokenType LastToken { get; }

        /// <summary>
        /// Gets the content of the current token.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        string Content { get; }

        /// <summary>
        /// Reads a token.
        /// </summary>
        void ReadToken();

        /// <summary>
        /// Reads a node.
        /// </summary>
        void ReadNode();
    }
}
