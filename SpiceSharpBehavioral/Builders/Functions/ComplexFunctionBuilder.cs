using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// A function builder for complex numbers.
    /// </summary>
    /// <seealso cref="IFunctionBuilder{T}"/>
    public class ComplexFunctionBuilder : IFunctionBuilder<Complex>
    {
        /// <inheritdoc/>
        public double FudgeFactor { get; set; } = 1e-20;

        /// <inheritdoc/>
        public double RelativeTolerance { get; set; } = 1e-6;

        /// <inheritdoc/>
        public double AbsoluteTolerance { get; set; } = 1e-12;

        /// <inheritdoc/>
        public Dictionary<string, ApplyFunction<Complex>> FunctionDefinitions { get; set; }

        /// <inheritdoc/>
        public Dictionary<VariableNode, IVariable<Complex>> Variables { get; set; }

        /// <inheritdoc/>
        public Func<Complex> Build(Node expression)
        {
            var instance = new ComplexILState(this);
            instance.Push(expression);
            return instance.CreateFunction();
        }
    }
}
