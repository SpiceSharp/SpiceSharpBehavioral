using SpiceSharpBehavioral.Parsers.ShuntingYard;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A parser for doubles.
    /// </summary>
    /// <seealso cref="Parser{T}" />
    public class DoubleParser : Parser<double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleParser"/> class.
        /// </summary>
        /// <param name="operators">The operators.</param>
        public DoubleParser(IParserDescription<double> operators) 
            : base(operators)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleParser"/> class.
        /// </summary>
        public DoubleParser()
            : base(new ShuntingYardDescription<double>(new DoubleOperators()))
        {
            // We will register our default operators as well
            var operators = (DoubleOperators)((ShuntingYardDescription<double>)Description).Operators;
            operators.FunctionFound += DoubleDefaults.FunctionFound;
            operators.VariableFound += DoubleDefaults.VariableFound;
        }
    }
}
