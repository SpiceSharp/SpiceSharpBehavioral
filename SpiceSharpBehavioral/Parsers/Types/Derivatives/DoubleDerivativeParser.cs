using SpiceSharpBehavioral.Parsers.Derivatives;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Derivatives.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <seealso cref="Parser{T}" />
    public class DoubleDerivativeParser<K> : Parser<IDerivatives<K, double>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleDerivativeParser{K}"/> class.
        /// </summary>
        public DoubleDerivativeParser()
        {
            var factory = new DoubleDerivativesFactory<K>();
            var arithmetic = new Double.ArithmeticOperator();
            var conditional = new Double.ConditionalOperator();
            var relational = new Double.RelationalOperator();
            var value = new Double.ValueFactory();

            Parameters.Arithmetic = new ArithmeticOperator<K, double>(arithmetic, factory);
            Parameters.Conditional = new ConditionalOperator<K, double>(conditional, factory, 0.0);
            Parameters.Relational = new RelationalOperator<K, double>(relational, factory);
            Parameters.Factory = new ValueFactory<K, double>(value, arithmetic, relational, conditional, factory);
        }
    }
}
