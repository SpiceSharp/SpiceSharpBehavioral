using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.ShuntingYard
{
    /// <summary>
    /// Describes a value and its derivatives.
    /// </summary>
    /// <typeparam name="K">The key type for derivatives.</typeparam>
    /// <typeparam name="V">The value type for derivatives.</typeparam>
    /// <seealso cref="IEnumerable{T}" />
    public interface IDerivatives<K, V> : IEnumerable<KeyValuePair<K, V>>
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        V Value { get; set; }

        /// <summary>
        /// Gets the keys of the derivatives.
        /// </summary>
        /// <value>
        /// The keys.
        /// </value>
        IEnumerable<K> Keys { get; }

        /// <summary>
        /// Gets the values of the derivatives.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        IEnumerable<V> Values { get; }

        /// <summary>
        /// Gets or sets the value of the specified derivative.
        /// </summary>
        /// <value>
        /// The value of the derivative.
        /// </value>
        /// <param name="key">The key of the derivative.</param>
        /// <returns>
        /// The value of the derivative.
        /// </returns>
        V this[K key] { get; set; }

        /// <summary>
        /// Determines whether the derivatives contains a derivative value with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the derivatives contain the key; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsKey(K key);

        /// <summary>
        /// Tries to get a value of a derivative.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        bool TryGetValue(K key, out V value);
    }
}
