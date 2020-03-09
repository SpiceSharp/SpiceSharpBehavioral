using SpiceSharp.Attributes;
using SpiceSharpBehavioral.Parsers;
using System;

namespace SpiceSharp.Components.BehavioralComponents
{
    /// <summary>
    /// Base parameters for a behavioral component.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>
        /// The expression.
        /// </value>
        [ParameterName("expression"), ParameterName("e"), ParameterInfo("The expression describing the component.")]
        public string Expression { get; set; }

        /// <summary>
        /// Gets or sets the parser factory.
        /// </summary>
        /// <value>
        /// The parser factory.
        /// </value>
        public Func<IBehavioralParser> ParserFactory { get; set; } = () => new BehavioralParser();

        /// <summary>
        /// Gets or sets the lexer factory.
        /// </summary>
        /// <value>
        /// The lexer factory.
        /// </value>
        public Func<string, ILexer> LexerFactory { get; set; } = expression => new Lexer(expression);
    }
}
