using SpiceSharp;
using SpiceSharpBehavioral.Parsers.Operators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpBehavioral.Parsers.Derivatives
{
    /// <summary>
    /// Implementation of a value factory for derivatives.
    /// </summary>
    /// <typeparam name="K">The derivative key.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    /// <seealso cref="IValueFactory{T}" />
    public class ValueFactory<K, V> : ValueFactory<IDerivatives<K, V>>, IDerivativeOperator<K, V>
    {
        /// <summary>
        /// Gets the base parser parameters that are used to construct the derivatives.
        /// </summary>
        /// <value>
        /// The base parameters.
        /// </value>
        public IParserParameters<V> Base { get; }

        /// <summary>
        /// Gets the factory for creating new derivatives.
        /// </summary>
        /// <value>
        /// The factory for creating new derivatives.
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
        /// Initializes a new instance of the <see cref="ValueFactory{K, V}"/> class.
        /// </summary>
        /// <param name="parent">The parent parser parameters.</param>
        /// <param name="factory">The factory for derivatives.</param>
        public ValueFactory(IParserParameters<V> parent, IDerivativeFactory<K, V> factory)
        {
            Base = parent.ThrowIfNull(nameof(parent));
            Factory = factory.ThrowIfNull(nameof(factory));
            FunctionFound += Defaults<K, V>.FunctionFound;
        }

        /// <summary>
        /// Creates the value based on its textual representation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="units">The units of the value.</param>
        /// <returns>
        /// The value of the variable.
        /// </returns>
        public override IDerivatives<K, V> CreateValue(double value, string units)
        {
            var n = Factory.Create();
            n.Value = Base.ValueFactory.CreateValue(value, units);
            return n;
        }

        /// <summary>
        /// Creates the property.
        /// </summary>
        /// <param name="type">The type of property.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public override IDerivatives<K, V> CreateProperty(PropertyType type, IReadOnlyList<string> arguments)
        {
            throw new NotSupportedException();
        }
    }
}
