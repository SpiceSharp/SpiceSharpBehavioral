namespace SpiceSharpBehavioral.Parsers.Derivatives
{
    /// <summary>
    /// A factory for derivatives.
    /// </summary>
    /// <typeparam name="K">The derivative key.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    public interface IDerivativeFactory<K, V>
    {
        /// <summary>
        /// Creates a derivatives input based on the inputs.
        /// </summary>
        /// <param name="inputs">The inputs that will affect the created derivative.</param>
        /// <returns>
        /// The derivative.
        /// </returns>
        IDerivatives<K, V> Create(params IDerivatives<K, V>[] inputs);
    }
}
