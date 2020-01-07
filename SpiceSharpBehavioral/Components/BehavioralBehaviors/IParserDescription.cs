using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Derivatives;
using System;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    /// <summary>
    /// Describes an <see cref="IParameterSet"/> that can be used for creating parsers.
    /// </summary>
    /// <seealso cref="IParameterSet" />
    public interface IParserDescription : IParameterSet
    {
        /// <summary>
        /// Creates a parser using the current description.
        /// </summary>
        /// <returns>
        /// The parser.
        /// </returns>
        IParser<IDerivatives<Variable, Func<double>>> Create();
    }
}
