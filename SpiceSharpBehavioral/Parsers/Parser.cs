using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A parser that uses the Shunting-Yard algorithm for parsing an expression.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="Parameterized" />
    /// <seealso cref="IParser{T}" />
    public partial class Parser<T> : IParser<T>
    {
        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public IParserDescription<T> Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser{T}"/> class.
        /// </summary>
        /// <param name="operators">The operators.</param>
        public Parser(IParserDescription<T> operators)
        {
            Description = operators.ThrowIfNull(nameof(operators));
        }

        /// <summary>
        /// Parses an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// The result of the parsed expression.
        /// </returns>
        /// <exception cref="ParserException">Thrown when the expression is invalid.</exception>
        public T Parse(string expression)
        {
            var reader = new Reader(expression);
            var parser = Description.Create(reader);
            while (reader.Peek() != '\0')
                parser.ParseToken(reader);
            return parser.Execute();
        }
    }
}
