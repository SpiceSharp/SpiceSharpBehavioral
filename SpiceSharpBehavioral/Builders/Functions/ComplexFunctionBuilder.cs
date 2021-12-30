using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Numerics;

namespace SpiceSharpBehavioral.Builders.Functions
{
    /// <summary>
    /// A function builder for complex numbers.
    /// </summary>
    /// <seealso cref="IFunctionBuilder{T}"/>
    public class ComplexFunctionBuilder : IFunctionBuilder<Complex>
    {
        /// <inheritdoc/>
        public double RelativeTolerance { get; set; } = 1e-6;

        /// <inheritdoc/>
        public double AbsoluteTolerance { get; set; } = 1e-12;

        /// <inheritdoc/>
        public event EventHandler<FunctionFoundEventArgs<Complex>> FunctionFound;

        /// <inheritdoc/>
        public event EventHandler<VariableFoundEventArgs<Complex>> VariableFound;

        /// <inheritdoc/>
        public Func<Complex> Build(Node expression)
        {
            var instance = new ComplexILState(this);
            instance.FunctionFound += (sender, args) => FunctionFound?.Invoke(sender, args);
            instance.VariableFound += (sender, args) => VariableFound?.Invoke(sender, args);
            instance.Push(expression);
            return instance.CreateFunction();
        }
    }
}
