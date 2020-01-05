using SpiceSharp;
using SpiceSharp.Attributes;
using SpiceSharpBehavioral.Parsers.Derivatives;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Parser parameters for parsing expressions for doubles.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IParserParameters{T}" />
    public class DerivativesParserParameters<K, V> : Parameterized, IParameterSet,
        IParserParameters<IDerivatives<K, V>>,
        IParameterized<ArithmeticOperator<K, V>>,
        IParameterized<ConditionalOperator<K, V>>,
        IParameterized<RelationalOperator<K, V>>,
        IParameterized<ValueFactory<K, V>>
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        /// <value>
        /// The base parameters.
        /// </value>
        public IParserParameters<V> Base { get; }

        /// <summary>
        /// Gets the factory for creating derivatives.
        /// </summary>
        /// <value>
        /// The factory for creating derivatives.
        /// </value>
        public IDerivativeFactory<K, V> Factory { get; }

        /// <summary>
        /// Gets the arithmetic operators.
        /// </summary>
        /// <value>
        /// The arithmetic operators.
        /// </value>
        [ParameterName("arithmetic"), ParameterInfo("The arithmetic operators")]
        public ArithmeticOperator<K, V> Arithmetic { get; }
        IArithmeticOperator<IDerivatives<K, V>> IParserParameters<IDerivatives<K, V>>.Arithmetic => Arithmetic;
        ArithmeticOperator<K, V> IParameterized<ArithmeticOperator<K, V>>.Parameters => Arithmetic;

        /// <summary>
        /// Gets the conditional operators.
        /// </summary>
        /// <value>
        /// The conditional operators.
        /// </value>
        [ParameterName("conditional"), ParameterInfo("The conditional operators")]
        public ConditionalOperator<K, V> Conditional { get; }
        IConditionalOperator<IDerivatives<K, V>> IParserParameters<IDerivatives<K, V>>.Conditional => Conditional;
        ConditionalOperator<K, V> IParameterized<ConditionalOperator<K, V>>.Parameters => Conditional;

        /// <summary>
        /// Gets the relational operators.
        /// </summary>
        /// <value>
        /// The relational operators.
        /// </value>
        [ParameterName("relational"), ParameterInfo("The relational operators")]
        public RelationalOperator<K, V> Relational { get; }
        IRelationalOperator<IDerivatives<K, V>> IParserParameters<IDerivatives<K, V>>.Relational => Relational;
        RelationalOperator<K, V> IParameterized<RelationalOperator<K, V>>.Parameters => Relational;

        /// <summary>
        /// Gets the value factory.
        /// </summary>
        /// <value>
        /// The value factory.
        /// </value>
        [ParameterName("valuefactory"), ParameterInfo("The value factory")]
        public ValueFactory<K, V> ValueFactory { get; }

        IValueFactory<IDerivatives<K, V>> IParserParameters<IDerivatives<K, V>>.ValueFactory => ValueFactory;
        ValueFactory<K, V> IParameterized<ValueFactory<K, V>>.Parameters => ValueFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DerivativesParserParameters{K, V}"/> class.
        /// </summary>
        /// <param name="parent">The parent parser parameters.</param>
        public DerivativesParserParameters(IParserParameters<V> parent)
        {
            Base = parent.ThrowIfNull(nameof(parent));
            Factory = new DerivativeFactory<K, V>();

            Arithmetic = new ArithmeticOperator<K, V>(parent, Factory);
            Conditional = new ConditionalOperator<K, V>(parent, Factory);
            Relational = new RelationalOperator<K, V>(parent, Factory);
            ValueFactory = new ValueFactory<K, V>(parent, Factory);
        }

        /// <summary>
        /// Clones the parameters.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        ICloneable ICloneable.Clone()
        {
            var clone = new DerivativesParserParameters<K, V>((IParserParameters<V>)Base.Clone());
            ((ICloneable)clone).CopyFrom(this);
            return clone;
        }

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        void ICloneable.CopyFrom(ICloneable source)
        {
            var src = (DerivativesParserParameters<K, V>)source;
            ((ICloneable)Arithmetic).CopyFrom(src.Arithmetic);
            ((ICloneable)Conditional).CopyFrom(src.Conditional);
            ((ICloneable)Relational).CopyFrom(src.Relational);
            ((ICloneable)ValueFactory).CopyFrom(src.ValueFactory);
        }
    }
}
