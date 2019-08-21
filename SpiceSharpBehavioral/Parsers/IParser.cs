namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Describes a class that parses an expression.
    /// </summary>
    /// <typeparam name="T">The return type of the parser.</typeparam>
    public interface IParser<T>
    {
        /// <summary>
        /// Parses an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        T Parse(string expression);
    }
}
