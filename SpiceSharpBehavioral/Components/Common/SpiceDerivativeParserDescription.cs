using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Helper;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    /// <summary>
    /// A description of a derivative parser.
    /// </summary>
    /// <typeparam name="T">The return value type.</typeparam>
    /// <seealso cref="ISpiceDerivativeParserDescription{T}" />
    public class SpiceDerivativeParserDescription : ISpiceDerivativeParserDescription<double>
    {
        /// <summary>
        /// Creates the parser.
        /// </summary>
        /// <returns>
        /// The parser.
        /// </returns>
        public ISpiceDerivativeParser<double> Create()
        {
            var parser = new SimpleDerivativeParser();
            parser.RegisterDefaultFunctions();
            return parser;
        }
    }
}
