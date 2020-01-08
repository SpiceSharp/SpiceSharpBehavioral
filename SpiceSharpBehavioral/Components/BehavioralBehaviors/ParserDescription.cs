using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Derivatives;
using System;
using SpiceSharpBehavioral.Components.Parsers;
using SpiceSharp.Attributes;

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
        /// Gets or sets the fudge factor.
        /// </summary>
        /// <value>
        /// The fudge factor.
        /// </value>
        [ParameterName("fudge"), ParameterInfo("The fudging factor for aiding convergence")]
        public GivenParameter<Func<double>> FudgeFactor { get; } = new GivenParameter<Func<double>>();

        /// <summary>
        /// Creates a parser using the current description.
        /// </summary>
        /// <returns>
        /// The parser.
        /// </returns>
        public IParser<IDerivatives<Variable, Func<double>>> Create(BehavioralBindingContext context)
            => new Parser<IDerivatives<Variable, Func<double>>>(new ParserParameters(context));
    }
}
