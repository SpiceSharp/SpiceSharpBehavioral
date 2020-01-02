using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Parameters for an <see cref="IParser{T}"/>.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="ParameterSet" />
    public class ParserParameters<T> : ParameterSet
    {
        /// <summary>
        /// Gets or sets the arithmetic operator description.
        /// </summary>
        /// <value>
        /// The arithmetic operator description.
        /// </value>
        public IArithmeticOperator<T> Arithmetic { get; set; }

        /// <summary>
        /// Gets or sets the relational operator description.
        /// </summary>
        /// <value>
        /// The relational operator description.
        /// </value>
        public IRelationalOperator<T> Relational { get; set; }

        /// <summary>
        /// Gets or sets the conditional operator description.
        /// </summary>
        /// <value>
        /// The conditional operator description.
        /// </value>
        public IConditionalOperator<T> Conditional { get; set; }

        /// <summary>
        /// Gets or sets the factory.
        /// </summary>
        /// <value>
        /// The factory.
        /// </value>
        public IValueFactory<T> Factory { get; set; }
    }
}
