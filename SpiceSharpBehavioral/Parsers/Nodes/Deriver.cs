using SpiceSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// A class that can derive an expression node tree.
    /// </summary>
    public class Deriver
    {
        private static readonly Node _zero = Node.Constant("0");
        private static readonly Node _one = Node.Constant("1");
        private static readonly Node _two = Node.Constant("2");

        /// <summary>
        /// Gets the map.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        public HashSet<VariableNode> Variables { get; } = new HashSet<VariableNode>();

        /// <summary>
        /// Gets or sets derivative definitions.
        /// </summary>
        /// <value>
        /// The derivative definitions.
        /// </value>
        public Dictionary<string, FunctionRule> FunctionRules { get; set; } = DeriverHelper.Defaults;

        /// <summary>
        /// Initializes a new instance of the <see cref="Deriver"/> class.
        /// </summary>
        /// <param name="variables">The variables to which the derivative should be computed.</param>
        public Deriver(IEnumerable<VariableNode> variables)
        {
            foreach (var variable in variables)
                Variables.Add(variable);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deriver"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public Deriver(params VariableNode[] variables)
            : this((IEnumerable<VariableNode>)variables)
        {
        }

        /// <summary>
        /// Derives the specified node to the variables.
        /// </summary>
        /// <param name="node">The nodes.</param>
        /// <returns>The derived nodes, or <c>null</c> if all derivatives are zero.</returns>
        public virtual Dictionary<VariableNode, Node> Derive(Node node)
        {
            // We can skip anything that is constant!
            if ((node.Properties & NodeProperties.Constant) != 0)
                return null;

            Dictionary<VariableNode, Node> a = null, b = null;
            switch (node)
            {
                case UnaryOperatorNode un:
                    a = Derive(un.Argument);
                    switch (un.NodeType)
                    {
                        case NodeTypes.Plus: return Apply(a, n => Node.Plus(n));
                        case NodeTypes.Minus: return Apply(a, n => Node.Minus(n));
                        case NodeTypes.Not: return null;
                    }
                    break;

                case BinaryOperatorNode bn:
                    a = Derive(bn.Left);
                    b = Derive(bn.Right);
                    switch (bn.NodeType)
                    {
                        case NodeTypes.Add: return Combine(a, b, n1 => n1, n2 => n2);
                        case NodeTypes.Subtract: return Combine(a, b, n1 => n1, n2 => Node.Minus(n2), (n1, n2) => Node.Subtract(n1, n2));
                        case NodeTypes.Multiply: return Combine(a, b, n1 => Multiply(n1, bn.Right), n2 => Multiply(bn.Left, n2));
                        case NodeTypes.Divide:
                            return Combine(a, b,
                                n1 => Divide(n1, bn.Right), 
                                n2 => Node.Minus(Divide(n2, Node.Power(bn.Right, _two))),
                                (n1, n2) => Divide(Node.Subtract(Multiply(n1, bn.Right), Multiply(bn.Left, n2)), Node.Power(bn.Right, _two)));
                        case NodeTypes.Pow:
                            return Combine(a, b,
                                n1 => Multiply(Multiply(bn.Right, Node.Power(bn.Left, Node.Subtract(bn.Right, _one))), n1),
                                n2 => Multiply(Multiply(bn, Node.Function("log", new[] { bn.Left })), n2));
                        case NodeTypes.Modulo:
                            return Combine(a, b,
                                n1 => n1,
                                n2 => Node.Minus(Multiply(bn, n2)),
                                (n1, n2) => Node.Subtract(n1, Node.Multiply(bn, n2)));
                        case NodeTypes.GreaterThan:
                        case NodeTypes.GreaterThanOrEqual:
                        case NodeTypes.LessThan:
                        case NodeTypes.LessThanOrEqual:
                        case NodeTypes.Equals:
                        case NodeTypes.NotEquals:
                            return null;
                    }
                    break;

                case TernaryOperatorNode tn:
                    a = Derive(tn.IfTrue);
                    b = Derive(tn.IfFalse);
                    return Combine(a, b,
                        n1 => Node.Conditional(tn.Condition, n1, _zero),
                        n2 => Node.Conditional(tn.Condition, _zero, n2),
                        (n1, n2) => Node.Conditional(tn.Condition, n1, n2));

                case FunctionNode fn:
                    if (fn.Arguments.Count == 0)
                        return null;

                    // Build the derived arguments
                    var lst = new Dictionary<VariableNode, Node[]>();
                    for (var i = 0; i < fn.Arguments.Count; i++)
                    {
                        var darg = Derive(fn.Arguments[i]);
                        if (darg != null)
                        {
                            foreach (var pair in darg)
                            {
                                if (!lst.TryGetValue(pair.Key, out var dargs))
                                {
                                    dargs = new Node[fn.Arguments.Count];
                                    lst.Add(pair.Key, dargs);
                                }
                                dargs[i] = pair.Value;
                            }
                        }
                    }

                    // If there are no derivative, let's just stop here
                    if (lst.Count == 0)
                        return null;

                    // Give a chance to our function rules
                    if (FunctionRules == null || !FunctionRules.TryGetValue(fn.Name, out var rule))
                        rule = (fn, darg) => ChainRule(fn, darg, "d{0}({1})");
                    a = new Dictionary<VariableNode, Node>();
                    foreach (var pair in lst)
                    {
                        var df = rule(fn, pair.Value);
                        if (df != null)
                            a.Add(pair.Key, df);
                    }

                    // If all derivatives are zero, return null
                    return a.Count > 0 ? a : null;
                
                case VariableNode vn:
                    if (Variables.Contains(vn))
                    {
                        a = new Dictionary<VariableNode, Node>
                        {
                            { vn, _one }
                        };
                        return a;
                    }
                    return null;

                case PropertyNode _:
                case ConstantNode _:
                    return null;
            }

            throw new Exception("Could not derive expression node {0}".FormatString(node));
        }

        /// <summary>
        /// Multiplies the specified nodes.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static Node Multiply(Node left, Node right)
        {
            if (left == null || right == null)
                return null;
            if (left.Equals(_zero) || right.Equals(_zero))
                return null;

            // Every derivative becomes 1 when derived, so quite common...
            if (left.Equals(_one))
                return right;
            if (right.Equals(_one))
                return left;
            return Node.Multiply(left, right);
        }

        /// <summary>
        /// Divides the specified nodes.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static Node Divide(Node left, Node right)
        {
            if (left.Equals(_zero))
                return _zero;
            if (right.Equals(_one))
                return left;
            return Node.Divide(left, right);
        }

        /// <summary>
        /// Applies the chain rule on a function.
        /// </summary>
        /// <param name="function">The function definition.</param>
        /// <param name="dargs">The derivatives of the arguments.</param>
        /// <param name="format">The format of the new function. {0} denotes the old name, while {1} will hold the index to which the function should be derived.</param>
        /// <returns></returns>
        public static Node ChainRule(FunctionNode function, IReadOnlyList<Node> dargs, string format)
        {
            Node result = null;
            for (var i = 0; i < dargs.Count; i++)
            {
                if (dargs[i] == null)
                    break;
                var b = Multiply(Node.Function(format.FormatString(function.Name, i), function.Arguments), dargs[i]);
                result = result == null ? b : Node.Add(result, b);
            }
            return result;
        }

        /// <summary>
        /// Applies the specified argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="func">The function.</param>
        /// <returns></returns>
        public static Dictionary<VariableNode, Node> Apply(Dictionary<VariableNode, Node> argument, Func<Node, Node> func)
        {
            if (argument == null)
                return null;
            var result = new Dictionary<VariableNode, Node>();
            foreach (var pair in argument)
                result.Add(pair.Key, func(pair.Value));
            return result;
        }

        /// <summary>
        /// Combines two arguments together.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <param name="leftOnly">The function called when the left argument is not zero.</param>
        /// <param name="rightOnly">The function called when the right argument is not zero.</param>
        /// <param name="both">The function called when both arguments are not zero.</param>
        /// <returns>The result.</returns>
        public static Dictionary<VariableNode, Node> Combine(Dictionary<VariableNode, Node> left, Dictionary<VariableNode, Node> right,
            Func<Node, Node> leftOnly, Func<Node, Node> rightOnly, Func<Node, Node, Node> both = null)
        {
            // Deal with special cases
            if (left == null && right == null)
                return null;
            else if (right == null)
                return Apply(left, leftOnly);
            else if (left == null)
                return Apply(right, rightOnly);

            // General case
            var result = new Dictionary<VariableNode, Node>();
            foreach (var key in left.Keys.Union(right.Keys).Distinct())
            {
                var hasL = left.TryGetValue(key, out var valueL);
                var hasR = right.TryGetValue(key, out var valueR);

                if (hasL && hasR)
                {
                    if (both == null)
                    {
                        var l = leftOnly(valueL);
                        var r = rightOnly(valueR);
                        if (l == null)
                            result.Add(key, r);
                        else if (r == null)
                            result.Add(key, l);
                        else
                            result.Add(key, Node.Add(l, r));
                    }
                    else
                        result.Add(key, both(valueL, valueR));
                }
                else if (hasL)
                    result.Add(key, leftOnly(valueL));
                else if (hasR)
                    result.Add(key, rightOnly(valueR));
            }
            return result;
        }
    }

    /// <summary>
    /// A function rule that can derive a function.
    /// </summary>
    /// <param name="function">The function that needs to be derived.</param>
    /// <param name="derivedArguments">The derived arguments.</param>
    /// <returns>The derived function.</returns>
    public delegate Node FunctionRule(FunctionNode function, IReadOnlyList<Node> derivedArguments);
}
