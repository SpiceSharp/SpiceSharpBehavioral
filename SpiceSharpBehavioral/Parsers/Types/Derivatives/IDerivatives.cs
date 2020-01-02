using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.Derivatives
{
    /// <summary>
    /// Describes a parsed expression with its derivatives.
    /// </summary>
    /// <typeparam name="K">The key type.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    /// <seealso cref="IEnumerable{T}" />
    public interface IDerivatives<K, V> : IEnumerable<KeyValuePair<K, V>>
    {
        /// <summary>
        /// Gets or sets the value of the derivative.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        V Value { get; set; }

        /// <summary>
        /// Gets or sets the derivatives with the specified keys.
        /// </summary>
        /// <value>
        /// The value of the derivative.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>The value of the derivative.</returns>
        V this[K key] { get; set; }

        /// <summary>
        /// Gets the derivative keys.
        /// </summary>
        /// <value>
        /// The keys.
        /// </value>
        IEnumerable<K> Keys { get; }

        /// <summary>
        /// Tries to get the value of a derivative.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The derivative value.</param>
        /// <returns>
        /// <c>true</c> if the derivative exists; otherwise, <c>false</c>.
        /// </returns>
        bool TryGetValue(K key, out V value);
    }
}
