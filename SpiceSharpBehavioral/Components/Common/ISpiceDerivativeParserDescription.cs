using SpiceSharpBehavioral.Parsers;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    /// <summary>
    /// A description for a parser.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public interface ISpiceDerivativeParserDescription<T>
    {
        /// <summary>
        /// Creates the parser.
        /// </summary>
        /// <returns>
        /// The parser.
        /// </returns>
        ISpiceDerivativeParser<T> Create();
    }
}
