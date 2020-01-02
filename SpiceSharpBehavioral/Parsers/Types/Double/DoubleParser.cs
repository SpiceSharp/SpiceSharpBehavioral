using SpiceSharpBehavioral.Parsers.Double;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A parser for double values.
    /// </summary>
    /// <seealso cref="Parser{Double}" />
    public class DoubleParser : Parser<double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleParser"/> class.
        /// </summary>
        public DoubleParser()
        {
            Parameters.Arithmetic = new ArithmeticOperator();
            Parameters.Conditional = new ConditionalOperator();
            Parameters.Relational = new RelationalOperator();
            Parameters.Factory = new ValueFactory();
        }
    }
}
