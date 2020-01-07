using SpiceSharpBehavioral.Parsers.Operators;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.DoubleFunc
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
        public static void FunctionFound(object sender, FunctionFoundEventArgs<Func<double>> e)
        {
            if (_functions.TryGetValue(e.Name, out var description))
                e.Result = description(e.Arguments);
        }

        private static Func<double> Abs(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 1)
            {
                var arg = arguments[0];
                return () => Math.Abs(arg());
            }
            throw new Exception();
        }
        private static Func<double> Sgn(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 1)
            {
                var arg = arguments[0];
                return () => Math.Sign(arg());
            }
            throw new Exception();
        }
        private static Func<double> Sqrt(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 1)
            {
                var arg = arguments[0];
                return () => Math.Sqrt(arg());
            }
            throw new Exception();
        }
        private static Func<double> Exp(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 1)
            {
                var arg = arguments[0];
                return () => Math.Exp(arg());
            }
            throw new Exception();
        }
        private static Func<double> Log(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 1)
            {
                var arg = arguments[0];
                return () => Math.Log(arg());
            }
            throw new Exception();
        }
        private static Func<double> Log10(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 1)
            {
                var arg = arguments[0];
                return () => Math.Log10(arg());
            }
            throw new Exception();
        }
        private static Func<double> Round(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 1)
            {
                var arg = arguments[0];
                return () => Math.Round(arg());
            }
            else if (arguments.Count == 2)
            {
                var arg = arguments[0];
                var second = arguments[1];
                return () => Math.Round(arg(), (int)(second() + 0.1));
            }
            throw new Exception();
        }
        private static Func<double> Sin(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 1)
            {
                var arg = arguments[0];
                return () => Math.Sin(arg());
            }
            throw new Exception();
        }
        private static Func<double> Cos(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 1)
            {
                var arg = arguments[0];
                return () => Math.Cos(arg());
            }
            throw new Exception();
        }
        private static Func<double> Tan(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 1)
            {
                var arg = arguments[0];
                return () => Math.Tan(arg());
            }
            throw new Exception();
        }
        private static Func<double> Asin(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 1)
            {
                var arg = arguments[0];
                return () => Math.Asin(arg());
            }
            throw new Exception();
        }
        private static Func<double> Acos(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 1)
            {
                var arg = arguments[0];
                return () => Math.Acos(arg());
            }
            throw new Exception();
        }
        private static Func<double> Atan(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 1)
            {
                var arg = arguments[0];
                return () => Math.Atan(arg());
            }
            throw new Exception();
        }
        private static Func<double> Sinh(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 1)
            {
                var arg = arguments[0];
                return () => Math.Sinh(arg());
            }
            throw new Exception();
        }
        private static Func<double> Cosh(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 1)
            {
                var arg = arguments[0];
                return () => Math.Cosh(arg());
            }
            throw new Exception();
        }
        private static Func<double> Tanh(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 1)
            {
                var arg = arguments[0];
                return () => Math.Tanh(arg());
            }
            throw new Exception();
        }
        private static Func<double> Max(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 0)
                throw new Exception();
            var arg = arguments[0];
            var r = arg;
            for (var i = 1; i < arguments.Count; i++)
            {
                // We need to make sure that our lambda expression uses its own local variable
                var arg1 = r;
                var arg2 = arguments[i];
                r = () => Math.Max(arg1(), arg2());
            }
            return r;
        }
        private static Func<double> Min(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 0)
                throw new Exception();
            var arg = arguments[0];
            var r = arg;
            for (var i = 1; i < arguments.Count; i++)
            {
                // We need to make sure that our lambda expression uses its own local variable
                var arg1 = r;
                var arg2 = arguments[i];
                r = () => Math.Min(arg1(), arg2());
            }
            return r;
        }
        private static Func<double> Pow(IReadOnlyList<Func<double>> arguments)
        {
            if (arguments.Count == 2)
            {
                var arg = arguments[0];
                var arg2 = arguments[1];
                return () => Math.Pow(arg(), arg2());
            }
            throw new Exception();
        }

        /// <summary>
        /// A function description.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns></returns>
        public delegate Func<double> FunctionDescription(IReadOnlyList<Func<double>> arguments);
    }
}
