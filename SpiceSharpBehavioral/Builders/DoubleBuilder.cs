using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// A builder that can compute values.
    /// </summary>
    /// <seealso cref="IBuilder{T}" />
    public class DoubleBuilder : IBuilder<double>
    {
        /// <summary>
        /// Gets the functions.
        /// </summary>
        /// <value>
        /// The functions.
        /// </value>
        public Dictionary<string, Func<double[], double>> FunctionDefinitions { get; set; }

        /// <summary>
        /// Gets or sets the voltages.
        /// </summary>
        /// <value>
        /// The voltages.
        /// </value>
        public Dictionary<string, IVariable<double>> Voltages { get; set; }

        /// <summary>
        /// Gets or sets the currents.
        /// </summary>
        /// <value>
        /// The currents.
        /// </value>
        public Dictionary<string, IVariable<double>> Currents { get; set; }

        /// <summary>
        /// Gets or sets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public Dictionary<string, IVariable<double>> Variables { get; set; }

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
                        case NodeTypes.Divide: return Functions.SafeDivide(Build(bn.Left), Build(bn.Right), FudgeFactor);
                        case NodeTypes.Modulo: return Build(bn.Left) % Build(bn.Right);
                        case NodeTypes.LessThan: return Build(bn.Left) < Build(bn.Right) ? 1.0 : 0.0;
                        case NodeTypes.GreaterThan: return Build(bn.Left) > Build(bn.Right) ? 1.0 : 0.0;
                        case NodeTypes.LessThanOrEqual: return Build(bn.Left) <= Build(bn.Right) ? 1.0 : 0.0;
                        case NodeTypes.GreaterThanOrEqual: return Build(bn.Left) >= Build(bn.Right) ? 1.0 : 0.0;
                        case NodeTypes.Equals: return Functions.Equals(Build(bn.Left), Build(bn.Right), RelativeTolerance, AbsoluteTolerance) ? 1.0 : 0.0;
                        case NodeTypes.NotEquals: return Functions.Equals(Build(bn.Left), Build(bn.Right), RelativeTolerance, AbsoluteTolerance) ? 0.0 : 1.0;
                        case NodeTypes.Pow: return Functions.Power(Build(bn.Left), Build(bn.Right));
                    }
                    break;

                case UnaryOperatorNode un:
                    switch (un.NodeType)
                    {
                        case NodeTypes.Plus: return Build(un.Argument);
                        case NodeTypes.Minus: return -Build(un.Argument);
                        case NodeTypes.Not: return Build(un.Argument) > 0 ? 0.0 : 1.0;
                    }
                    break;

                case TernaryOperatorNode tn:
                    return Build(tn.Condition) > 0.0 ? Build(tn.IfTrue) : Build(tn.IfFalse);

                case FunctionNode fn:
                    if (FunctionDefinitions != null && FunctionDefinitions.TryGetValue(fn.Name, out var definition))
                    {
                        var args = new double[fn.Arguments.Count];
                        for (var i = 0; i < args.Length; i++)
                            args[i] = Build(fn.Arguments[i]);
                        return definition.Invoke(args);
                    }
                    break;

                case ConstantNode cn:
                    return Functions.ParseNumber(cn.Literal);

                case VoltageNode vn:
                    if (vn.QuantityType == QuantityTypes.Raw && Voltages != null)
                    {
                        if (Voltages.TryGetValue(vn.Name, out var variable))
                        {
                            var result = variable.Value;
                            if (vn.Reference != null && Voltages.TryGetValue(vn.Reference, out variable))
                                result -= variable.Value;
                            return result;
                        }
                    }
                    break;

                case CurrentNode curn:
                    if (curn.QuantityType == QuantityTypes.Raw && Currents != null)
                    {
                        if (Currents.TryGetValue(curn.Name, out var variable))
                            return variable.Value;
                    }
                    break;

                case VariableNode varn:
                    if (Variables != null)
                    {
                        if (Variables.TryGetValue(varn.Name, out var variable))
                            return variable.Value;
                    }
                    break;
            }
            return BuildNode(expression);
        }

        /// <summary>
        /// Builds the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Unrecognized node</exception>
        protected virtual double BuildNode(Node node)
        {
            throw new Exception("Unrecognized expression node {0}".FormatString(node));
        }
    }
}
