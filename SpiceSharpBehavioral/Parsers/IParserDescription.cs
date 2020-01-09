namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A description of operators.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public interface IParserDescription<T>
    {
        /// <summary>
        /// Creates a parser state for a specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The parser state.</returns>
        IParserState<T> Create(IReader reader);
    }
}
