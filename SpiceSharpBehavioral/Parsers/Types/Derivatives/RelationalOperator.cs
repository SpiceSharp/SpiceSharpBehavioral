using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers.Derivatives
{
    /// <summary>
    /// Relational operators for derivatives.
    /// </summary>
    /// <typeparam name="K">The derivative key.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    /// <seealso cref="IRelationalOperator{T}" />
    public class RelationalOperator<K, V> : ParameterSet, IRelationalOperator<IDerivatives<K, V>>
    {
        private readonly IParserParameters<V> _parent;
        private readonly IDerivativeFactory<K, V> _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalOperator{K, V}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="factory">The factory.</param>
        public RelationalOperator(IParserParameters<V> parent, IDerivativeFactory<K, V> factory)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
            _factory = factory.ThrowIfNull(nameof(factory));
        }

        /// <summary>
        /// Returns a value based on whether or not the operands are equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) == (right).
        /// </returns>
        public IDerivatives<K, V> Equal(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = _factory.Create();
            n.Value = _parent.Relational.Equal(left.Value, right.Value);
            return n;
        }

        /// <summary>
        /// Returns a value based on whether or not the left operand is greater or equal than the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) greater or equal to (right).
        /// </returns>
        public IDerivatives<K, V> GreaterOrEqual(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = _factory.Create();
            n.Value = _parent.Relational.GreaterOrEqual(left.Value, right.Value);
            return n;
        }

        /// <summary>
        /// Returns a value based on whether or not the left operand is greater than the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) greater or equal to (right).
        /// </returns>
        public IDerivatives<K, V> GreaterThan(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = _factory.Create();
            n.Value = _parent.Relational.GreaterThan(left.Value, right.Value);
            return n;
        }

        /// <summary>
        /// Returns a value based on whether or not the left operand is smaller or equal than the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) less or equal to (right).
        /// </returns>
        public IDerivatives<K, V> LessOrEqual(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = _factory.Create();
            n.Value = _parent.Relational.LessOrEqual(left.Value, right.Value);
            return n;
        }

        /// <summary>
        /// Returns a value based on whether or not the left operand is smaller than the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) less than (right).
        /// </returns>
        public IDerivatives<K, V> LessThan(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = _factory.Create();
            n.Value = _parent.Relational.LessThan(left.Value, right.Value);
            return n;
        }

        /// <summary>
        /// Returns a value based on whether or not the operands are equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of (left) != (right).
        /// </returns>
        public IDerivatives<K, V> NotEqual(IDerivatives<K, V> left, IDerivatives<K, V> right)
        {
            var n = _factory.Create();
            n.Value = _parent.Relational.NotEqual(left.Value, right.Value);
            return n;
        }
    }
}
