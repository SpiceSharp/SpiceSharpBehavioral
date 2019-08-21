using SpiceSharp;
using SpiceSharp.Attributes;
using SpiceSharpBehavioral.Parsers;
using System.Collections.Generic;
using SpiceSharpBehavioral.Parsers.Helper;
using SpiceSharp.Circuits;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    /// <summary>
    /// The base parameters for behavioral sources.
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the expression for the behavioral source.
        /// </summary>
        [ParameterName("expr"), ParameterName("expression"), ParameterInfo("The expression that describes the behavioral source")]
        public string Expression { get; set; }

        /// <summary>
        /// Gets or sets the parser used to parse the expression.
        /// </summary>
        [ParameterName("parser"), ParameterInfo("The parser that is used to parse the expression")]
        public ISpiceDerivativeParser<double> Parser { get; set; }

        /// <summary>
        /// Gets or sets the comparer for Spice properties.
        /// </summary>
        public EqualityComparer<string> SpicePropertyComparer
        {
            get => _spicePropertyComparer;
            set => _spicePropertyComparer = value ?? EqualityComparer<string>.Default;
        }
        private EqualityComparer<string> _spicePropertyComparer = EqualityComparer<string>.Default;

        /// <summary>
        /// Gets or sets the instance data used by the component.
        /// </summary>
        /// <remarks>
        /// We need this data during the parsing of expressions to map local nodes
        /// to global nodes.
        /// </remarks>
        public InstanceData Instance { get; set; }

        /// <summary>
        /// Calculate the defaults
        /// </summary>
        public override void CalculateDefaults()
        {
            if (Parser == null)
            {
                var parser = new SimpleDerivativeParser();
                parser.RegisterDefaultFunctions();
                Parser = parser;
            }
        }
    }
}
