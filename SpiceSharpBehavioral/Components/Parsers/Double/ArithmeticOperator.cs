using SpiceSharp;
using SpiceSharp.Attributes;
using SpiceSharp.Components.BehavioralBehaviors;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpBehavioral.Components.Parsers.Double
{
    /// <summary>
    /// Arithmetic operator for behavioral components.
    /// </summary>
    /// <seealso cref="SpiceSharpBehavioral.Parsers.Double.ArithmeticOperator" />
    public class ArithmeticOperator : SpiceSharpBehavioral.Parsers.Double.ArithmeticOperator
    {
        private BehavioralBindingContext _context;

        /// <summary>
        /// Gets or sets the fudge factor.
        /// </summary>
        /// <value>
        /// The fudge factor.
        /// </value>
        [ParameterName("fudge"), ParameterInfo("The fudging factor for aiding convergence")]
        public double FudgeFactor { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFactory"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ArithmeticOperator(BehavioralBindingContext context)
        {
            _context = context.ThrowIfNull(nameof(context));
            if (context.TryGetSimulationParameterSet(out BiasingParameters bp))
                FudgeFactor = bp.Gmin * 1e-20;
            else
                FudgeFactor = 1e-20;
        }

        /// <summary>
        /// Divides the operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The division (left) / (right).
        /// </returns>
        public override double Divide(double left, double right)
        {
            // Stay away from 0
            if (right >= 0)
                right += FudgeFactor;
            else
                right -= FudgeFactor;
            if (right.Equals(0.0))
                return double.PositiveInfinity;
            return left / right;
        }

        /// <summary>
        /// Raises a base to an exponent.
        /// </summary>
        /// <param name="base">The base value.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>
        /// The power (base) ^ (exponent).
        /// </returns>
        public override double Pow(double @base, double exponent)
        {
            return Math.Pow(Math.Abs(@base), exponent);
        }

        /// <summary>
        /// Raises the base to an integer (fixed) exponent.
        /// </summary>
        /// <param name="base">The base.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>
        /// The power (base) ^ exponent
        /// </returns>
        public override double Pow(double @base, int exponent)
        {
            switch (exponent)
            {
                case 0: return 1.0;
                case 1: return Math.Abs(@base);
                case 2: return @base * @base;
                default: return Math.Pow(Math.Abs(@base), exponent);
            }
        }
    }
}
