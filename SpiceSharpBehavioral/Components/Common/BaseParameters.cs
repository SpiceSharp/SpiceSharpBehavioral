using SpiceSharp;
using SpiceSharp.Attributes;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    /// <summary>
    /// The base parameters for a <see cref="BehavioralComponent"/>.
    /// </summary>
    /// <seealso cref="ParameterSet" />
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
        public ISpiceDerivativeParserDescription<double> Parser { get; set; }

        /// <summary>
        /// Gets or sets the comparer for Spice properties.
        /// </summary>
        public EqualityComparer<string> PropertyComparer
        {
            get => _propertyComparer;
            set => _propertyComparer = value ?? EqualityComparer<string>.Default;
        }
        private EqualityComparer<string> _propertyComparer = EqualityComparer<string>.Default;
    }
}
