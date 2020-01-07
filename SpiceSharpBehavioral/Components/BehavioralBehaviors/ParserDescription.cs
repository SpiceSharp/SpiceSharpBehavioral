using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Derivatives;
using System;
using SpiceSharpBehavioral.Components.Parsers;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    /// <summary>
    /// A default description of a parser for components.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IParserDescription" />
    public class ParserDescription : ParameterSet, IParserDescription
    {
        /// <summary>
        /// Creates a parser using the current description.
        /// </summary>
        /// <returns>
        /// The parser.
        /// </returns>
        public IParser<IDerivatives<Variable, Func<double>>> Create()
            => new Parser<IDerivatives<Variable, Func<double>>>(new ParserParameters());
    }
}
