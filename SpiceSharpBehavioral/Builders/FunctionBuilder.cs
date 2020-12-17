using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// A function builder.
    /// </summary>
    /// <seealso cref="IBuilder{T}" />
    public class FunctionBuilder : IBuilder<Func<double>>
    {
        /// <summary>
        /// Gets or sets the fudge factor.
        /// </summary>
        /// <value>
        /// The fudge factor.
        /// </value>
        public double FudgeFactor { get; set; } = 1e-20;

        /// <summary>
        /// Gets or sets the relative tolerance.
        /// </summary>
        /// <value>
        /// The relative tolerance.
        /// </value>
        public double RelativeTolerance { get; set; } = 1e-6;

        /// <summary>
        /// Gets or sets the absolute tolerance.
        /// </summary>
        /// <value>
        /// The absolute tolerance.
        /// </value>
        public double AbsoluteTolerance { get; set; } = 1e-12;

        /// <summary>
        /// Gets or sets the function definitions.
        /// </summary>
        /// <value>
        /// The function definitions.
        /// </value>
        public Dictionary<string, ApplyFunction> FunctionDefinitions { get; set; }

        /// <inheritdoc/>
        public Dictionary<VariableNode, Node> Map { get; set; }

        /// <summary>
        /// Gets or sets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public Dictionary<VariableNode, IVariable<double>> Variables { get; set; }

        /// <summary>
        /// Builds the specified value from the specified expression node.
        /// </summary>
        /// <param name="expression">The expression node.</param>
        /// <returns>
        /// The value.
        /// </returns>
        public Func<double> Build(Node expression)
        {
            // Create a new instance that will be used to build the function
            var instance = new ILState(this);
            instance.Push(expression);
            return instance.CreateFunction();
        }
    }

    /// <summary>
    /// A delegate for applying a function.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="arguments">The arguments.</param>
    public delegate void ApplyFunction(ILState state, IReadOnlyList<Node> arguments);
}
