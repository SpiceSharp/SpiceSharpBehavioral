using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Describes a parser.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public interface IParser<T>
    {
        /// <summary>
        /// Parses an expression using the specified lexer.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns>
        /// The parse result.
        /// </returns>
        T Parse(ILexer lexer);
    }
}
