namespace SpiceSharpBehavioral.Parsers.ShuntingYard
{
    /// <summary>
    /// A parser for derivatives.
    /// </summary>
    /// <typeparam name="K">The derivative key type.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    /// <seealso cref="Parser{T}" />
    public class DerivativesParser<K, V> : Parser<IDerivatives<K ,V>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DerivativesParser{K, V}"/> class.
        /// </summary>
        /// <param name="operators">The operators.</param>
        public DerivativesParser(IParserDescription<IDerivatives<K, V>> operators)
            : base(operators)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DerivativesParser{K, V}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public DerivativesParser(IShuntingYardOperators<V> parent)
            : base(new ShuntingYardDescription<IDerivatives<K, V>>(new DerivativesOperators<K, V>(parent)))
        {
        }
    }
}
