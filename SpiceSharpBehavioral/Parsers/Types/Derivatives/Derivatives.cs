using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.Derivatives
{
    /// <summary>
    /// An implementation of derivatives.
    /// </summary>
    /// <typeparam name="K">The key type.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    /// <seealso cref="IDerivatives{K, V}" />
    public class Derivatives<K, V> : IDerivatives<K, V>
    {
        private readonly Dictionary<K, V> _derivatives = new Dictionary<K, V>();

        /// <summary>
        /// Gets or sets the <see cref="Double"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="Double"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>The derivative value.</returns>
        public V this[K key]
        {
            get
            {
                if (_derivatives.TryGetValue(key, out var value))
                    return value;
                return default;
            }
            set => _derivatives[key] = value;
        }

        /// <summary>
        /// Gets or sets the value of the derivative.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public V Value { get; set; }

        /// <summary>
        /// Gets the keys of the derivatives.
        /// </summary>
        IEnumerable<K> IDerivatives<K, V>.Keys => _derivatives.Keys;

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => _derivatives.GetEnumerator();

        /// <summary>
        /// Tries to get the value of a derivative.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The derivative value.</param>
        /// <returns>
        /// <c>true</c> if the derivative exists; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetValue(K key, out V value)
            => _derivatives.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
