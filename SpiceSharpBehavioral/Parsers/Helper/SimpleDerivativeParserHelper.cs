using SpiceSharp;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.Helper
{
    /// <summary>
    /// Helper methods for a <see cref="SimpleDerivativeParser"/>.
    /// </summary>
    public static class SimpleDerivativeParserHelper
    {
        /// <summary>
        /// The default functions.
        /// </summary>
        public static Dictionary<string, DoubleDerivativesFunction> DefaultFunctions { get; set; } = new Dictionary<string, DoubleDerivativesFunction>()
        {
            { "Exp", ApplyExp },
            { "Log", ApplyLog },
            { "Pow", ApplyPow },
            { "Log10", ApplyLog10 },
            { "Sqrt", ApplySqrt },
            { "Sin", ApplySin },
            { "Cos", ApplyCos },
            { "Tan", ApplyTan },
            { "Asin", ApplyAsin },
            { "Acos", ApplyAcos },
            { "Atan", ApplyAtan },
            { "Abs", ApplyAbs },
            { "Round", ApplyRound },
            { "Min", ApplyMin },
            { "Max", ApplyMax }
        };

        /// <summary>
        /// Register default functions.
        /// </summary>
        /// <param name="parser">The parser.</param>
        public static void RegisterDefaultFunctions(this SimpleDerivativeParser parser)
        {
            parser.FunctionFound += DefaultFunctionFound;
        }

        /// <summary>
        /// Remove the default functions from the parser.
        /// </summary>
        /// <param name="parser">The parser.</param>
        public static void UnregisterDefaultFunctions(this SimpleDerivativeParser parser)
        {
            parser.FunctionFound -= DefaultFunctionFound;
        }

        /// <summary>
        /// Default functions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DefaultFunctionFound(object sender, FunctionFoundEventArgs<DoubleDerivatives> e)
        {
            if (DefaultFunctions.TryGetValue(e.Name, out var function))
            {
                var arguments = new DoubleDerivatives[e.ArgumentCount];
                for (var i = 0; i < e.ArgumentCount; i++)
                    arguments[i] = e[i];
                e.Result = function?.Invoke(arguments);
            }
        }

        /// <summary>
        /// Exponentials
        /// </summary>
        public static DoubleDerivatives ApplyExp(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new DoubleDerivatives(arg.Count);
            result[0] = Math.Exp(arg[0]);

            // Chain rule for derivatives
            for (var i = 1; i < arg.Count; i++)
            {
                if (!arg[i].Equals(0.0))
                    result[i] = result[0] * arg[i];
            }
            return result;
        }

        /// <summary>
        /// Logarithms
        /// </summary>
        public static DoubleDerivatives ApplyLog(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            if (arguments.Length == 1)
            {
                // Ln(f(x))' = 1/f(x)*f'(x)
                var arg = arguments[0];
                var result = new DoubleDerivatives(arg.Count);
                result[0] = Math.Log(arg[0]);

                // Chain rule
                for (var i = 1; i < arg.Count; i++)
                {
                    if (!arg[i].Equals(0.0))
                        result[i] = arg[i] / arg[0];
                }
                return result;
            }

            if (arguments.Length == 2)
            {
                // Log(g(x), f(x)) = Ln(g(x)) / Ln(f(x))
                var g = arguments[0];
                var f = arguments[1];
                var size = Math.Max(f.Count, g.Count);
                var result = new DoubleDerivatives(size);
                result[0] = Math.Log(g[0], f[0]);

                // Chain rule
                for (var i = 1; i < size; i++)
                {
                    if (!g[i].Equals(0.0))
                        result[i] = g[i] / g[0] / Math.Log(f[0]);
                    if (!f[i].Equals(0.0))
                        result[i] -= f[i] / f[0] * result[0];
                }
                return result;
            }

            throw new CircuitException("Invalid number of arguments, {0} given but 2 expected".FormatString(arguments.Length));
        }
        public static DoubleDerivatives ApplyLog10(DoubleDerivatives[] arguments)
        {
            var result = ApplyLog(arguments);
            var a = Math.Log(10.0);
            for (var i = 0; i < arguments[0].Count; i++)
                result[i] = result[i] / a;
            return result;
        }

        /// <summary>
        /// Power method.
        /// </summary>
        public static DoubleDerivatives ApplyPow(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 2);
            var f = arguments[0];
            var g = arguments[1];
            var size = Math.Max(f.Count, g.Count);
            var result = new DoubleDerivatives(size);
            result[0] = Math.Pow(f[0], g[0]);

            // Apply chain rule
            for (var i = 1; i < size; i++)
            {
                if (!f[i].Equals(0.0))
                    result[i] = g[0] * f[i] * Math.Pow(f[0], g[0] - 1);
                if (!g[i].Equals(0.0))
                    result[i] += g[i] * result[0] * Math.Log(f[0]);
            }
            return result;
        }

        /// <summary>
        /// Square root method.
        /// </summary>
        public static DoubleDerivatives ApplySqrt(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new DoubleDerivatives(arg.Count);
            result[0] = Math.Sqrt(arg[0]);

            // Apply the chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (!arg[i].Equals(0.0))
                    result[i] = -0.5 * arg[i] / result[0];
            }
            return result;
        }

        /// <summary>
        /// Trigonometry
        /// </summary>
        public static DoubleDerivatives ApplySin(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new DoubleDerivatives(arg.Count);
            result[0] = Math.Sin(arg[0]);

            // Apply the chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (!arg[i].Equals(0.0))
                    result[i] = Math.Cos(arg[0]) * arg[i];
            }
            return result;
        }
        private static DoubleDerivatives ApplyCos(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new DoubleDerivatives(arg.Count);
            result[0] = Math.Cos(arg[0]);

            // Apply the chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (!arg[i].Equals(0.0))
                    result[i] = -Math.Sin(arg[0]) * arg[i];
            }
            return result;
        }
        private static DoubleDerivatives ApplyTan(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new DoubleDerivatives(arg.Count);
            result[0] = Math.Tan(arg[0]);

            // Apply chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (!arg[i].Equals(0.0))
                    result[i] = arg[i] / Square(Math.Cos(arg[0]));
            }
            return result;
        }

        /// <summary>
        /// Inverse trigonometry.
        /// </summary>
        public static DoubleDerivatives ApplyAsin(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new DoubleDerivatives(arg.Count);
            result[0] = Math.Asin(arg[0]);

            // Apply chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (!arg[i].Equals(0.0))
                    result[i] = arg[i] / Math.Sqrt(1 - Square(arg[0]));
            }
            return result;
        }
        public static DoubleDerivatives ApplyAcos(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new DoubleDerivatives(arg.Count);
            result[0] = Math.Acos(arg[0]);

            // Apply chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (!arg[i].Equals(0.0))
                    result[i] = -arg[i] / Math.Sqrt(1 - Square(arg[0]));
            }
            return result;
        }
        public static DoubleDerivatives ApplyAtan(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new DoubleDerivatives(arg.Count);
            result[0] = Math.Atan(arg[0]);

            // Apply chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (!arg[i].Equals(0.0))
                    result[i] = arg[i] / (1 + Square(arg[0]));
            }
            return result;
        }

        /// <summary>
        /// Miscellaneous
        /// </summary>
        public static DoubleDerivatives ApplyAbs(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new DoubleDerivatives(arg.Count);
            result[0] = Math.Abs(arg[0]);

            // Apply chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (!arg[i].Equals(0.0))
                    result[i] = arg[i] * (arg[0] > 0 ? 1 : arg[0] < 0 ? -1 : 0);
            }
            return result;
        }
        public static DoubleDerivatives ApplyRound(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            var arg = arguments[0];
            if (arguments.Length == 1)
            {
                var result = new DoubleDerivatives();
                result[0] = Math.Round(arg[0]);
                
                for (var i = 1; i < arg.Count; i++)
                    if (!arg[i].Equals(0.0))
                        throw new CircuitException("Cannot differentiate Round()");
                return result;
            }
            if (arguments.Length == 2)
            {
                var result = new DoubleDerivatives();
                result[0] = Math.Round(arg[0], (int)Math.Round(arguments[1][0]));

                for (var i = 1; i < arg.Count; i++)
                    if (!arg[i].Equals(0.0))
                        throw new CircuitException("Cannot differentiate Round()");
                for (var i = 1; i < arguments[1].Count; i++)
                    if (!arguments[1][i].Equals(0.0))
                        throw new CircuitException("Cannot differentiate Round()");
                return result;
            }
            throw new CircuitException("Invalid number of arguments for Round()");
        }
        public static DoubleDerivatives ApplyMin(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            var result = new DoubleDerivatives();
            var min = arguments[0][0];
            for (var i = 1; i < arguments.Length; i++)
            {
                min = Math.Min(min, arguments[i][0]);
                for (var k = 1; k < arguments[i].Count; k++)
                    if (!arguments[i][k].Equals(0.0))
                        throw new CircuitException("Cannot differentiate Min()");
            }
            result[0] = min;
            return result;
        }
        public static DoubleDerivatives ApplyMax(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            var result = new DoubleDerivatives();
            var min = arguments[0][0];
            for (var i = 1; i < arguments.Length; i++)
            {
                min = Math.Max(min, arguments[i][0]);
                for (var k = 1; k < arguments[i].Count; k++)
                    if (!arguments[i][k].Equals(0.0))
                        throw new CircuitException("Cannot differentiate Min()");
            }
            result[0] = min;
            return result;
        }

        /// <summary>
        /// Square a number
        /// </summary>
        /// <param name="x">The argument.</param>
        /// <returns></returns>
        public static double Square(double x) => x * x;

        /// <summary>
        /// Delegate for applying a function.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns></returns>
        public delegate DoubleDerivatives DoubleDerivativesFunction(DoubleDerivatives[] arguments);
    }
}
