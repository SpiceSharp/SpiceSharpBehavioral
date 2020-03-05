using SpiceSharp.Simulations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// Describes a value with its derivatives.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <seealso cref="IEnumerable{T}" />
    public class Derivatives<T> : IEnumerable<KeyValuePair<Variable, T>>
    {
        private readonly Dictionary<Variable, T> _derivatives = new Dictionary<Variable, T>();

        /// <summary>
        /// Gets the number of derivatives.
        /// </summary>
        /// <value>
        /// The number of derivatives.
        /// </value>
        public int Count => _derivatives.Count;

        /// <summary>
        /// Gets the keys of the derivatives.
        /// </summary>
        /// <value>
        /// The keys of the derivatives.
        /// </value>
        public IEnumerable<Variable> Keys => _derivatives.Keys;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value { get; set; }

        /// <summary>
        /// Gets or sets the derivative with the specified key.
        /// </summary>
        /// <value>
        /// The derivative value.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>The derivative value.</returns>
        public T this[Variable key]
        {
            get
            {
                if (_derivatives.TryGetValue(key, out var value))
                    return value;
                return default;
            }
            set
            {
                _derivatives[key] = value;
            }
        }

        /// <summary>
        /// Determines whether this instance contains the derivative key.
        /// </summary>
        /// <param name="key">The derivative key.</param>
        /// <returns>
        ///   <c>true</c> if the derivative key exists; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Variable key) => _derivatives.ContainsKey(key);

        /// <summary>
        /// Tries to get the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the value exists; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetValue(Variable key, out T value) => _derivatives.TryGetValue(key, out value);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<Variable, T>> GetEnumerator() => _derivatives.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Combines the derivatives using another one.
        /// </summary>
        /// <param name="other">The other derivative.</param>
        /// <param name="function">The function.</param>
        public void Combine(Derivatives<T> other, Func<T, T> function)
        {
            foreach (var pair in other)
                _derivatives[pair.Key] = function(pair.Value);
        }

        /// <summary>
        /// Combines the derivatives with two others.
        /// </summary>
        /// <param name="left">The left derivatives.</param>
        /// <param name="right">The right derivatives.</param>
        /// <param name="combine">The function when the two derivatives have a key.</param>
        /// <param name="leftFunc">The function when only the left derivative has a key.</param>
        /// <param name="rightFunc">The function when only the right derivative has a key.</param>
        public void Combine(Derivatives<T> left, Derivatives<T> right, Func<T, T, T> combine, Func<T, T> leftFunc, Func<T, T> rightFunc)
        {
            foreach (var key in left._derivatives.Keys.Union(right._derivatives.Keys).Distinct())
            {
                var hasLeft = left._derivatives.TryGetValue(key, out var leftValue);
                var hasRight = right._derivatives.TryGetValue(key, out var rightValue);
                if (hasLeft && hasRight)
                    _derivatives[key] = combine(leftValue, rightValue);
                else if (hasLeft)
                    _derivatives[key] = leftFunc(leftValue);
                else
                    _derivatives[key] = rightFunc(rightValue);
            }
        }
    }
}
