using SpiceSharp;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;
using SpiceSharpBehavioral.Parsers;
using System;

namespace SpiceSharpBehavioral.Components.Parsers.DoubleFunc
{
    /// <summary>
    /// Parser parameters for components.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IParserParameters{T}" />
    public class ParserParameters : ParameterSet, IParserParameters<Func<double>>
    {
        /// <summary>
        /// Gets the arithmetic operators.
        /// </summary>
        /// <value>
        /// The arithmetic operators.
        /// </value>
        public IArithmeticOperator<Func<double>> Arithmetic { get; }

        /// <summary>
        /// Gets the conditional operators.
        /// </summary>
        /// <value>
        /// The conditional operators.
        /// </value>
        public IConditionalOperator<Func<double>> Conditional { get; }

        /// <summary>
        /// Gets the relational operators.
        /// </summary>
        /// <value>
        /// The relational operators.
        /// </value>
        public IRelationalOperator<Func<double>> Relational { get; }

        /// <summary>
        /// Gets the value factory.
        /// </summary>
        /// <value>
        /// The value factory.
        /// </value>
        public IValueFactory<Func<double>> ValueFactory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserParameters"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ParserParameters(BehavioralBindingContext context)
        {
            Arithmetic = new ArithmeticOperator(context);
            Conditional = new SpiceSharpBehavioral.Parsers.DoubleFunc.ConditionalOperator();
            Relational = new SpiceSharpBehavioral.Parsers.DoubleFunc.RelationalOperator();
            ValueFactory = new ValueFactory(context);
        }
    }
}
