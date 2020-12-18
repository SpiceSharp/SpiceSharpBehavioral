using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// A function builder.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public interface IFunctionBuilder<T> : IBuilder<Func<T>>
    {
        /// <summary>
        /// Gets or sets the fudge factor.
        /// </summary>
        /// <value>
        /// The fudging factor.
        /// </value>
        double FudgeFactor { get; set; }

        /// <summary>
        /// Gets or sets the relative tolerance.
        /// </summary>
        /// <value>
        /// The relative tolerance.
        /// </value>
        double RelativeTolerance { get; set; }

        /// <summary>
        /// Gets or sets the absolute tolerance.
        /// </summary>
        /// <value>
        /// The absolute tolerance.
        /// </value>
        double AbsoluteTolerance { get; set; }

        /// <summary>
        /// Gets or sets the function definitions.
        /// </summary>
        /// <value>
        /// The function definitions.
        /// </value>
        public Dictionary<string, ApplyFunction<T>> FunctionDefinitions { get; set; }

        /// <summary>
        /// Gets or sets the variables.
        /// </summary>
        /// <value>
        /// The function variables.
        /// </value>
        public Dictionary<VariableNode, IVariable<T>> Variables { get; set; }
    }

    /// <summary>
    /// A delegate for applying a function.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="arguments">The arguments.</param>
    public delegate void ApplyFunction<T>(IILState<T> state, IReadOnlyList<Node> arguments);
}
