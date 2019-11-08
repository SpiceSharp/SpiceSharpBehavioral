using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Event arguments for finding properties of a <see cref="SimpleDerivativeParser"/>.
    /// </summary>
    public class SimpleDerivativePropertyEventArgs : SpicePropertyFoundEventArgs<double>
    {
        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public DoubleDerivatives Result { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="property">The property.</param>
        public SimpleDerivativePropertyEventArgs(SpiceProperty property)
            : base(property)
        {
            Result = null;
        }

        /// <summary>
        /// Apply a derivative.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <param name="derivative"></param>
        public override void Apply(Func<double> value, int index, double derivative)
        {
            var result = new DoubleDerivatives();

            // Specify actual value
            if (value != null)
                result[0] = value;

            // Specify derivative
            if (index > 0)
                result[index] = () => derivative;
            Result = result;
            Applied = true;
        }
    }
}
