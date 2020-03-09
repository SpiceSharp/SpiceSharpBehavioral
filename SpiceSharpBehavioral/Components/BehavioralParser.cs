using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers;
using System;
using System.Linq.Expressions;

namespace SpiceSharp.Components.BehavioralComponents
{
    /// <summary>
    /// A behavioral parser that uses LINQ expressions to create the functions.
    /// </summary>
    /// <seealso cref="IBehavioralParser" />
    public class BehavioralParser : IBehavioralParser
    {
        /// <summary>
        /// Gets the parser.
        /// </summary>
        /// <value>
        /// The parser.
        /// </value>
        public Parser<Derivatives<Expression>> Parser { get; }

        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>
        /// The builder.
        /// </value>
        public DerivativeBuilder<Expression> Builder { get; }

        /// <summary>
        /// Gets the base builder.
        /// </summary>
        /// <value>
        /// The base builder.
        /// </value>
        public ExpressionBuilder BaseBuilder { get; }

        ISimulation IBehavioralParser.Simulation
        {
            set
            {
                Builder.Simulation = value;
                BaseBuilder.Simulation = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralParser"/> class.
        /// </summary>
        public BehavioralParser()
        {
            BaseBuilder = new ExpressionBuilder();
            Builder = new DerivativeBuilder<Expression>(BaseBuilder);
            Parser = new Parser<Derivatives<Expression>>(Builder);
        }

        /// <summary>
        /// Parses an expression using the specified lexer.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns>
        /// The parse result.
        /// </returns>
        Derivatives<Func<double>> IParser<Derivatives<Func<double>>>.Parse(ILexer lexer)
        {
            var parsed = Parser.Parse(lexer);
            var result = new Derivatives<Func<double>>
            {
                Value = Expression.Lambda<Func<double>>(parsed.Value).Compile()
            };
            foreach (var pair in parsed)
                result[pair.Key] = Expression.Lambda<Func<double>>(pair.Value).Compile();
            return result;
        }
    }
}
