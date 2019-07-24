using SpiceSharp;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.Helper
{
    public static class SimpleParserHelper
    {
        public static Dictionary<string, SimpleFunction> DefaultFunctions { get; set; } = new Dictionary<string, SimpleFunction>()
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
        /// Logarithms
        /// </summary>
        public static double ApplyLog(double[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            if (arguments.Length == 1)
                return Math.Log(arguments[0]);
            if (arguments.Length == 2)
                return Math.Log(arguments[0], arguments[1]);
            throw new CircuitException("Invalid number of arguments, {0} given but 2 expected".FormatString(arguments.Length));
        }
        public static double ApplyLog10(double[] arguments) => Math.Log10(arguments.ThrowIfNot(nameof(arguments), 1)[0]);

        /// <summary>
        /// Power method.
        /// </summary>
        public static double ApplyPow(double[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 2);
            return Math.Pow(arguments[0], arguments[1]);
        }

        /// <summary>
        /// Square root method.
        /// </summary>
        public static double ApplySqrt(double[] arguments) => Math.Sqrt(arguments.ThrowIfNot(nameof(arguments), 1)[0]);

        /// <summary>
        /// Trigonometry
        /// </summary>
        public static double ApplySin(double[] arguments) => Math.Sin(arguments.ThrowIfNot(nameof(arguments), 1)[0]);
        private static double ApplyCos(double[] arguments) => Math.Cos(arguments.ThrowIfNot(nameof(arguments), 1)[0]);
        private static double ApplyTan(double[] arguments) => Math.Tan(arguments.ThrowIfNot(nameof(arguments), 1)[0]);

        /// <summary>
        /// Inverse trigonometry.
        /// </summary>
        public static double ApplyAsin(double[] arguments) => Math.Asin(arguments.ThrowIfNot(nameof(arguments), 1)[0]);
        public static double ApplyAcos(double[] arguments) => Math.Acos(arguments.ThrowIfNot(nameof(arguments), 1)[0]);
        public static double ApplyAtan(double[] arguments) => Math.Atan(arguments.ThrowIfNot(nameof(arguments), 1)[0]);

        /// <summary>
        /// Miscellaneous
        /// </summary>
        public static double ApplyAbs(double[] arguments) => Math.Abs(arguments.ThrowIfNot(nameof(arguments), 1)[0]);
        public static double ApplyRound(double[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            if (arguments.Length == 1)
                return Math.Round(arguments[0]);
            if (arguments.Length == 2)
                return Math.Round(arguments[0], (int)Math.Round(arguments[1]));
            throw new CircuitException("Invalid number of arguments for Round()");
        }
        public static double ApplyMin(double[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            var min = arguments[0];
            for (var i = 1; i < arguments.Length; i++)
                min = Math.Min(min, arguments[i]);
            return min;
        }
        public static double ApplyMax(double[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            var max = arguments[0];
            for (var i = 1; i < arguments.Length; i++)
                max = Math.Max(max, arguments[i]);
            return max;
        }

        public delegate double SimpleFunction(double[] arguments);
    }
}
