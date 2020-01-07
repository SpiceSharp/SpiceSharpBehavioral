using SpiceSharp;
using SpiceSharp.Attributes;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    /// <summary>
    /// The base parameters for the behavioral current source.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>
        /// The expression.
        /// </value>
        [ParameterName("expression"), ParameterName("expr"), ParameterName("e"), ParameterInfo("The expression that describes the behavioral source")]
        public string Expression { get; set; }
    }
}
