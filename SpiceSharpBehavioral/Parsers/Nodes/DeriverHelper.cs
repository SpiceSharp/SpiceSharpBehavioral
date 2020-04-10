using SpiceSharpBehavioral.Diagnostics;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// 
    /// </summary>
    public static class DeriverHelper
    {
        private static Node _zero = Node.Constant("0");
        private static Node _one = Node.Constant("1");
        private static Node _two = Node.Constant("2");

        /// <summary>
        /// The default function rules.
        /// </summary>
        public static readonly Dictionary<string, FunctionRule> Defaults = new Dictionary<string, FunctionRule>
        {
            { "abs", (f, da) => Deriver.ChainRule(f, da.Check(1), "sgn") },
            { "sgn", Zero },
            { "pwr", DPwr },
            { "sqrt", (f, da) => Deriver.Multiply(Deriver.Divide(Node.Constant("0.5"), f), da.Check(1)[0]) },
            { "log", (f, da) => Deriver.Divide(da.Check(1)[0], f.Arguments[0]) },
            { "log10", (f, da) => Deriver.Divide(Deriver.Divide(da.Check(1)[0], Node.Constant(Math.Log(10.0).ToString("e"))), f.Arguments[0]) },
            { "exp", (f, da) => Deriver.Multiply(f, da.Check(1)[0]) },
            { "sin", (f, da) => Deriver.ChainRule(f, da.Check(1), "cos") },
            { "cos", (f, da) => Node.Minus(Deriver.ChainRule(f, da.Check(1), "sin")) },
            { "tan", (f, da) => Node.Divide(da.Check(1)[0], Node.Power(Node.Function("cos", f.Arguments), _two)) },
            { "sinh", (f, da) => Deriver.ChainRule(f, da.Check(1), "cosh") },
            { "cosh", (f, da) => Deriver.ChainRule(f, da.Check(1), "sinh") },
            { "tanh", (f, da) => Deriver.Divide(da.Check(1)[0], Node.Power(Node.Function("cosh", f.Arguments), _two)) },
            { "asin", DAsin },
            { "acos", (f, da) => Node.Minus(DAsin(f, da)) },
            { "atan", DAtan },
            { "ceil", Zero },
            { "floor", Zero },
            { "nint", Zero },
            { "round", Zero },
            { "square", (f, da) => Deriver.Multiply(Deriver.Multiply(_two, da.Check(1)[0]), f.Arguments[0]) }
        };

        private static IReadOnlyList<Node> Check(this IReadOnlyList<Node> arguments, int expected)
        {
            if (arguments == null || arguments.Count != expected)
                throw new ArgumentMismatchException(expected, arguments.Count);
            return arguments;
        }

        private static Node Zero(FunctionNode f, IReadOnlyList<Node> dargs) => null;
        private static Node DAsin(FunctionNode f, IReadOnlyList<Node> dargs)
        {
            return Deriver.Divide(
                dargs.Check(1)[0],
                Node.Function("sqrt", new[] { Node.Subtract(_one, Node.Power(f.Arguments[0], _two)) })
                );
        }

        private static Node DAtan(FunctionNode f, IReadOnlyList<Node> dargs)
        {
            return Deriver.Divide(
                dargs.Check(1)[0],
                Node.Add(_one, Node.Power(f.Arguments[0], _two)));
        }

        private static Node DPwr(FunctionNode f, IReadOnlyList<Node> dargs)
        {
            dargs.Check(2);
            Node result = null;
            if (dargs[0] != null)
            {
                result = Deriver.Multiply(
                    Deriver.Multiply(f.Arguments[1], dargs[0]),
                    Node.Function("pwr", f.Arguments[0], Node.Subtract(f.Arguments[1], _one))
                    );
            }
            if (dargs[1] != null)
            {
                var b = Deriver.Multiply(
                    Deriver.Multiply(dargs[1], Node.Function("log", f.Arguments[0])),
                    f);
                result = result == null ? b : Node.Add(result, b);
            }
            return result;
        }
    }
}
