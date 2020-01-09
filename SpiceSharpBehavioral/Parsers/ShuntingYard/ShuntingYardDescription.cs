using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers.ShuntingYard
{
    /// <summary>
    /// A description of a Shunting-Yard parser.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <seealso cref="IParserDescription{T}" />
    public class ShuntingYardDescription<T> : IParserDescription<T>
    {
        /// <summary>
        /// Gets the operators used by parsers.
        /// </summary>
        /// <value>
        /// The operators.
        /// </value>
        public IShuntingYardOperators<T> Operators { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShuntingYardDescription{T}"/> class.
        /// </summary>
        /// <param name="operators">The operators.</param>
        public ShuntingYardDescription(IShuntingYardOperators<T> operators)
        {
            Operators = operators.ThrowIfNull(nameof(operators));
        }

        /// <summary>
        /// Creates a parser state for a specified reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>
        /// The parser state.
        /// </returns>
        public IParserState<T> Create(IReader reader)
            => new ShuntingYard<T>(Operators);
    }
}
