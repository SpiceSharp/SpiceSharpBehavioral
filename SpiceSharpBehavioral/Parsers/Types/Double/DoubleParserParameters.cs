using SpiceSharp;
using SpiceSharp.Attributes;
using System;
using SpiceSharpBehavioral.Parsers.Double;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Parser parameters for parsing expressions for doubles.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IParserParameters{T}" />
    public class DoubleParserParameters : ParameterSet, IParserParameters<double>
    {
        /// <summary>
        /// Gets the arithmetic operators.
        /// </summary>
        /// <value>
        /// The arithmetic operators.
        /// </value>
        [ParameterName("arithmetic"), ParameterInfo("The arithmetic operators")]
        public ArithmeticOperator Arithmetic { get; } = new ArithmeticOperator();
        IArithmeticOperator<double> IParserParameters<double>.Arithmetic => Arithmetic;

        /// <summary>
        /// Gets the conditional operators.
        /// </summary>
        /// <value>
        /// The conditional operators.
        /// </value>
        [ParameterName("conditional"), ParameterInfo("The conditional operators")]
        public ConditionalOperator Conditional { get; } = new ConditionalOperator();
        IConditionalOperator<double> IParserParameters<double>.Conditional => Conditional;

        /// <summary>
        /// Gets the relational operators.
        /// </summary>
        /// <value>
        /// The relational operators.
        /// </value>
        [ParameterName("relational"), ParameterInfo("The relational operators")]
        public RelationalOperator Relational { get; } = new RelationalOperator();
        IRelationalOperator<double> IParserParameters<double>.Relational => Relational;

        /// <summary>
        /// Gets the value factory.
        /// </summary>
        /// <value>
        /// The value factory.
        /// </value>
        [ParameterName("valuefactory"), ParameterInfo("The value factory")]
        public ValueFactory ValueFactory { get; } = new ValueFactory();

        IValueFactory<double> IParserParameters<double>.ValueFactory => ValueFactory;
    }
}
