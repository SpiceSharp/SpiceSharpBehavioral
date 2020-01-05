using SpiceSharpBehavioral.Parsers.Operators;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.Double
{
    /// <summary>
    /// Default functions for double parsers.
    /// </summary>
    public static class Defaults
    {
        private static Dictionary<string, FunctionDescription> _functions = new Dictionary<string, FunctionDescription>
        {
            { "abs", Abs }, { "sgn", Sgn }, { "sqrt", Sqrt }, { "exp", Exp },
            { "log", Log }, { "ln", Log }, { "log10", Log10 }, { "round", Round },
            { "sin", Sin }, { "cos", Cos }, { "tan", Tan }, { "asin", Asin },
            { "acos", Acos }, { "atan", Atan }, { "sinh", Sinh }, { "cosh", Cosh },
            { "tanh", Tanh }, { "max", Max }, { "min", Min }, { "pow", Pow }
        };

        /// <summary>
        /// Gets or sets the comparer used for functions.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        public static IEqualityComparer<string> Comparer
        {
            get => _functions.Comparer;
            set
            {
                if (_functions.Comparer != value)
                {
                    var nfunctions = new Dictionary<string, FunctionDescription>(value);
                    foreach (var pair in _functions)
                        nfunctions[pair.Key] = pair.Value;
                }
            }
        }

        /// <summary>
        /// Gets the functions.
        /// </summary>
        /// <value>
        /// The functions.
        /// </value>
        public static IDictionary<string, FunctionDescription> Functions => _functions;

        /// <summary>
        /// Event for default functions.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FunctionFoundEventArgs{Double}"/> instance containing the event data.</param>
        public static void FunctionFound(object sender, FunctionFoundEventArgs<double> e)
        {
            if (_functions.TryGetValue(e.Name, out var description))
                e.Result = description(e.Arguments);
        }

        private static double Abs(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Abs(arguments[0]);
            throw new Exception();
        }
        private static double Sgn(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Sign(arguments[0]);
            throw new Exception();
        }
        private static double Sqrt(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Sqrt(arguments[0]);
            throw new Exception();
        }
        private static double Exp(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Exp(arguments[0]);
            throw new Exception();
        }
        private static double Log(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Log(arguments[0]);
            throw new Exception();
        }
        private static double Log10(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Log10(arguments[0]);
            throw new Exception();
        }
        private static double Round(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Round(arguments[0]);
            else if (arguments.Count == 2)
            {
                var second = (int)(arguments[1] + 0.1);
                return Math.Round(arguments[0], second);
            }
            throw new Exception();
        }
        private static double Sin(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Sin(arguments[0]);
            throw new Exception();
        }
        private static double Cos(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Cos(arguments[0]);
            throw new Exception();
        }
        private static double Tan(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Tan(arguments[0]);
            throw new Exception();
        }
        private static double Asin(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Asin(arguments[0]);
            throw new Exception();
        }
        private static double Acos(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Acos(arguments[0]);
            throw new Exception();
        }
        private static double Atan(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Atan(arguments[0]);
            throw new Exception();
        }
        private static double Sinh(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Sinh(arguments[0]);
            throw new Exception();
        }
        private static double Cosh(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Cosh(arguments[0]);
            throw new Exception();
        }
        private static double Tanh(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Tanh(arguments[0]);
            throw new Exception();
        }
        private static double Max(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 0)
                throw new Exception();
            var r = arguments[0];
            for (var i = 1; i < arguments.Count; i++)
                r = Math.Max(r, arguments[i]);
            return r;
        }
        private static double Min(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 0)
                throw new Exception();
            var r = arguments[0];
            for (var i = 1; i < arguments.Count; i++)
                r = Math.Min(r, arguments[i]);
            return r;
        }
        private static double Pow(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 2)
                return Math.Pow(arguments[0], arguments[1]);
            throw new Exception();
        }

        /// <summary>
        /// A function description.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns></returns>
        public delegate double FunctionDescription(IReadOnlyList<double> arguments);
    }
}
