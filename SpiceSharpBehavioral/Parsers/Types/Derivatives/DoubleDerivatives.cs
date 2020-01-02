using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.Derivatives
{
    /// <summary>
    /// An implementation of derivatives for doubles.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <seealso cref="Dictionary{K, V}" />
    /// <seealso cref="IDerivatives{K, V}" />
    public class DoubleDerivatives<K> : IDerivatives<K, double>
    {
        private readonly Dictionary<K, double> _derivatives = new Dictionary<K, double>();

        /// <summary>
        /// Gets or sets the <see cref="Double"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="Double"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>The derivative value.</returns>
        public double this[K key]
        {
            get
            {
                if (_derivatives.TryGetValue(key, out var value))
                    return value;
                return 0.0;
            }
            set => _derivatives[key] = value;
        }

        /// <summary>
        /// Gets or sets the value of the derivative.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public double Value { get; set; }

        /// <summary>
        /// Gets the keys of the derivatives.
        /// </summary>
        IEnumerable<K> IDerivatives<K, double>.Keys => _derivatives.Keys;

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<K, double>> GetEnumerator() => _derivatives.GetEnumerator();

        /// <summary>
        /// Tries to get the value of a derivative.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The derivative value.</param>
        /// <returns>
        /// <c>true</c> if the derivative exists; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetValue(K key, out double value)
            => _derivatives.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// An implementation of a derivative factory for derivatives of doubles.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <seealso cref="IDerivativeFactory{K, V}" />
    public class DoubleDerivativesFactory<K> : IDerivativeFactory<K, double>
    {
        /// <summary>
        /// Creates a derivatives input based on the inputs.
        /// </summary>
        /// <param name="inputs">The inputs that will affect the created derivative.</param>
        /// <returns>
        /// The derivative.
        /// </returns>
        public IDerivatives<K, double> Create(params IDerivatives<K, double>[] inputs)
            => new DoubleDerivatives<K>();
    }
}
