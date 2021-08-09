using SpiceSharpBehavioral.Diagnostics;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// 
    /// </summary>
    public static class DerivativesHelper
    {
        /// <summary>
        /// The default function rules.
        /// </summary>
        public static Dictionary<string, FunctionRule> Defaults { get; set; } = new Dictionary<string, FunctionRule>(StringComparer.OrdinalIgnoreCase)
        {
            { "abs", (f, da) => Derivatives.ChainRule(f, da.Check(1), "sgn") },
            { "sgn", Zero },
            { "pow", DPwrs }, // We can reuse here
            { "pwr", DPwr },
            { "pwrs", DPwrs },
            { "sqrt", (f, da) => 0.5 / f * da.Check(1)[0] },
            { "log", (f, da) => da.Check(1)[0] / f.Arguments[0] },
            { "ln", (f, da) => da.Check(1)[0] / f.Arguments[0] },
            { "log10", (f, da) => da.Check(1)[0] / Node.Constant(Math.Log(10.0)) / f.Arguments[0] },
            { "exp", (f, da) => f * da.Check(1)[0] },
            { "sin", (f, da) => Derivatives.ChainRule(f, da.Check(1), "cos") },
            { "cos", (f, da) => Node.Minus(Derivatives.ChainRule(f, da.Check(1), "sin")) },
            { "tan", (f, da) => Node.Divide(da.Check(1)[0], Node.Power(Node.Function("cos", f.Arguments), Node.Two)) },
            { "sinh", (f, da) => Derivatives.ChainRule(f, da.Check(1), "cosh") },
            { "cosh", (f, da) => Derivatives.ChainRule(f, da.Check(1), "sinh") },
            { "tanh", (f, da) => da.Check(1)[0] / Node.Power(Node.Function("cosh", f.Arguments), Node.Two) },
            { "asin", DAsin }, { "arcsin", DAsin },
            { "acos", (f, da) => -DAsin(f, da) }, { "arccos", (f, da) => -DAsin(f, da) },
            { "atan", DAtan }, { "arctan", DAtan },
            { "atan2", DAtan2 },
            { "atanh", DAtanh },
            { "hypot", DHypot },
            { "ceil", Zero },
            { "floor", Zero },
            { "nint", Zero },
            { "round", Zero },
            { "min", Min },
            { "max", Max },
            { "square", (f, da) => Node.Two * da.Check(1)[0] * f.Arguments[0] },
            { "ddt", (f, da) => Node.Function("ddt_slope", new[] { da.Check(1)[0] }) },
            { "idt", (f, da) => Node.Function("idt_slope", new[] { da.Check(1)[0] }) },
            { "limit", Limit },
            { "db", (f, da) => Node.Constant(8.68588963807) * da.Check(1)[0] / f.Arguments[0] },
            { "real", (f, da) => Node.Function("real", da.Check(1)[0]) },
            { "imag", (f, da) => Node.Function("imag", da.Check(1)[0]) },
            { "arg", (f, da) => DAtan2(f, new[] { Node.Function("imag", da.Check(1)[0]), Node.Function("real", da[0]) }) },
        };

        /// <summary>
        /// Helper methods that changes the equality comparer for function names.
        /// </summary>
        /// <param name="comparer">The name comparer.</param>
        public static void RemapFunctions(IEqualityComparer<string> comparer)
        {
            var nmap = new Dictionary<string, FunctionRule>(comparer);
            foreach (var map in Defaults)
            {
                nmap.Add(map.Key, map.Value);
            }
            Defaults = nmap;
        }

        private static IReadOnlyList<Node> Check(this IReadOnlyList<Node> arguments, int expected)
        {
            if (arguments == null || arguments.Count != expected)
                throw new ArgumentMismatchException(expected, arguments.Count);
            return arguments;
        }
        private static Node Zero(FunctionNode f, IReadOnlyList<Node> dargs) => null;
        private static Node Min(FunctionNode f, IReadOnlyList<Node> dargs)
        {
            dargs.Check(2);
            Node result = Node.Conditional(Node.LessThan(f.Arguments[0], f.Arguments[1]), dargs[0] ?? Node.Zero, dargs[1] ?? Node.Zero);
            return result;
        }
        private static Node Max(FunctionNode f, IReadOnlyList<Node> dargs)
        {
            dargs.Check(2);
            Node result = Node.Conditional(Node.GreaterThan(f.Arguments[0], f.Arguments[1]), dargs[0] ?? Node.Zero, dargs[1] ?? Node.Zero);
            return result;
        }
        private static Node DAsin(FunctionNode f, IReadOnlyList<Node> dargs) => dargs.Check(1)[0] / Node.Function("sqrt", Node.One - Node.Power(f.Arguments[0], Node.Two));
        private static Node DAtan(FunctionNode f, IReadOnlyList<Node> dargs) =>  dargs.Check(1)[0] / (Node.One + Node.Power(f.Arguments[0], Node.Two));
        private static Node DPwr(FunctionNode f, IReadOnlyList<Node> dargs)
        {
            dargs.Check(2);
            Node result = null;
            if (dargs[0] != null)
                result = Node.Function("sgn", f.Arguments[0]) * f.Arguments[1] * dargs[0] * Node.Function(f.Name, f.Arguments[0], f.Arguments[1] - 1.0);
            if (dargs[1] != null)
                result += dargs[1] * Node.Function("log", f.Arguments[0]) * f;
            return result;
        }
        private static Node DPwrs(FunctionNode f, IReadOnlyList<Node> dargs)
        {
            dargs.Check(2);
            Node result = null;
            if (dargs[0] != null)
                result = f.Arguments[1] * dargs[0] * Node.Function(f.Name, f.Arguments[0], f.Arguments[1] - 1.0);
            if (dargs[1] != null)
                result += dargs[1] * Node.Function("log", f.Arguments[0]) * f;
            return result;
        }
        private static Node DAtan2(FunctionNode f, IReadOnlyList<Node> dargs)
        {
            dargs.Check(2);
            if (dargs[1] == null)
            {
                if (dargs[0] == null)
                    return null;
                return f.Arguments[1] * dargs[0] / (Node.Function("square", f.Arguments[0]) + Node.Function("square", f.Arguments[1]));
            }
            else if (dargs[0] == null)
                return -f.Arguments[0] * dargs[1] / (Node.Function("square", f.Arguments[0]) + Node.Function("square", f.Arguments[1]));
            else
                return (f.Arguments[1] * dargs[0] - f.Arguments[0] * dargs[1]) / (Node.Function("square", f.Arguments[0]) + Node.Function("square", f.Arguments[1]));
        }
        private static Node DAtanh(FunctionNode f, IReadOnlyList<Node> dargs) => dargs.Check(1)[0] / (1 - Node.Function("square", f.Arguments[0]));
        private static Node DHypot(FunctionNode f, IReadOnlyList<Node> dargs)
        {
            dargs.Check(2);
            if (dargs[1] == null)
            {
                if (dargs[0] == null)
                    return null;
                return 0.5 * f.Arguments[0] * dargs[0] / f;
            }
            else if (dargs[0] == null)
                return 0.5 * f.Arguments[1] * dargs[1] / f;
            else
                return 0.5 * (f.Arguments[0] * dargs[0] + f.Arguments[1] * dargs[1]) / f;
        }
        private static Node DPassThrough(FunctionNode f, IReadOnlyList<Node> dargs) => Node.Function(f.Name, dargs);

        private static Node Limit(FunctionNode f, IReadOnlyList<Node> dargs)
        {
            dargs.Check(3);

            var x = f.Arguments[0];
            var y = f.Arguments[1];
            var z = f.Arguments[2];

            Node result = null;
            if (dargs[0] != null)
            {
                result = Node.Conditional(
                    Node.And(
                        Node.GreaterThan(x, Node.Function("min", y, z)),
                        Node.LessThan(x, Node.Function("max", y, z))),
                    dargs[0],
                    Node.Zero);
            }

            if (dargs[1] != null)
            {
                result += Node.Conditional(
                    Node.And(
                        Node.LessThanOrEqual(y, z),
                        Node.LessThanOrEqual(x, Node.Function("min", y, z))),
                    dargs[1],
                    Node.Zero);
            }

            if (dargs[2] != null)
            {
                result += Node.Conditional(
                    Node.And(
                        Node.LessThan(y, z),
                        Node.GreaterThanOrEqual(x, Node.Function("max", y, z))),
                    dargs[2],
                    Node.Zero);
            }

            return result;
        }
    }
}
