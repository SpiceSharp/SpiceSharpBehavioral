using SpiceSharp;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.Helper
{
    /// <summary>
    /// Helper methods for a <see cref="SimpleParser"/>.
    /// </summary>
    public static class SimpleParserHelper
    {
        /// <summary>
        /// Gets or sets the default functions.
        /// </summary>
        /// <value>
        /// The default functions.
        /// </value>
        public static Dictionary<string, SimpleFunction> DefaultFunctions { get; set; } = new Dictionary<string, SimpleFunction>()
        {
            { "Exp", ApplyExp },
            { "Log", ApplyLog },
            { "Ln", ApplyLog },
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
        public static void RegisterDefaultFunctions(this SimpleParser parser)
        {
            parser.FunctionFound += DefaultFunctionFound;
        }

        /// <summary>
        /// Remove the default functions from the parser.
        /// </summary>
        /// <param name="parser">The parser.</param>
        public static void UnregisterDefaultFunctions(this SimpleParser parser)
        {
            parser.FunctionFound -= DefaultFunctionFound;
        }

        /// <summary>
        /// Default functions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DefaultFunctionFound(object sender, FunctionFoundEventArgs<double> e)
        {
            if (DefaultFunctions.TryGetValue(e.Name, out var function))
            {
                var arguments = new double[e.ArgumentCount];
                for (var i = 0; i < e.ArgumentCount; i++)
                    arguments[i] = e[i];
                e.Result = function?.Invoke(arguments) ?? e.Result;
            }
        }

        /// <summary>
        /// Exponentials
        /// </summary>
        public static double ApplyExp(double[] arguments) => Math.Exp(arguments.ThrowIfNot(nameof(arguments), 1)[0]);

        /// <summary>
        /// Applies the logarithm.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The logarithm result.</returns>
        public static double ApplyLog(double[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            if (arguments.Length == 1)
                return Math.Log(arguments[0]);
            if (arguments.Length == 2)
                return Math.Log(arguments[0], arguments[1]);
            throw new CircuitException("Invalid number of arguments, {0} given but 2 expected".FormatString(arguments.Length));
        }

        /// <summary>
        /// Applies the base-10 logarithm.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The logarithm result.</returns>
        public static double ApplyLog10(double[] arguments) => Math.Log10(arguments.ThrowIfNot(nameof(arguments), 1)[0]);

        /// <summary>
        /// Raises to a power.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The power.</returns>
        public static double ApplyPow(double[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 2);
            return Math.Pow(arguments[0], arguments[1]);
        }

        /// <summary>
        /// Applies the square root.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The square root result.</returns>
        public static double ApplySqrt(double[] arguments) => Math.Sqrt(arguments.ThrowIfNot(nameof(arguments), 1)[0]);

        /// <summary>
        /// Applies the sine.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The sine result.</returns>
        public static double ApplySin(double[] arguments) => Math.Sin(arguments.ThrowIfNot(nameof(arguments), 1)[0]);

        /// <summary>
        /// Applies the cosine.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The cosine result.</returns>
        private static double ApplyCos(double[] arguments) => Math.Cos(arguments.ThrowIfNot(nameof(arguments), 1)[0]);

        /// <summary>
        /// Applies the tangent.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The tangent result.</returns>
        private static double ApplyTan(double[] arguments) => Math.Tan(arguments.ThrowIfNot(nameof(arguments), 1)[0]);

        /// <summary>
        /// Applies the arcsine.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The arcsine result.</returns>
        public static double ApplyAsin(double[] arguments) => Math.Asin(arguments.ThrowIfNot(nameof(arguments), 1)[0]);

        /// <summary>
        /// Applies the arccosine.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The arccosine result.</returns>
        public static double ApplyAcos(double[] arguments) => Math.Acos(arguments.ThrowIfNot(nameof(arguments), 1)[0]);

        /// <summary>
        /// Applies the arctangent.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The arctangent result.</returns>
        public static double ApplyAtan(double[] arguments) => Math.Atan(arguments.ThrowIfNot(nameof(arguments), 1)[0]);

        /// <summary>
        /// Applies the absolute value.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The absolute value result.</returns>
        public static double ApplyAbs(double[] arguments) => Math.Abs(arguments.ThrowIfNot(nameof(arguments), 1)[0]);

        /// <summary>
        /// Rounds the value.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The rounded result.</returns>
        public static double ApplyRound(double[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            if (arguments.Length == 1)
                return Math.Round(arguments[0]);
            if (arguments.Length == 2)
                return Math.Round(arguments[0], (int)Math.Round(arguments[1]));
            throw new CircuitException("Invalid number of arguments for Round()");
        }

        /// <summary>
        /// Applies the minimum.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The minimum result.</returns>
        public static double ApplyMin(double[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            var min = arguments[0];
            for (var i = 1; i < arguments.Length; i++)
                min = Math.Min(min, arguments[i]);
            return min;
        }

        /// <summary>
        /// Applies the maximum.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The maximum result.</returns>
        public static double ApplyMax(double[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            var max = arguments[0];
            for (var i = 1; i < arguments.Length; i++)
                max = Math.Max(max, arguments[i]);
            return max;
        }

        /// <summary>
        /// A delegate for applying simple functions.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The result of the function.</returns>
        public delegate double SimpleFunction(double[] arguments);
    }
}
