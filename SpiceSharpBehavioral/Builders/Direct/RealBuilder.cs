using SpiceSharp;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;

namespace SpiceSharpBehavioral.Builders.Direct
{
    /// <summary>
    /// A builder that can compute values.
    /// </summary>
    /// <seealso cref="IBuilder{T}" />
    public class RealBuilder : IDirectBuilder<double>
    {
        /// <inheritdoc/>
        public event EventHandler<FunctionFoundEventArgs<double>> FunctionFound;

        /// <inheritdoc/>
        public event EventHandler<VariableFoundEventArgs<double>> VariableFound;

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
        public double Build(Node expression)
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
                        case NodeTypes.Modulo: return Build(bn.Left) % Build(bn.Right);
                        case NodeTypes.LessThan: return Build(bn.Left) < Build(bn.Right) ? 1.0 : 0.0;
                        case NodeTypes.GreaterThan: return Build(bn.Left) > Build(bn.Right) ? 1.0 : 0.0;
                        case NodeTypes.LessThanOrEqual: return Build(bn.Left) <= Build(bn.Right) ? 1.0 : 0.0;
                        case NodeTypes.GreaterThanOrEqual: return Build(bn.Left) >= Build(bn.Right) ? 1.0 : 0.0;
                        case NodeTypes.Equals: return HelperFunctions.Equals(Build(bn.Left), Build(bn.Right), RelativeTolerance, AbsoluteTolerance) ? 1.0 : 0.0;
                        case NodeTypes.NotEquals: return HelperFunctions.Equals(Build(bn.Left), Build(bn.Right), RelativeTolerance, AbsoluteTolerance) ? 0.0 : 1.0;
                        case NodeTypes.And: return Build(bn.Left) > 0.5 && Build(bn.Right) > 0.5 ? 1.0 : 0.0;
                        case NodeTypes.Or: return Build(bn.Left) > 0.5 || Build(bn.Right) > 0.5 ? 1.0 : 0.0;
                        case NodeTypes.Xor: return Build(bn.Left) > 0.5 ^ Build(bn.Right) > 0.5 ? 1.0 : 0.0;
                        case NodeTypes.Pow: return HelperFunctions.Power(Build(bn.Left), Build(bn.Right));
                    }
                    break;

                case UnaryOperatorNode un:
                    switch (un.NodeType)
                    {
                        case NodeTypes.Plus: return Build(un.Argument);
                        case NodeTypes.Minus: return -Build(un.Argument);
                        case NodeTypes.Not: return Build(un.Argument) > 0.5 ? 0.0 : 1.0;
                    }
                    break;

                case TernaryOperatorNode tn:
                    return Build(tn.Condition) > 0.5 ? Build(tn.IfTrue) : Build(tn.IfFalse);

                case FunctionNode fn:
                    var fargs = new FunctionFoundEventArgs<double>(this, fn);
                    OnFunctionFound(fargs);
                    if (!fargs.Created)
                        throw new SpiceSharpException($"Could not recognize function {fn.Name}");
                    return fargs.Result;

                case ConstantNode cn:
                    return cn.Literal;

                case VariableNode vn:
                    var vargs = new VariableFoundEventArgs<double>(this, vn);
                    OnVariableFound(vargs);
                    if (!vargs.Created)
                        throw new SpiceSharpException($"Could not recognize variable {vn.Name}");
                    return vargs.Result;
            }
            return BuildNode(expression);
        }

        /// <summary>
        /// Called when a function was found.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnFunctionFound(FunctionFoundEventArgs<double> args) => FunctionFound?.Invoke(this, args);

        /// <summary>
        /// Called when a variable was found.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnVariableFound(VariableFoundEventArgs<double> args) => VariableFound?.Invoke(this, args);

        /// <summary>
        /// Builds the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The built value.</returns>
        /// <exception cref="SpiceSharpException">Unrecognized node</exception>
        protected virtual double BuildNode(Node node)
        {
            throw new SpiceSharpException("Unrecognized expression node {0}".FormatString(node));
        }
    }
}
