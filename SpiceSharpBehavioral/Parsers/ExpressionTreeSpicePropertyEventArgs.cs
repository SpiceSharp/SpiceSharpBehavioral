using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Event arguments for finding spice properties for the <see cref="ExpressionTreeDerivativeParser"/>.
    /// </summary>
    public class ExpressionTreeSpicePropertyEventArgs : SpicePropertyFoundEventArgs<double>
    {
        private static readonly MethodInfo InvokeMethod = typeof(Func<double>).GetTypeInfo().GetMethod("Invoke");

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public Expression Result { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionTreeSpicePropertyEventArgs"/>.
        /// </summary>
        /// <param name="property"></param>
        public ExpressionTreeSpicePropertyEventArgs(SpiceProperty property)
            : base(property)
        {
        }

        /// <summary>
        /// Apply a method as the result.
        /// </summary>
        /// <param name="value">The function that returns the value for the current iteration.</param>
        /// <param name="index">The derivative index to apply to.</param>
        /// <param name="derivative">The value of the derivative to apply to.</param>
        public override void Apply(Func<double> value, int index, double derivative)
        {
            Result = Expression.Call(Expression.Constant(value), InvokeMethod);
            Applied = true;
        }
    }
}
