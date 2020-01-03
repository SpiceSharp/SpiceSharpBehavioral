namespace SpiceSharpBehavioral.Parsers.Derivatives
{
    /// <summary>
    /// Implementation of an <see cref="IDerivativeFactory{K, V}"/> that creates <see cref="Derivatives{K, V}"/>.
    /// </summary>
    /// <typeparam name="K">The key type.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    /// <seealso cref="IDerivativeFactory{K, V}" />
    public class DerivativeFactory<K, V> : IDerivativeFactory<K, V>
    {
        /// <summary>
        /// Creates a derivatives input based on the inputs.
        /// </summary>
        /// <param name="inputs">The inputs that will affect the created derivative.</param>
        /// <returns>
        /// The derivative.
        /// </returns>
        public IDerivatives<K, V> Create(params IDerivatives<K, V>[] inputs)
            => new Derivatives<K, V>();
    }
}
