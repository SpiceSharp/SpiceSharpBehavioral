using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Derivatives;
using System;

namespace SpiceSharpBehavioral.Components.Parsers
{
    /// <summary>
    /// Parser parameters for components.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IParserParameters{T}" />
    public class ParserParameters : ParameterSet, IParserParameters<IDerivatives<Variable, Func<double>>>
    {
        /// <summary>
        /// Gets the arithmetic operators.
        /// </summary>
        /// <value>
        /// The arithmetic operators.
        /// </value>
        public IArithmeticOperator<IDerivatives<Variable, Func<double>>> Arithmetic => throw new NotImplementedException();

        /// <summary>
        /// Gets the conditional operators.
        /// </summary>
        /// <value>
        /// The conditional operators.
        /// </value>
        public IConditionalOperator<IDerivatives<Variable, Func<double>>> Conditional => throw new NotImplementedException();

        /// <summary>
        /// Gets the relational operators.
        /// </summary>
        /// <value>
        /// The relational operators.
        /// </value>
        public IRelationalOperator<IDerivatives<Variable, Func<double>>> Relational => throw new NotImplementedException();

        /// <summary>
        /// Gets the value factory.
        /// </summary>
        /// <value>
        /// The value factory.
        /// </value>
        public IValueFactory<IDerivatives<Variable, Func<double>>> ValueFactory => throw new NotImplementedException();
    }
}
