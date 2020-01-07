using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Describes a set of parameters for an <see cref="IParser{T}" />.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <seealso cref="IParameterSet" />
    public interface IParserParameters<T> : IParameterSet
    {
        /// <summary>
        /// Gets the arithmetic operators.
        /// </summary>
        /// <value>
        /// The arithmetic operators.
        /// </value>
        IArithmeticOperator<T> Arithmetic { get; }

        /// <summary>
        /// Gets the conditional operators.
        /// </summary>
        /// <value>
        /// The conditional operators.
        /// </value>
        IConditionalOperator<T> Conditional { get; }

        /// <summary>
        /// Gets the relational operators.
        /// </summary>
        /// <value>
        /// The relational operators.
        /// </value>
        IRelationalOperator<T> Relational { get; }

        /// <summary>
        /// Gets the value factory.
        /// </summary>
        /// <value>
        /// The value factory.
        /// </value>
        IValueFactory<T> ValueFactory { get; }
    }
}
