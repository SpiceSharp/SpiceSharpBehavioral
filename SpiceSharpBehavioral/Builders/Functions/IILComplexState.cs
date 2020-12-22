using SpiceSharpBehavioral.Parsers.Nodes;
using System.Numerics;

namespace SpiceSharpBehavioral.Builders.Functions
{
    /// <summary>
    /// Additional methods that can be supplied by a complex IL state.
    /// </summary>
    public interface IILComplexState : IILState<Complex>
    {
        /// <summary>
        /// Get the real part of a complex value on the stack.
        /// </summary>
        /// <param name="node">The node (optional).</param>
        void PushReal(Node node = null);

        /// <summary>
        /// Convert a real value on the stack to a complex value.
        /// </summary>
        void RealToComplex();
    }
}
