﻿using SpiceSharp;
using SpiceSharp.Attributes;
using SpiceSharpBehavioral.Parsers.Double;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Parser parameters for parsing expressions for doubles.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IParserParameters{T}" />
    public class DoubleParserParameters : Parameterized, 
        IParameterSet, 
        IParserParameters<double>,
        IParameterized<ArithmeticOperator>,
        IParameterized<ConditionalOperator>,
        IParameterized<RelationalOperator>,
        IParameterized<ValueFactory>
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
        ArithmeticOperator IParameterized<ArithmeticOperator>.Parameters => Arithmetic;

        /// <summary>
        /// Gets the conditional operators.
        /// </summary>
        /// <value>
        /// The conditional operators.
        /// </value>
        [ParameterName("conditional"), ParameterInfo("The conditional operators")]
        public ConditionalOperator Conditional { get; } = new ConditionalOperator();
        IConditionalOperator<double> IParserParameters<double>.Conditional => Conditional;
        ConditionalOperator IParameterized<ConditionalOperator>.Parameters => Conditional;

        /// <summary>
        /// Gets the relational operators.
        /// </summary>
        /// <value>
        /// The relational operators.
        /// </value>
        [ParameterName("relational"), ParameterInfo("The relational operators")]
        public RelationalOperator Relational { get; } = new RelationalOperator();
        IRelationalOperator<double> IParserParameters<double>.Relational => Relational;
        RelationalOperator IParameterized<RelationalOperator>.Parameters => Relational;

        /// <summary>
        /// Gets the value factory.
        /// </summary>
        /// <value>
        /// The value factory.
        /// </value>
        [ParameterName("valuefactory"), ParameterInfo("The value factory")]
        public ValueFactory ValueFactory { get; } = new ValueFactory();

        IValueFactory<double> IParserParameters<double>.ValueFactory => ValueFactory;
        ValueFactory IParameterized<ValueFactory>.Parameters => ValueFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleParserParameters"/> class.
        /// </summary>
        public DoubleParserParameters()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleParserParameters"/> class.
        /// </summary>
        /// <param name="arithmetic">The arithmetic.</param>
        /// <param name="conditional">The conditional.</param>
        /// <param name="relational">The relational.</param>
        /// <param name="valueFactory">The value factory.</param>
        protected DoubleParserParameters(
            ArithmeticOperator arithmetic,
            ConditionalOperator conditional,
            RelationalOperator relational,
            ValueFactory valueFactory)
        {
            Arithmetic = arithmetic.ThrowIfNull(nameof(arithmetic));
            Conditional = conditional.ThrowIfNull(nameof(conditional));
            Relational = relational.ThrowIfNull(nameof(relational));
            ValueFactory = valueFactory.ThrowIfNull(nameof(valueFactory));
        }

        /// <summary>
        /// Clones the parameters.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        ICloneable ICloneable.Clone()
        {
            var clone = new DoubleParserParameters(
                (ArithmeticOperator)((ICloneable)Arithmetic).Clone(),
                (ConditionalOperator)((ICloneable)Conditional).Clone(),
                (RelationalOperator)((ICloneable)Relational).Clone(),
                (ValueFactory)((ICloneable)ValueFactory).Clone()
                );
            return clone;
        }

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        void ICloneable.CopyFrom(ICloneable source)
        {
            var src = (DoubleParserParameters)source;
            ((ICloneable)Arithmetic).CopyFrom(src.Arithmetic);
            ((ICloneable)Conditional).CopyFrom(src.Conditional);
            ((ICloneable)Relational).CopyFrom(src.Relational);
            ((ICloneable)ValueFactory).CopyFrom(src.ValueFactory);
        }
    }
}
