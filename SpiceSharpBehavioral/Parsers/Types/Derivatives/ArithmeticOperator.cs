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
    public class ArithmeticOperator<K, V> : ParameterSet, IArithmeticOperator<IDerivatives<K, V>>, IDerivativeOperator<K, V>
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        /// <value>
        /// The base.
        /// </value>
        public IParserParameters<V> Base { get; }

        /// <summary>
        /// Gets the factory for derivatives.
        /// </summary>
        /// <value>
        /// The factory.
        /// </value>
        public IDerivativeFactory<K, V> Factory { get; }

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
            Base = parent.ThrowIfNull(nameof(parent));
            Factory = factory.ThrowIfNull(nameof(factory));
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
            var n = Factory.Create(left, right);
            n.Value = Base.Arithmetic.Add(left.Value, right.Value);

            // Adding of derivatives
            foreach (var key in CombinedKeys(left, right))
            {
                var hasLeft = left.TryGetValue(key, out var leftDerivative);
                var hasRight = right.TryGetValue(key, out var rightDerivative);
                if (hasLeft && hasRight)
                    n[key] = Base.Arithmetic.Add(leftDerivative, rightDerivative);
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
            var n = Factory.Create(left, right);
            n.Value = Base.Arithmetic.Subtract(left.Value, right.Value);

            // Adding of derivatives
            foreach (var key in CombinedKeys(left, right))
            {
                var hasLeft = left.TryGetValue(key, out var leftDerivative);
                var hasRight = right.TryGetValue(key, out var rightDerivative);
                if (hasLeft && hasRight)
                    n[key] = Base.Arithmetic.Subtract(leftDerivative, rightDerivative);
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
            var n = Factory.Create(left, right);
            n.Value = Base.Arithmetic.Divide(left.Value, right.Value);

            // Adding of derivatives
            foreach (var key in CombinedKeys(left, right))
            {
                var hasLeft = left.TryGetValue(key, out var leftDerivative);
                var hasRight = right.TryGetValue(key, out var rightDerivative);
                if (hasLeft && hasRight)
                {
                    n[key] = Base.Arithmetic.Divide(
                        Base.Arithmetic.Subtract(
                            Base.Arithmetic.Multiply(leftDerivative, right.Value),
                            Base.Arithmetic.Multiply(left.Value, rightDerivative)),
                        Base.Arithmetic.Pow(right.Value, 2));
                }
                else if (hasLeft)
                    n[key] = Base.Arithmetic.Divide(leftDerivative, right.Value);
                else if (hasRight)
                {
                    n[key] = Base.Arithmetic.Negate(
                        Base.Arithmetic.Divide(
                            Base.Arithmetic.Multiply(left.Value, rightDerivative),
                            Base.Arithmetic.Pow(right.Value, 2)));
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
            var n = Factory.Create(left, right);
            n.Value = Base.Arithmetic.Modulo(left.Value, right.Value);
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
            var n = Factory.Create(left, right);
            n.Value = Base.Arithmetic.Multiply(left.Value, right.Value);

            foreach (var key in CombinedKeys(left, right))
            {
                var hasLeft = left.TryGetValue(key, out var leftDerivative);
                var hasRight = right.TryGetValue(key, out var rightDerivative);
                if (hasLeft && hasRight)
                    n[key] = Base.Arithmetic.Add(
                        Base.Arithmetic.Multiply(leftDerivative, right.Value),
                        Base.Arithmetic.Multiply(left.Value, rightDerivative));
                else if (hasLeft)
                    n[key] = Base.Arithmetic.Multiply(leftDerivative, right.Value);
                else if (hasRight)
                    n[key] = Base.Arithmetic.Multiply(left.Value, rightDerivative);
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
            var n = Factory.Create(input);
            n.Value = Base.Arithmetic.Negate(input.Value);
            foreach (var pair in input)
                n[pair.Key] = Base.Arithmetic.Negate(pair.Value);
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
            var n = Factory.Create(@base, exponent);
            n.Value = Base.Arithmetic.Pow(@base.Value, exponent.Value);
            foreach (var key in CombinedKeys(@base, exponent))
            {
                var hasLeft = @base.TryGetValue(key, out var leftDerivative);
                var hasRight = exponent.TryGetValue(key, out var rightDerivative);
                if (hasLeft && hasRight)
                {
                    n[key] = Base.Arithmetic.Add(
                        Base.Arithmetic.Multiply(
                            Base.Arithmetic.Multiply(
                                Base.Arithmetic.Pow(@base.Value, Base.Arithmetic.Decrement(exponent.Value)), 
                                exponent.Value),
                            leftDerivative),
                        Base.Arithmetic.Multiply(
                            Base.Arithmetic.Multiply(n.Value, Base.Arithmetic.Log(@base.Value)),
                            rightDerivative));
                }
                else if (hasLeft)
                    n[key] = Base.Arithmetic.Multiply(Base.Arithmetic.Multiply(
                        Base.Arithmetic.Pow(@base.Value, Base.Arithmetic.Decrement(exponent.Value)), exponent.Value), leftDerivative);
                else if (hasRight)
                {
                    n[key] = Base.Arithmetic.Multiply(Base.Arithmetic.Multiply(n.Value, Base.Arithmetic.Log(@base.Value)), rightDerivative);
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
            var n = Factory.Create(value);
            n.Value = Base.Arithmetic.Log(value.Value);
            foreach (var pair in value)
                n[pair.Key] = Base.Arithmetic.Divide(pair.Value, value.Value);
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
            var n = Factory.Create(input);
            n.Value = Base.Arithmetic.Increment(input.Value);
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
            var n = Factory.Create(input);
            n.Value = Base.Arithmetic.Decrement(input.Value);
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
            var n = Factory.Create(@base);
            n.Value = Base.Arithmetic.Pow(@base.Value, exponent);
            foreach (var pair in @base)
                n[pair.Key] = Base.Arithmetic.Multiply(Base.Arithmetic.Pow(@base.Value, exponent - 1), pair.Value);
            return n;
        }
    }
}
