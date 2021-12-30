using SpiceSharpBehavioral.Parsers.Nodes;
using System;

namespace SpiceSharpBehavioral.Builders.Functions
{
    /// <summary>
    /// A function builder for real numbers.
    /// </summary>
    /// <seealso cref="IFunctionBuilder{T}" />
    public class RealFunctionBuilder : IFunctionBuilder<double>
    {
        /// <inheritdoc/>
        public double RelativeTolerance { get; set; } = 1e-6;

        /// <inheritdoc/>
        public double AbsoluteTolerance { get; set; } = 1e-12;

        /// <inheritdoc/>
        public event EventHandler<FunctionFoundEventArgs<double>> FunctionFound;

        /// <inheritdoc/>
        public event EventHandler<VariableFoundEventArgs<double>> VariableFound;

        /// <inheritdoc/>
        public Func<double> Build(Node expression)
        {
            // Create a new instance that will be used to build the function
            var instance = new RealILState(this);
            instance.FunctionFound += (sender, args) => FunctionFound?.Invoke(sender, args);
            instance.VariableFound += (sender, args) => VariableFound?.Invoke(sender, args);
            instance.Push(expression);
            return instance.CreateFunction();
        }
    }
}
