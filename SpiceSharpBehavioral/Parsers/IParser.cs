using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Describes a parser.
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// Parses an expression using the specified lexer.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns>
        /// The parse result.
        /// </returns>
        Node Parse(ILexer lexer);
    }
}
