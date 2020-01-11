using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.ShuntingYard
{
    /// <summary>
    /// A default implementation of derivatives.
    /// </summary>
    /// <typeparam name="K">The derivatives key type.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    /// <seealso cref="Dictionary{K, V}" />
    public class Derivatives<K, V> : Dictionary<K, V>, IDerivatives<K, V>
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public V Value { get; set; }

        IEnumerable<K> IDerivatives<K, V>.Keys => Keys;
        IEnumerable<V> IDerivatives<K, V>.Values => Values;
    }
}
