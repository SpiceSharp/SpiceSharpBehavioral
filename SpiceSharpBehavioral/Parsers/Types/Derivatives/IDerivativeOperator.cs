namespace SpiceSharpBehavioral.Parsers.Derivatives
{
    /// <summary>
    /// Describes an operator for derivatives.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public interface IDerivativeOperator<K, V>
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        /// <value>
        /// The base.
        /// </value>
        IParserParameters<V> Base { get; }

        /// <summary>
        /// Gets the factory for derivatives.
        /// </summary>
        /// <value>
        /// The factory.
        /// </value>
        IDerivativeFactory<K, V> Factory { get; }
    }
}
