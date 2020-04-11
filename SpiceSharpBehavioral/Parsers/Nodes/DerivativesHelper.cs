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
        public static readonly Dictionary<string, FunctionRule> Defaults = new Dictionary<string, FunctionRule>
        {
            { "abs", (f, da) => Derivatives.ChainRule(f, da.Check(1), "sgn") },
            { "sgn", Zero },
            { "pwr", DPwr },
            { "sqrt", (f, da) => 0.5 / f * da.Check(1)[0] },
            { "log", (f, da) => da.Check(1)[0] / f.Arguments[0] },
            { "log10", (f, da) => da.Check(1)[0] / Node.Constant(Math.Log(10.0)) / f.Arguments[0] },
            { "exp", (f, da) => f * da.Check(1)[0] },
            { "sin", (f, da) => Derivatives.ChainRule(f, da.Check(1), "cos") },
            { "cos", (f, da) => Node.Minus(Derivatives.ChainRule(f, da.Check(1), "sin")) },
            { "tan", (f, da) => Node.Divide(da.Check(1)[0], Node.Power(Node.Function("cos", f.Arguments), Node.Two)) },
            { "sinh", (f, da) => Derivatives.ChainRule(f, da.Check(1), "cosh") },
            { "cosh", (f, da) => Derivatives.ChainRule(f, da.Check(1), "sinh") },
            { "tanh", (f, da) => da.Check(1)[0] / Node.Power(Node.Function("cosh", f.Arguments), Node.Two) },
            { "asin", DAsin },
            { "acos", (f, da) => -DAsin(f, da) },
            { "atan", DAtan },
            { "ceil", Zero },
            { "floor", Zero },
            { "nint", Zero },
            { "round", Zero },
            { "square", (f, da) => Node.Two * da.Check(1)[0] * f.Arguments[0] }
        };

        private static IReadOnlyList<Node> Check(this IReadOnlyList<Node> arguments, int expected)
        {
            if (arguments == null || arguments.Count != expected)
                throw new ArgumentMismatchException(expected, arguments.Count);
            return arguments;
        }

        private static Node Zero(FunctionNode f, IReadOnlyList<Node> dargs) => null;
        private static Node DAsin(FunctionNode f, IReadOnlyList<Node> dargs) => dargs.Check(1)[0] / Node.Function("sqrt", Node.One - Node.Power(f.Arguments[0], Node.Two));
        private static Node DAtan(FunctionNode f, IReadOnlyList<Node> dargs) =>  dargs.Check(1)[0] / (Node.One + Node.Power(f.Arguments[0], Node.Two));
        private static Node DPwr(FunctionNode f, IReadOnlyList<Node> dargs)
        {
            dargs.Check(2);
            Node result = null;
            if (dargs[0] != null)
                result = f.Arguments[1] * dargs[0] * Node.Function("pwr", f.Arguments[0], f.Arguments[1] - Node.One);
            if (dargs[1] != null)
                result += dargs[1] * Node.Function("log", f.Arguments[0]) * f;
            return result;
        }
    }
}
