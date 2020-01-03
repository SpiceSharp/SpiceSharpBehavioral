using SpiceSharp;
using System.Linq;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.Derivatives
{
    /// <summary>
    /// Arithmetic operation implementation for derivatives.
    /// </summary>
    /// <typeparam name="K">The derivative key.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    /// <seealso cref="IArithmeticOperator{T}" />
    public class ArithmeticOperator<K, V> : ParameterSet, IArithmeticOperator<IDerivatives<K, V>>
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
        /// Initializes a new instance of the <see cref="ArithmeticOperator{K, V}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="factory">The factory.</param>
        public ArithmeticOperator(IParserParameters<V> parent, IDerivativeFactory<K, V> factory)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
            _factory = factory.ThrowIfNull(nameof(factory));
        }

        /// <summary>
        /// Adds two operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The addition (left) + (right).
        /// </returns>
        public IDerivatives<K, V> Add(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = _factory.Create(left, right);
            n.Value = _parent.Arithmetic.Add(left.Value, right.Value);

            // Adding of derivatives
            foreach (var key in CombinedKeys(left, right))
            {
                var hasLeft = left.TryGetValue(key, out var leftDerivative);
                var hasRight = right.TryGetValue(key, out var rightDerivative);
                if (hasLeft && hasRight)
                    n[key] = _parent.Arithmetic.Add(leftDerivative, rightDerivative);
                else if (hasLeft)
                    n[key] = leftDerivative;
                else if (hasRight)
                    n[key] = rightDerivative;
            }
            return n;
        }

        /// <summary>
        /// Subtracts the right operand from the left.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The subtraction (left) - (right).
        /// </returns>
        public IDerivatives<K, V> Subtract(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = _factory.Create(left, right);
            n.Value = _parent.Arithmetic.Subtract(left.Value, right.Value);

            // Adding of derivatives
            foreach (var key in CombinedKeys(left, right))
            {
                var hasLeft = left.TryGetValue(key, out var leftDerivative);
                var hasRight = right.TryGetValue(key, out var rightDerivative);
                if (hasLeft && hasRight)
                    n[key] = _parent.Arithmetic.Subtract(leftDerivative, rightDerivative);
                else if (hasLeft)
                    n[key] = leftDerivative;
                else if (hasRight)
                    n[key] = rightDerivative;
            }
            return n;
        }

        /// <summary>
        /// Divides the operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The division (left) / (right).
        /// </returns>
        public IDerivatives<K, V> Divide(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = _factory.Create(left, right);
            n.Value = _parent.Arithmetic.Divide(left.Value, right.Value);

            // Adding of derivatives
            foreach (var key in CombinedKeys(left, right))
            {
                var hasLeft = left.TryGetValue(key, out var leftDerivative);
                var hasRight = right.TryGetValue(key, out var rightDerivative);
                if (hasLeft && hasRight)
                {
                    n[key] = _parent.Arithmetic.Subtract(_parent.Arithmetic.Divide(leftDerivative, right.Value),
                        _parent.Arithmetic.Divide(
                            _parent.Arithmetic.Multiply(left.Value, rightDerivative),
                            _parent.Arithmetic.Multiply(right.Value, right.Value)));
                }
                else if (hasLeft)
                    n[key] = _parent.Arithmetic.Divide(leftDerivative, right.Value);
                else if (hasRight)
                {
                    n[key] = _parent.Arithmetic.Negate(
                        _parent.Arithmetic.Divide(
                            _parent.Arithmetic.Multiply(left.Value, rightDerivative),
                            _parent.Arithmetic.Multiply(right.Value, right.Value)));
                }
            }
            return n;
        }

        /// <summary>
        /// Takes the modulo.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// the modulo (left) % (right).
        /// </returns>
        public IDerivatives<K, V> Modulo(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = _factory.Create(left, right);
            n.Value = _parent.Arithmetic.Modulo(left.Value, right.Value);
            return n;
        }

        /// <summary>
        /// Multiplies the operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The multiplication (left) * (right).
        /// </returns>
        public IDerivatives<K, V> Multiply(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = _factory.Create(left, right);
            n.Value = _parent.Arithmetic.Multiply(left.Value, right.Value);

            foreach (var key in CombinedKeys(left, right))
            {
                var hasLeft = left.TryGetValue(key, out var leftDerivative);
                var hasRight = right.TryGetValue(key, out var rightDerivative);
                if (hasLeft && hasRight)
                    n[key] = _parent.Arithmetic.Add(_parent.Arithmetic.Multiply(leftDerivative, right.Value), _parent.Arithmetic.Multiply(left.Value, rightDerivative));
                else if (hasLeft)
                    n[key] = _parent.Arithmetic.Multiply(leftDerivative, right.Value);
                else if (hasRight)
                    n[key] = _parent.Arithmetic.Multiply(left.Value, rightDerivative);
            }
            return n;
        }

        /// <summary>
        /// Negates the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The arithmetic negative -(input).
        /// </returns>
        public IDerivatives<K, V> Negate(IDerivatives<K, V> input)
        {
            var n = _factory.Create(input);
            n.Value = _parent.Arithmetic.Negate(input.Value);
            foreach (var pair in input)
                n[pair.Key] = _parent.Arithmetic.Negate(pair.Value);
            return n;
        }

        /// <summary>
        /// Raises a base to an exponent.
        /// </summary>
        /// <param name="base">The base value.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>
        /// The power (base) ^ (exponent).
        /// </returns>
        public IDerivatives<K, V> Pow(IDerivatives<K, V> @base, IDerivatives<K, V> exponent)
        {
            var n = _factory.Create(@base, exponent);
            n.Value = _parent.Arithmetic.Pow(@base.Value, exponent.Value);
            foreach (var key in CombinedKeys(@base, exponent))
            {
                var hasLeft = @base.TryGetValue(key, out var leftDerivative);
                var hasRight = exponent.TryGetValue(key, out var rightDerivative);
                if (hasLeft && hasRight)
                {
                    n[key] = _parent.Arithmetic.Add(
                        _parent.Arithmetic.Multiply(_parent.Arithmetic.Multiply(
                            _parent.Arithmetic.Pow(@base.Value, _parent.Arithmetic.Decrement(exponent.Value)), exponent.Value), leftDerivative),
                        _parent.Arithmetic.Multiply(_parent.Arithmetic.Multiply(n.Value, _parent.Arithmetic.Log(@base.Value)), rightDerivative)
                        );
                }
                else if (hasLeft)
                    n[key] = _parent.Arithmetic.Multiply(_parent.Arithmetic.Multiply(
                        _parent.Arithmetic.Pow(@base.Value, _parent.Arithmetic.Decrement(exponent.Value)), exponent.Value), leftDerivative);
                else if (hasRight)
                {
                    n[key] = _parent.Arithmetic.Multiply(_parent.Arithmetic.Multiply(n.Value, _parent.Arithmetic.Log(@base.Value)), rightDerivative);
                }
            }
            return n;
        }

        /// <summary>
        /// Takes the natural logarithm.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The natural logarithm Ln(value).
        /// </returns>
        public IDerivatives<K, V> Log(IDerivatives<K, V> value)
        {
            var n = _factory.Create(value);
            n.Value = _parent.Arithmetic.Log(value.Value);
            foreach (var pair in value)
                n[pair.Key] = _parent.Arithmetic.Divide(pair.Value, value.Value);
            return n;
        }

        /// <summary>
        /// Increments the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The arithmetic increment (input) + unity
        /// </returns>
        public IDerivatives<K, V> Increment(IDerivatives<K, V> input)
        {
            var n = _factory.Create(input);
            n.Value = _parent.Arithmetic.Increment(input.Value);
            foreach (var pair in input)
                n[pair.Key] = pair.Value;
            return n;
        }

        /// <summary>
        /// Decrements the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The arithmetic decrement (input) - unity
        /// </returns>
        public IDerivatives<K, V> Decrement(IDerivatives<K, V> input)
        {
            var n = _factory.Create(input);
            n.Value = _parent.Arithmetic.Decrement(input.Value);
            foreach (var pair in input)
                n[pair.Key] = pair.Value;
            return n;
        }

        /// <summary>
        /// Raises the base to an integer (fixed) exponent.
        /// </summary>
        /// <param name="base">The base.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>
        /// The power (base) ^ exponent
        /// </returns>
        public IDerivatives<K, V> Pow(IDerivatives<K, V> @base, int exponent)
        {
            var n = _factory.Create(@base);
            n.Value = _parent.Arithmetic.Pow(@base.Value, exponent);
            foreach (var pair in @base)
                n[pair.Key] = _parent.Arithmetic.Multiply(_parent.Arithmetic.Pow(@base.Value, exponent - 1), pair.Value);
            return n;
        }
    }
}
