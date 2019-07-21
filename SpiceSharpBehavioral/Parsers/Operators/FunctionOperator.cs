using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.Operators
{
    /// <summary>
    /// A function operator.
    /// </summary>
    /// <seealso cref="SpiceSharpBehavioral.Parsers.Operators.BracketOperator" />
    public class FunctionOperator : BracketOperator
    {
        /// <summary>
        /// Gets the name of the operator.
        /// </summary>
        /// <value>
        /// The name of the operator.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        public int Arguments { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionOperator"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public FunctionOperator(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionOperator"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="arguments">The initial number of arguments.</param>
        public FunctionOperator(string name, int arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        /// <summary>
        /// Adds the argument.
        /// </summary>
        public void AddArgument() => Arguments++;
    }
}
