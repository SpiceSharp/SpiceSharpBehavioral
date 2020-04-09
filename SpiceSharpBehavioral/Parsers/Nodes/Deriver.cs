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
        private static readonly Node Zero = Node.Constant("0");
        private static readonly Node One = Node.Constant("1");
        private static readonly Node MinusOne = Node.Minus(Node.Constant("1"));
        private static readonly Node Two = Node.Constant("2");

        /// <summary>
        /// Gets the comparer.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        public IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deriver"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public Deriver(IEqualityComparer<string> comparer = null)
        {
            Comparer = comparer ?? EqualityComparer<string>.Default;
        }

        /// <summary>
        /// Derives the specified nodes.
        /// </summary>
        /// <param name="node">The nodes.</param>
        /// <returns>The derived nodes.</returns>
        public virtual Derivatives Derive(Node node)
        {
            // We can skip anything that is constant!
            if ((node.Properties & NodeProperties.Constant) != 0)
                return Derivatives.Empty;

            Derivatives result, a, b;
            switch (node)
            {
                case UnaryOperatorNode un:
                    a = Derive(un.Argument);
                    switch (un.NodeType)
                    {
                        case NodeTypes.Plus: return Combine(a, n => Node.Plus(n));
                        case NodeTypes.Minus: return Combine(a, n => Node.Minus(n));
                        case NodeTypes.Not: return Derivatives.Empty;
                    }
                    break;

                case BinaryOperatorNode bn:
                    a = Derive(bn.Left);
                    b = Derive(bn.Right);
                    switch (bn.NodeType)
                    {
                        case NodeTypes.Add:
                            return Combine(a, b, 
                                n1 => n1,
                                n2 => n2);
                        case NodeTypes.Subtract:
                            return Combine(a, b, 
                                n1 => n1, 
                                n2 => Node.Minus(n2), 
                                (n1, n2) => Node.Subtract(n1, n2));
                        case NodeTypes.Multiply:
                            return Combine(a, b, 
                                n1 => Multiply(n1, bn.Right),
                                n2 => Multiply(bn.Left, n2));
                        case NodeTypes.Divide:
                            return Combine(a, b, 
                                n1 => Divide(n1, bn.Right), 
                                n2 => Node.Minus(Divide(n2, Node.Power(bn.Right, Two))),
                                (n1, n2) => Divide(Node.Subtract(Multiply(n1, bn.Right), Multiply(bn.Left, n2)), Node.Power(bn.Right, Two)));
                        case NodeTypes.Pow:
                            return Combine(a, b,
                                n1 => Multiply(Multiply(bn.Right, Node.Power(bn.Left, Node.Subtract(bn.Right, One))), n1),
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
                            return Derivatives.Empty;
                    }
                    break;

                case TernaryOperatorNode tn:
                    a = Derive(tn.IfTrue);
                    b = Derive(tn.IfFalse);
                    return Combine(a, b,
                        n1 => Node.Conditional(tn.Condition, n1, Zero),
                        n2 => Node.Conditional(tn.Condition, Zero, n2),
                        (n1, n2) => Node.Conditional(tn.Condition, n1, n2));

                case FunctionNode fn:
                    result = new Derivatives();
                    for (var i = 0; i < fn.Arguments.Count; i++)
                    {
                        // Chain rule
                        a = Derive(fn.Arguments[i]);
                        string dname = "d{0}({1})".FormatString(fn.Name, i);
                        result = Combine(result, a,
                            n1 => n1,
                            n2 => Multiply(Node.Function(dname, fn.Arguments), n2));
                    }
                    return result;

                case VoltageNode vn:
                    if (vn.QuantityType != QuantityTypes.Raw)
                        throw new Exception("Invalid derivative: Voltage quantity {0} cannot be used".FormatString(vn.QuantityType));
                    if (Comparer.Equals(vn.Name, vn.Reference))
                        return new Derivatives();
                    result = new Derivatives(new Dictionary<string, Node>(Comparer), null);
                    result.Voltage.Add(vn.Name, One);
                    if (vn.Reference != null)
                        result.Voltage.Add(vn.Reference, MinusOne);
                    return result;

                case CurrentNode cn:
                    if (cn.QuantityType != QuantityTypes.Raw)
                        throw new Exception("Invalid derivative: Voltage quantity {0} cannot be used".FormatString(cn.QuantityType));
                    result = new Derivatives(null, new Dictionary<string, Node>(Comparer));
                    result.Current.Add(cn.Name, One);
                    return result;

                case ConstantNode _:
                case PropertyNode _:
                case VariableNode _:
                    return Derivatives.Empty;
            }

            throw new Exception("Could not derive expression node {0}".FormatString(node));
        }

        private static Node Multiply(Node left, Node right)
        {
            if (left.Equals(Zero) || right.Equals(Zero))
                return Zero;
            if (left.Equals(One))
                return right;
            if (right.Equals(One))
                return left;
            return Node.Multiply(left, right);
        }

        private static Node Divide(Node left, Node right)
        {
            if (left.Equals(Zero))
                return Zero;
            if (right.Equals(One))
                return left;
            return Node.Divide(left, right);
        }

        /// <summary>
        /// Combines two derivatives using combination functions.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <param name="leftOnly">The function called when only a left argument exists.</param>
        /// <param name="rightOnly">The function called when only a right argument exists.</param>
        /// <param name="both">The function called when both arguments exist.</param>
        /// <returns>The result.</returns>
        protected Derivatives Combine(
            Derivatives left, Derivatives right,
            Func<Node, Node> leftOnly, Func<Node, Node> rightOnly, Func<Node, Node, Node> both = null)
        {
            Dictionary<string, Node> v = null, c = null;

            Node nl = null, nr = null;
            if ((left.Voltage != null && left.Voltage.Count > 0) ||
                (right.Voltage != null && right.Voltage.Count > 0))
            {
                v = new Dictionary<string, Node>(Comparer);
                foreach (var key in Keys(left.Voltage, right.Voltage))
                {
                    var hasLeft = left.Voltage?.TryGetValue(key, out nl) ?? false;
                    var hasRight = right.Voltage?.TryGetValue(key, out nr) ?? false;
                    if (hasLeft && hasRight)
                    {
                        if (both != null)
                            v.Add(key, both(nl, nr));
                        else
                            v.Add(key, Node.Add(leftOnly(nl), rightOnly(nr)));
                    }
                    else if (hasLeft)
                        v.Add(key, leftOnly(nl));
                    else if (hasRight)
                        v.Add(key, rightOnly(nr));
                }
            }

            if ((left.Current != null && left.Current.Count > 0) ||
                (right.Current != null && right.Current.Count > 0))
            {
                c = new Dictionary<string, Node>(Comparer);
                foreach (var key in Keys(left.Current, right.Current))
                {
                    var hasLeft = left.Current?.TryGetValue(key, out nl) ?? false;
                    var hasRight = right.Current?.TryGetValue(key, out nr) ?? false;
                    if (hasLeft && hasRight)
                    {
                        if (both != null)
                            v.Add(key, both(nl, nr));
                        else
                            v.Add(key, Node.Add(leftOnly(nl), rightOnly(nr)));
                    }
                    else if (hasLeft)
                        c.Add(key, leftOnly(nl));
                    else if (hasRight)
                        c.Add(key, rightOnly(nr));
                }
            }

            return new Derivatives(v, c);
        }

        /// <summary>
        /// Combines the specified argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="func">The function.</param>
        /// <returns>The result.</returns>
        protected Derivatives Combine(Derivatives argument, Func<Node, Node> func)
        {
            Dictionary<string, Node> v = null, c = null;

            if (argument.Voltage != null)
            {
                v = new Dictionary<string, Node>(Comparer);
                foreach (var pair in argument.Voltage)
                    v.Add(pair.Key, func(pair.Value));
            }
            if (argument.Current != null)
            {
                c = new Dictionary<string, Node>(Comparer);
                foreach (var pair in argument.Current)
                    c.Add(pair.Key, func(pair.Value));
            }
            return new Derivatives(v, c);
        }

        private IEnumerable<string> Keys(Dictionary<string, Node> left, Dictionary<string, Node> right)
        {
            if (left != null && right != null)
                return left.Keys.Union(right.Keys).Distinct(Comparer);
            else if (left != null)
                return left.Keys;
            else if (right != null)
                return right.Keys;
            return Enumerable.Empty<string>();
        }
    }
}
