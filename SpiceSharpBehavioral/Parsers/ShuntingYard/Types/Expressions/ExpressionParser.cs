using System;
using System.Linq.Expressions;
using SpiceSharpBehavioral.Parsers.ShuntingYard;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A parser for expressions.
    /// </summary>
    /// <seealso cref="Parser{T}" />
    /// <seealso cref="IParser{T}" />
    public class ExpressionParser : Parser<Expression>, IParser<Func<double>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionParser"/> class.
        /// </summary>
        /// <param name="operators">The operators.</param>
        public ExpressionParser(IParserDescription<Expression> operators)
            : base(operators)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionParser"/> class.
        /// </summary>
        public ExpressionParser()
            : base(new ShuntingYardDescription<Expression>(new ExpressionOperators()))
        {
            // We will register our default functions as well
            var operators = (ExpressionOperators)((ShuntingYardDescription<Expression>)Description).Operators;
            operators.FunctionFound += ExpressionDefaults.FunctionFound;
            operators.VariableFound += ExpressionDefaults.VariableFound;
        }

        /// <summary>
        /// Parses an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The result.</returns>
        Func<double> IParser<Func<double>>.Parse(string expression)
        {
            var result = Parse(expression);
            return Expression.Lambda<Func<double>>(result).Compile();
        }
    }
}
