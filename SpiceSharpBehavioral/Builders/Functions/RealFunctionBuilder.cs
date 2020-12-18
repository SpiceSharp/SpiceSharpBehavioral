using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// A function builder for real numbers.
    /// </summary>
    /// <seealso cref="IFunctionBuilder{T}" />
    public class RealFunctionBuilder : IFunctionBuilder<double>
    {
        /// <inheritdoc/>
        public double FudgeFactor { get; set; } = 1e-20;

        /// <inheritdoc/>
        public double RelativeTolerance { get; set; } = 1e-6;

        /// <inheritdoc/>
        public double AbsoluteTolerance { get; set; } = 1e-12;

        /// <inheritdoc/>
        public Dictionary<string, ApplyFunction<double>> FunctionDefinitions { get; set; }

        /// <inheritdoc/>
        public Dictionary<VariableNode, IVariable<double>> Variables { get; set; }

        /// <inheritdoc/>
        public Func<double> Build(Node expression)
        {
            // Create a new instance that will be used to build the function
            var instance = new RealILState(this);
            instance.Push(expression);
            return instance.CreateFunction();
        }
    }
}
