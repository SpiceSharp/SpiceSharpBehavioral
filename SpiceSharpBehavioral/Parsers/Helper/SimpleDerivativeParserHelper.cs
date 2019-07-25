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
            var a0 = arg[0];
            result[0] = () => Math.Exp(a0());

            // Chain rule for derivatives
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                {
                    var ai = arg[i];
                    result[i] = () => Math.Exp(a0()) * ai();
                }
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
                var a0 = arg[0];
                result[0] = () => Math.Log(a0());

                // Chain rule
                for (var i = 1; i < arg.Count; i++)
                {
                    if (arg[i] != null)
                    {
                        var ai = arg[i];
                        result[i] = () => ai() / a0();
                    }
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
                var g0 = g[0];
                var f0 = f[0];
                result[0] = () => Math.Log(g0(), f0());

                // Chain rule
                for (var i = 1; i < size; i++)
                {
                    if (g[i] != null && f[i] != null)
                    {
                        var gi = g[i];
                        var fi = f[i];
                        result[i] = () =>
                        {
                            var tmpf0 = f0();
                            var tmpg0 = g0();
                            return gi() / tmpg0 / Math.Log(tmpf0) - fi() / tmpf0 * Math.Log(tmpg0, tmpf0);
                        };
                    }
                    else if (g[i] != null)
                    {
                        var gi = g[i];
                        result[i] = () => gi() / g0() / Math.Log(f0());
                    }
                    else if (f[i] != null)
                    {
                        var fi = f[i];
                        result[i] = () =>
                        {
                            var tmpf0 = f0();
                            return -fi() / f0() * Math.Log(g0(), tmpf0);
                        };
                    }
                }
                return result;
            }

            throw new CircuitException("Invalid number of arguments, {0} given but 2 expected".FormatString(arguments.Length));
        }
        public static DoubleDerivatives ApplyLog10(DoubleDerivatives[] arguments)
        {
            var result = ApplyLog(arguments);
            for (var i = 0; i < arguments[0].Count; i++)
            {
                var arg = result[i];
                result[i] = () => arg() / Math.Log(10.0);
            }
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
            return f.Pow(g);
        }

        /// <summary>
        /// Square root method.
        /// </summary>
        public static DoubleDerivatives ApplySqrt(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var a = arguments[0];
            var result = new DoubleDerivatives(a.Count);
            var a0 = a[0];
            result[0] = () => Math.Sqrt(a0());

            // Apply the chain rule
            for (var i = 1; i < a.Count; i++)
            {
                if (a[i] != null)
                {
                    var ai = a[i];
                    result[i] = () => -0.5 * ai() / Math.Sqrt(a0());
                }
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
            var a0 = arg[0];
            result[0] = () => Math.Sin(a0());

            // Apply the chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                {
                    var ai = arg[i];
                    result[i] = () => Math.Cos(a0()) * ai();
                }
            }
            return result;
        }
        private static DoubleDerivatives ApplyCos(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new DoubleDerivatives(arg.Count);
            var a0 = arg[0];
            result[0] = () => Math.Cos(a0());

            // Apply the chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                {
                    var ai = arg[i];
                    result[i] = () => -Math.Sin(a0()) * ai();
                }
            }
            return result;
        }
        private static DoubleDerivatives ApplyTan(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new DoubleDerivatives(arg.Count);
            var a0 = arg[0];
            result[0] = () => Math.Tan(a0());

            // Apply the chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                {
                    var ai = arg[i];
                    result[i] = () =>
                    {
                        var tmp = Math.Cos(a0());
                        return ai() / tmp / tmp;
                    };
                }
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
            var a0 = arg[0];
            result[0] = () => Math.Asin(a0());

            // Apply the chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                {
                    var ai = arg[i];
                    result[i] = () =>
                    {
                        var x = a0();
                        return ai() / Math.Sqrt(1 - x * x);
                    };
                }
            }
            return result;
        }
        public static DoubleDerivatives ApplyAcos(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new DoubleDerivatives(arg.Count);
            var a0 = arg[0];
            result[0] = () => Math.Acos(a0());

            // Apply the chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                {
                    var ai = arg[i];
                    result[i] = () =>
                    {
                        var x = a0();
                        return -ai() / Math.Sqrt(1 - x * x);
                    };
                }
            }
            return result;
        }
        public static DoubleDerivatives ApplyAtan(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new DoubleDerivatives(arg.Count);
            var a0 = arg[0];
            result[0] = () => Math.Atan(a0());

            // Apply the chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                {
                    var ai = arg[i];
                    result[i] = () =>
                    {
                        var x = a0();
                        return ai() / (1 + x * x);
                    };
                }
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
            var a0 = arg[0];
            result[0] = () => Math.Abs(a0());

            // Apply chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                {
                    var ai = arg[i];
                    result[i] = () =>
                    {
                        var tmp = a0();
                        return tmp > 0 ? 1 : tmp < 0 ? -1 : 0;
                    };
                }
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
                var a0 = arg[0];
                result[0] = () => Math.Round(a0());
                
                for (var i = 1; i < arg.Count; i++)
                    if (!arg[i].Equals(0.0))
                        throw new CircuitException("Cannot differentiate Round()");
                return result;
            }
            if (arguments.Length == 2)
            {
                var result = new DoubleDerivatives();
                var a0 = arg[0];
                var b0 = arguments[1][0];
                result[0] = () => Math.Round(a0(), (int)Math.Round(b0()));

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
            if (arguments.Length == 1)
            {
                result[0] = arguments[0][0];
                return result;
            }

            var a = arguments[0][0];
            var b = arguments[1][0];
            Func<double> c = () => Math.Min(a(), b());
            for (var i = 2; i < arguments.Length; i++)
            {
                a = c;
                b = arguments[i][0];
                c = () => Math.Min(a(), b());
                for (var k = 1; k < arguments[i].Count; k++)
                    if (!arguments[i][k].Equals(0.0))
                        throw new CircuitException("Cannot differentiate Min()");
            }
            result[0] = c;
            return result;
        }
        public static DoubleDerivatives ApplyMax(DoubleDerivatives[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            var result = new DoubleDerivatives();
            if (arguments.Length == 1)
            {
                result[0] = arguments[0][0];
                return result;
            }

            var a = arguments[0][0];
            var b = arguments[1][0];
            Func<double> c = () => Math.Max(a(), b());
            for (var i = 2; i < arguments.Length; i++)
            {
                a = c;
                b = arguments[i][0];
                c = () => Math.Max(a(), b());
                for (var k = 1; k < arguments[i].Count; k++)
                    if (!arguments[i][k].Equals(0.0))
                        throw new CircuitException("Cannot differentiate Min()");
            }
            result[0] = c;
            return result;
        }

        /// <summary>
        /// Delegate for applying a function.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns></returns>
        public delegate DoubleDerivatives DoubleDerivativesFunction(DoubleDerivatives[] arguments);
    }
}
