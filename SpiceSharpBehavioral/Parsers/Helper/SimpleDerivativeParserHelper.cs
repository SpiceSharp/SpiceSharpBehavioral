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
        private static void DefaultFunctionFound(object sender, FunctionFoundEventArgs<Derivatives<Func<double>>> e)
        {
            if (DefaultFunctions.TryGetValue(e.Name, out var function))
            {
                var arguments = new Derivatives<Func<double>>[e.ArgumentCount];
                for (var i = 0; i < e.ArgumentCount; i++)
                    arguments[i] = e[i];
                e.Result = function?.Invoke(arguments);
            }
        }

        /// <summary>
        /// Exponentials
        /// </summary>
        public static Derivatives<Func<double>> ApplyExp(Derivatives<Func<double>>[] arguments)
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
        public static Derivatives<Func<double>> ApplyLog(Derivatives<Func<double>>[] arguments)
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
        public static Derivatives<Func<double>> ApplyLog10(Derivatives<Func<double>>[] arguments)
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
        public static Derivatives<Func<double>> ApplyPow(Derivatives<Func<double>>[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 2);
            var f = arguments[0];
            var g = arguments[1];
            return f.Pow(g);
        }

        /// <summary>
        /// Square root method.
        /// </summary>
        public static Derivatives<Func<double>> ApplySqrt(Derivatives<Func<double>>[] arguments)
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
        public static Derivatives<Func<double>> ApplySin(Derivatives<Func<double>>[] arguments)
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
        private static Derivatives<Func<double>> ApplyCos(Derivatives<Func<double>>[] arguments)
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
        private static Derivatives<Func<double>> ApplyTan(Derivatives<Func<double>>[] arguments)
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
        public static Derivatives<Func<double>> ApplyAsin(Derivatives<Func<double>>[] arguments)
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
        public static Derivatives<Func<double>> ApplyAcos(Derivatives<Func<double>>[] arguments)
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
        public static Derivatives<Func<double>> ApplyAtan(Derivatives<Func<double>>[] arguments)
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
        public static Derivatives<Func<double>> ApplyAbs(Derivatives<Func<double>>[] arguments)
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
        public static Derivatives<Func<double>> ApplyRound(Derivatives<Func<double>>[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            var arg = arguments[0];
            if (arguments.Length == 1)
            {
                var result = new DoubleDerivatives();
                var a0 = arg[0];
                result[0] = () => Math.Round(a0());

                // The derivative of Round() is always 0 (horizontal), but doesn't exist depending on where it is rounded
                for (var i = 1; i < arg.Count; i++)
                    if (arg[i] != null)
                        CircuitWarning.Warning(null, "Trying to derive Round() for which the derivative may not exist in some points");
                return result;
            }
            if (arguments.Length == 2)
            {
                var result = new DoubleDerivatives();
                var a0 = arg[0];
                var b0 = arguments[1][0];
                result[0] = () => Math.Round(a0(), (int)Math.Round(b0()));

                // The derivative of Round() is always 0 (horizontal), but doesn't exist depending on where it is rounded
                for (var i = 1; i < arg.Count; i++)
                    if (arg[i] != null)
                        CircuitWarning.Warning(null, "Trying to derive Round() to the first argument for which the derivative may not exist in some points");
                for (var i = 1; i < arguments[1].Count; i++)
                    if (arguments[1][i] != null)
                        CircuitWarning.Warning(null, "Trying to derive Round() to the second argument for which the derivative may not exist in some points");
                return result;
            }
            throw new CircuitException("Invalid number of arguments for Round()");
        }
        public static Derivatives<Func<double>> ApplyMin(Derivatives<Func<double>>[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            var size = 1;
            for (var i = 0; i < arguments.Length; i++)
                size = Math.Max(size, arguments[i].Count);
            var result = new DoubleDerivatives(size);
            if (arguments.Length == 1)
            {
                result[0] = arguments[0][0];
                return result;
            }

            {
                // First two arguments
                var a = arguments[0][0];
                var b = arguments[1][0];
                result[0] = () => Math.Min(a(), b());
                for (var k = 1; k < size; k++)
                {
                    if (arguments[0][k] != null || arguments[1][k] != null)
                    {
                        CircuitWarning.Warning(null, "Trying to derive Min() for which the derivative may not exist in some points");
                        var tmpda = arguments[0][k];
                        var tmpdb = arguments[1][k];
                        var funca = arguments[0][0];
                        var funcb = arguments[1][0];
                        if (tmpda != null && tmpdb != null)
                            result[k] = () => funca() < funcb() ? tmpda() : tmpdb(); // Use the derivative of the function that is currently the smallest
                        else if (tmpda != null)
                            result[k] = () => funca() < funcb() ? tmpda() : 0;
                        else if (tmpdb != null)
                            result[k] = () => funca() < funcb() ? 0 : tmpdb();
                    }
                }
            }

            for (var i = 2; i < arguments.Length; i++)
            {
                // First two arguments
                var a = result[0];
                var b = arguments[i][0];
                result[0] = () => Math.Min(a(), b());
                for (var k = 1; k < size; k++)
                {
                    if (arguments[0][k] != null || arguments[1][k] != null)
                    {
                        CircuitWarning.Warning(null, "Trying to derive Min() for which the derivative may not exist in some points");
                        var tmpda = result[k];
                        var tmpdb = arguments[i][k];
                        var funca = a;
                        var funcb = arguments[i][0];
                        if (tmpda != null && tmpdb != null)
                            result[k] = () => funca() < funcb() ? tmpda() : tmpdb(); // Use the derivative of the function that is currently the smallest
                        else if (tmpda != null)
                            result[k] = () => funca() < funcb() ? tmpda() : 0;
                        else if (tmpdb != null)
                            result[k] = () => funca() < funcb() ? 0 : tmpdb();
                    }
                }
            }
            return result;
        }
        public static Derivatives<Func<double>> ApplyMax(Derivatives<Func<double>>[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            var size = 1;
            for (var i = 0; i < arguments.Length; i++)
                size = Math.Max(size, arguments[i].Count);
            var result = new DoubleDerivatives(size);
            if (arguments.Length == 1)
            {
                result[0] = arguments[0][0];
                return result;
            }

            {
                // First two arguments
                var a = arguments[0][0];
                var b = arguments[1][0];
                result[0] = () => Math.Max(a(), b());
                for (var k = 1; k < size; k++)
                {
                    if (arguments[0][k] != null || arguments[1][k] != null)
                    {
                        CircuitWarning.Warning(null, "Trying to derive Min() for which the derivative may not exist in some points");
                        var tmpda = arguments[0][k];
                        var tmpdb = arguments[1][k];
                        var funca = arguments[0][0];
                        var funcb = arguments[1][0];
                        if (tmpda != null && tmpdb != null)
                            result[k] = () => funca() > funcb() ? tmpda() : tmpdb(); // Use the derivative of the function that is currently the smallest
                        else if (tmpda != null)
                            result[k] = () => funca() > funcb() ? tmpda() : 0;
                        else if (tmpdb != null)
                            result[k] = () => funca() > funcb() ? 0 : tmpdb();
                    }
                }
            }

            for (var i = 2; i < arguments.Length; i++)
            {
                // First two arguments
                var a = result[0];
                var b = arguments[i][0];
                result[0] = () => Math.Max(a(), b());
                for (var k = 1; k < size; k++)
                {
                    if (arguments[0][k] != null || arguments[1][k] != null)
                    {
                        CircuitWarning.Warning(null, "Trying to derive Min() for which the derivative may not exist in some points");
                        var tmpda = result[k];
                        var tmpdb = arguments[i][k];
                        var funca = a;
                        var funcb = arguments[i][0];
                        if (tmpda != null && tmpdb != null)
                            result[k] = () => funca() > funcb() ? tmpda() : tmpdb(); // Use the derivative of the function that is currently the smallest
                        else if (tmpda != null)
                            result[k] = () => funca() > funcb() ? tmpda() : 0;
                        else if (tmpdb != null)
                            result[k] = () => funca() > funcb() ? 0 : tmpdb();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Delegate for applying a function.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns></returns>
        public delegate Derivatives<Func<double>> DoubleDerivativesFunction(Derivatives<Func<double>>[] arguments);
    }
}
