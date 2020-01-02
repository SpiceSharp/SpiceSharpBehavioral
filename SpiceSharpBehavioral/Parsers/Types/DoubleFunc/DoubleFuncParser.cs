using SpiceSharpBehavioral.Parsers.DoubleFunc;
using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A parser for double values.
    /// </summary>
    /// <seealso cref="Parser{Double}" />
    public class DoubleFuncParser : Parser<Func<double>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleParser"/> class.
        /// </summary>
        public DoubleFuncParser()
        {
            Parameters.Arithmetic = new ArithmeticOperator();
            Parameters.Conditional = new ConditionalOperator();
            Parameters.Relational = new RelationalOperator();
            Parameters.Factory = new ValueFactory();
        }
    }
}
