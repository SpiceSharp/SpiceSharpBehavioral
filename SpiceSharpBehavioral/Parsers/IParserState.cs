using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Describes operators for a parser.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <seealso cref="IParameterSet" />
    public interface IParserState<T>
    {
        /// <summary>
        /// Reads a token.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        void ParseToken(IReader buffer);

        /// <summary>
        /// Finish up and return the final result.
        /// </summary>
        /// <returns>The final result.</returns>
        T Execute();
    }
}
