using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.LaplaceBehaviors
{
    /// <summary>
    /// Base parameters for a Laplace-based component.
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    [GeneratedParameters]
    public partial class Parameters : ParameterSet<Parameters>
    {
        /// <summary>
        /// Gets or sets the numerator of the Laplace source.
        /// </summary>
        /// <value>
        /// The numerator.
        /// </value>
        [ParameterName("num"), ParameterName("numerator"), ParameterInfo("The numerator of the transfer function")]
        public double[] Numerator { get; set; } = new[] { 1.0 };

        /// <summary>
        /// Gets or sets the denominator of the Laplace source.
        /// </summary>
        /// <value>
        /// The denominator.
        /// </value>
        [ParameterName("denom"), ParameterName("denominator"), ParameterInfo("The denominator of the transfer function")]
        public double[] Denominator { get; set; } = new[] { 1.0 };

        /// <summary>
        /// Gets or sets the delay.
        /// </summary>
        /// <value>
        /// The delay.
        /// </value>
        [ParameterName("delay"), ParameterName("td"), ParameterInfo("The delay")]
        public double Delay { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parameters"/> class.
        /// </summary>
        public Parameters()
        {
        }
    }
}
