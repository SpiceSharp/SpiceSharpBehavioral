using SpiceSharp;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Numerics;

namespace SpiceSharpBehavioral.Builders.Direct
{
    /// <summary>
    /// A builder that can compute values.
    /// </summary>
    /// <seealso cref="IBuilder{T}" />
    public class ComplexBuilder : IDirectBuilder<Complex>
    {
        /// <inheritdoc/>
        public event EventHandler<FunctionFoundEventArgs<Complex>> FunctionFound;

        /// <inheritdoc/>
        public event EventHandler<VariableFoundEventArgs<Complex>> VariableFound;

        /// <inheritdoc/>
        public double RelativeTolerance { get; set; } = 1e-6;

        /// <inheritdoc/>
        public double AbsoluteTolerance { get; set; } = 1e-12;

        /// <summary>
        /// Builds the specified value from the specified expression node.
        /// </summary>
        /// <param name="expression">The expression node.</param>
        /// <returns>
        /// The value.
        /// </returns>
        public Complex Build(Node expression)
        {
            switch (expression)
            {
                case BinaryOperatorNode bn:
                    switch (bn.NodeType)
                    {
                        case NodeTypes.Add: return Build(bn.Left) + Build(bn.Right);
                        case NodeTypes.Subtract: return Build(bn.Left) - Build(bn.Right);
                        case NodeTypes.Multiply: return Build(bn.Left) * Build(bn.Right);
                        case NodeTypes.Divide: return HelperFunctions.SafeDivide(Build(bn.Left), Build(bn.Right));
                        case NodeTypes.Modulo: return Build(bn.Left).Real % Build(bn.Right).Real;
                        case NodeTypes.LessThan: return Build(bn.Left).Real < Build(bn.Right).Real ? 1.0 : 0.0;
                        case NodeTypes.GreaterThan: return Build(bn.Left).Real > Build(bn.Right).Real ? 1.0 : 0.0;
                        case NodeTypes.LessThanOrEqual: return Build(bn.Left).Real <= Build(bn.Right).Real ? 1.0 : 0.0;
                        case NodeTypes.GreaterThanOrEqual: return Build(bn.Left).Real >= Build(bn.Right).Real ? 1.0 : 0.0;
                        case NodeTypes.Equals: return HelperFunctions.Equals(Build(bn.Left), Build(bn.Right), RelativeTolerance, AbsoluteTolerance) ? 1.0 : 0.0;
                        case NodeTypes.NotEquals: return HelperFunctions.Equals(Build(bn.Left), Build(bn.Right), RelativeTolerance, AbsoluteTolerance) ? 0.0 : 1.0;
                        case NodeTypes.And: return Build(bn.Left).Real > 0.5 && Build(bn.Right).Real > 0.5 ? 1.0 : 0.0;
                        case NodeTypes.Or: return Build(bn.Left).Real > 0.5 || Build(bn.Right).Real > 0.5 ? 1.0 : 0.0;
                        case NodeTypes.Xor: return Build(bn.Left).Real > 0.5 ^ Build(bn.Right).Real > 0.5 ? 1.0 : 0.0;
                        case NodeTypes.Pow: return HelperFunctions.Power(Build(bn.Left), Build(bn.Right));
                    }
                    break;

                case UnaryOperatorNode un:
                    switch (un.NodeType)
                    {
                        case NodeTypes.Plus: return Build(un.Argument);
                        case NodeTypes.Minus: return -Build(un.Argument);
                        case NodeTypes.Not: return Build(un.Argument).Real > 0.5 ? 0.0 : 1.0;
                    }
                    break;

                case TernaryOperatorNode tn:
                    return Build(tn.Condition).Real > 0.5 ? Build(tn.IfTrue) : Build(tn.IfFalse);

                case FunctionNode fn:
                    var fargs = new FunctionFoundEventArgs<Complex>(this, fn);
                    OnFunctionFound(fargs);
                    if (!fargs.Created)
                        throw new SpiceSharpException($"Could not recognized function {fn.Name}");
                    return fargs.Result;

                case ConstantNode cn:
                    return cn.Literal;

                case VariableNode vn:
                    var vargs = new VariableFoundEventArgs<Complex>(this, vn);
                    OnVariableFound(vargs);
                    if (!vargs.Created)
                        throw new SpiceSharpException($"Could not recognized variable {vn.Name}");
                    return vargs.Result;
            }
            return BuildNode(expression);
        }

        /// <summary>
        /// Called when a function was found.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnFunctionFound(FunctionFoundEventArgs<Complex> args) => FunctionFound?.Invoke(this, args);

        /// <summary>
        /// Called when a variable was found.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnVariableFound(VariableFoundEventArgs<Complex> args) => VariableFound?.Invoke(this, args);

        /// <summary>
        /// Builds the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The built value.</returns>
        /// <exception cref="SpiceSharpException">Unrecognized node</exception>
        protected virtual Complex BuildNode(Node node)
        {
            throw new SpiceSharpException("Unrecognized expression node {0}".FormatString(node));
        }
    }
}
