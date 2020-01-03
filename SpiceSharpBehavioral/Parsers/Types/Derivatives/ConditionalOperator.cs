using SpiceSharp;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpBehavioral.Parsers.Derivatives
{
    /// <summary>
    /// Conditional operators implementation for derivatives.
    /// </summary>
    /// <typeparam name="K">The derivative key value.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    /// <seealso cref="IConditionalOperator{T}" />
    public class ConditionalOperator<K, V> : ParameterSet, IConditionalOperator<IDerivatives<K, V>>
    {
        private readonly IParserParameters<V> _parent;
        private readonly IDerivativeFactory<K, V> _factory;

        /// <summary>
        /// Enumerates the combined keys.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The derivative keys.</returns>
        protected IEnumerable<K> CombinedKeys(IDerivatives<K, V> left, IDerivatives<K, V> right)
            => left.Keys.Union(right.Keys).Distinct();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalOperator{K, V}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="factory">The factory.</param>
        public ConditionalOperator(IParserParameters<V> parent, IDerivativeFactory<K, V> factory)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
            _factory = factory.ThrowIfNull(nameof(factory));
        }

        /// <summary>
        /// Applies the logical and (&amp;&amp;) operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The logical and (left) &amp;&amp; (right).
        /// </returns>
        public IDerivatives<K, V> And(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = _factory.Create();
            n.Value = _parent.Conditional.And(left.Value, right.Value);
            return n;
        }

        /// <summary>
        /// The conditional result.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="ifTrue">If true.</param>
        /// <param name="ifFalse">If false.</param>
        /// <returns>
        /// The conditional result (condition) ? (ifTrue) : (ifFalse).
        /// </returns>
        public IDerivatives<K, V> IfThenElse(IDerivatives<K, V> condition, IDerivatives<K, V> ifTrue, IDerivatives<K, V> ifFalse)
        {
            var n = _factory.Create(ifTrue, ifFalse);
            n.Value = _parent.Conditional.IfThenElse(condition.Value, ifTrue.Value, ifFalse.Value);
            foreach (var key in CombinedKeys(ifTrue, ifFalse))
            {
                var hasLeft = ifTrue.TryGetValue(key, out var leftDerivative);
                var hasRight = ifFalse.TryGetValue(key, out var rightDerivative);
                if (hasLeft && hasRight)
                    n[key] = _parent.Conditional.IfThenElse(condition.Value, leftDerivative, rightDerivative);
                else if (hasLeft)
                    n[key] = _parent.Conditional.IfThenElse(condition.Value, leftDerivative, _parent.ValueFactory.CreateValue(0.0));
                else if (hasRight)
                    n[key] = _parent.Conditional.IfThenElse(condition.Value, _parent.ValueFactory.CreateValue(0.0), rightDerivative);
            }
            return n;
        }

        /// <summary>
        /// Applies the logical not (!) operator.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The logical not !(input).
        /// </returns>
        public IDerivatives<K, V> Not(IDerivatives<K, V> input)
        {
            var n = _factory.Create();
            n.Value = _parent.Conditional.Not(input.Value);
            return n;
        }

        /// <summary>
        /// Applies the logical or (||) operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The logical or (left) || (right).
        /// </returns>
        public IDerivatives<K, V> Or(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = _factory.Create();
            n.Value = _parent.Conditional.Or(left.Value, right.Value);
            return n;
        }
    }
}
