using SpiceSharp;
using SpiceSharp.Attributes;
using SpiceSharpBehavioral.Parsers;
using System.Collections.Generic;

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
        public ExpressionTreeDerivativeParser Parser { get; set; }

        /// <summary>
        /// Gets or sets the comparer for Spice properties.
        /// </summary>
        public EqualityComparer<string> SpicePropertyComparer
        {
            get => _spicePropertyComparer;
            set => _spicePropertyComparer = value ?? EqualityComparer<string>.Default;
        }
        private EqualityComparer<string> _spicePropertyComparer = EqualityComparer<string>.Default;
    }
}
