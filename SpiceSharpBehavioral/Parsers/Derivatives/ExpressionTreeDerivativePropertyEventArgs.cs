using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Event arguments for finding Spice properties for the <see cref="ExpressionTreeDerivativeParser"/>.
    /// </summary>
    public class ExpressionTreeDerivativePropertyEventArgs : SpicePropertyFoundEventArgs<double>
    {
        private static readonly MethodInfo InvokeMethod = typeof(Func<double>).GetTypeInfo().GetMethod("Invoke");

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public ExpressionTreeDerivatives Result { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="property">The property.</param>
        public ExpressionTreeDerivativePropertyEventArgs(SpiceProperty property)
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
            var result = new ExpressionTreeDerivatives();

            // Specify the actual value
            if (value != null)
                result[0] = Expression.Call(Expression.Constant(value), InvokeMethod);

            // Specify the derivative
            if (index > 0)
                result[index] = Expression.Constant(derivative);
            Result = result;
            Applied = true;
        }
    }
}
