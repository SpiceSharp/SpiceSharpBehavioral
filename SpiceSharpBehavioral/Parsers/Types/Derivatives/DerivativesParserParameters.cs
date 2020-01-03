using SpiceSharp;
using SpiceSharp.Attributes;
using System;
using SpiceSharpBehavioral.Parsers.Derivatives;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Parser parameters for parsing expressions for doubles.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IParserParameters{T}" />
    public class DerivativesParserParameters<K, V> : ParameterSet, IParserParameters<IDerivatives<K, V>>
    {
        /// <summary>
        /// Gets the arithmetic operators.
        /// </summary>
        /// <value>
        /// The arithmetic operators.
        /// </value>
        [ParameterName("arithmetic"), ParameterInfo("The arithmetic operators")]
        public ArithmeticOperator<K, V> Arithmetic { get; }
        IArithmeticOperator<IDerivatives<K, V>> IParserParameters<IDerivatives<K, V>>.Arithmetic => Arithmetic;

        /// <summary>
        /// Gets the conditional operators.
        /// </summary>
        /// <value>
        /// The conditional operators.
        /// </value>
        [ParameterName("conditional"), ParameterInfo("The conditional operators")]
        public ConditionalOperator<K, V> Conditional { get; }
        IConditionalOperator<IDerivatives<K, V>> IParserParameters<IDerivatives<K, V>>.Conditional => Conditional;

        /// <summary>
        /// Gets the relational operators.
        /// </summary>
        /// <value>
        /// The relational operators.
        /// </value>
        [ParameterName("relational"), ParameterInfo("The relational operators")]
        public RelationalOperator<K, V> Relational { get; }
        IRelationalOperator<IDerivatives<K, V>> IParserParameters<IDerivatives<K, V>>.Relational => Relational;

        /// <summary>
        /// Gets the value factory.
        /// </summary>
        /// <value>
        /// The value factory.
        /// </value>
        [ParameterName("valuefactory"), ParameterInfo("The value factory")]
        public ValueFactory<K, V> ValueFactory { get; }

        IValueFactory<IDerivatives<K, V>> IParserParameters<IDerivatives<K, V>>.ValueFactory => ValueFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DerivativesParserParameters{K, V}"/> class.
        /// </summary>
        /// <param name="parent">The parent parser parameters.</param>
        public DerivativesParserParameters(IParserParameters<V> parent)
        {
            var factory = new DerivativeFactory<K, V>();
            Arithmetic = new ArithmeticOperator<K, V>(parent.Arithmetic, factory);
            Conditional = new ConditionalOperator<K, V>(parent.Conditional, factory, parent.ValueFactory.CreateValue(0.0, ""));
            Relational = new RelationalOperator<K, V>(parent.Relational, factory);
            ValueFactory = new ValueFactory<K, V>(parent.ValueFactory, parent.Arithmetic, parent.Relational, parent.Conditional, factory);
        }
    }
}
