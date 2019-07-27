using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Event arguments for finding spice properties in a <see cref="SimpleParser"/>.
    /// </summary>
    public class SimpleSpicePropertyEventArgs : SpicePropertyFoundEventArgs<double>
    {
        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public double Result
        {
            get => _result;
            set
            {
                _result = value;
                Found = true;
            }
        }
        private double _result = double.NaN;

        /// <summary>
        /// Gets whether or not the property has been found. In other words,
        /// if the property <see cref="Result"/> has been specified.
        /// </summary>
        public bool Found { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="SimpleSpicePropertyEventArgs"/> class.
        /// </summary>
        /// <param name="property"></param>
        public SimpleSpicePropertyEventArgs(SpiceProperty property)
            : base(property)
        {
            Found = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <param name="derivative"></param>
        public override void Apply(Func<double> value, int index, double derivative)
        {
            if (value != null)
                Result = value();
        }
    }
}
