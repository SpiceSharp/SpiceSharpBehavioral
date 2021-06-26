using SpiceSharp;
using SpiceSharpBehavioral.Diagnostics;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Builders.Direct
{
    /// <summary>
    /// Helper methods for a <see cref="RealBuilder"/>.
    /// </summary>
    public static class RealBuilderHelper
    {
        // Random generator
        private static readonly Random _rnd = new Random();

        /// <summary>
        /// A set of default functions.
        /// </summary>
        public static Dictionary<string, Func<double[], double>> Defaults { get; set; } = new Dictionary<string, Func<double[], double>>()
        {
            { "abs", Abs },
            { "sgn", Sgn },
            { "sqrt", Sqrt },
            { "pow", Pow },
            { "pwr", Pwr },
            { "pwrs", Pwrs },
            { "log", Log }, { "ln", Log },
            { "log10", Log10 },
            { "exp", Exp },
            { "sin", Sin },
            { "cos", Cos },
            { "tan", Tan },
            { "sinh", Sinh },
            { "cosh", Cosh },
            { "tanh", Tanh },
            { "asin", Asin }, { "arcsin", Asin },
            { "acos", Acos }, { "arccos", Acos },
            { "atan", Atan }, { "arctan", Atan },
            { "u", U },
            { "u2", U2 },
            { "uramp", URamp },
            { "ceil", Ceil },
            { "floor", Floor },
            { "nint", Nint },
            { "round", Round },
            { "square", Square },
            { "pwl", Pwl }, { "dpwl(0)", PwlDerivative },
            { "table", Pwl }, { "dtable(0)", PwlDerivative },
            { "tbl", Pwl }, { "dtbl(0)", PwlDerivative },
            { "min", Min },
            { "max", Max },
            { "atan2", Atan2 },
            { "atanh", Atanh },
            { "hypot", Hypot },
            { "rnd", Random }, { "rand", Random },
            { "if", If },
            { "limit", Limit },
            { "real", args => args.Check(1)[0] },
            { "imag", args => 0.0 },
            { "arg", args => 0.0 },
            { "db", args => 20 * Log10(args) }
        };

        private static double[] Check(this double[] args, int expected)
        {
            if (args == null || args.Length != expected)
                throw new ArgumentMismatchException(expected, args?.Length ?? 0);
            return args;
        }

        /// <summary>
        /// Registers the default functions.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> is <c>null</c>.</exception>
        public static void RegisterDefaultFunctions(this IDirectBuilder<double> builder)
        {
            builder.ThrowIfNull(nameof(builder));
            builder.FunctionFound += OnFunctionFound;
        }

        private static void OnFunctionFound(object sender, FunctionFoundEventArgs<double> args)
        {
            if (!args.Created && Defaults.TryGetValue(args.Function.Name, out var definition))
            {
                var arguments = new double[args.Function.Arguments.Count];
                for (var i = 0; i < arguments.Length; i++)
                    arguments[i] = args.Builder.Build(args.Function.Arguments[i]);
                args.Result = definition(arguments);
            }
        }

        /// <summary>
        /// Helper methods that changes the equality comparer for function names.
        /// </summary>
        /// <param name="comparer">The name comparer.</param>
        public static void RemapFunctions(IEqualityComparer<string> comparer)
        {
            var nmap = new Dictionary<string, Func<double[], double>>(comparer);
            foreach (var map in Defaults)
            {
                nmap.Add(map.Key, map.Value);
            }
            Defaults = nmap;
        }

        // No-argument functions
        private static double Random(double[] args) { args.Check(0); return _rnd.NextDouble(); }

        // One-argument functions
        private static double Abs(double[] args) => Math.Abs(args.Check(1)[0]);
        private static double Sgn(double[] args) => Math.Sign(args.Check(1)[0]);
        private static double Sqrt(double[] args) => HelperFunctions.Sqrt(args.Check(1)[0]);
        private static double URamp(double[] args) => HelperFunctions.Ramp(args.Check(1)[0]);
        private static double U(double[] args) => HelperFunctions.Step(args.Check(1)[0]);
        private static double U2(double[] args) => HelperFunctions.Step2(args.Check(1)[0]);
        private static double Sin(double[] args) => Math.Sin(args.Check(1)[0]);
        private static double Cos(double[] args) => Math.Cos(args.Check(1)[0]);
        private static double Tan(double[] args) => Math.Tan(args.Check(1)[0]);
        private static double Asin(double[] args) => Math.Asin(args.Check(1)[0]);
        private static double Acos(double[] args) => Math.Acos(args.Check(1)[0]);
        private static double Atan(double[] args) => Math.Atan(args.Check(1)[0]);
        private static double Sinh(double[] args) => Math.Sinh(args.Check(1)[0]);
        private static double Cosh(double[] args) => Math.Cosh(args.Check(1)[0]);
        private static double Tanh(double[] args) => Math.Tanh(args.Check(1)[0]);
        private static double Ceil(double[] args) => Math.Ceiling(args.Check(1)[0]);
        private static double Floor(double[] args) => Math.Floor(args.Check(1)[0]);
        private static double Exp(double[] args) => Math.Exp(args.Check(1)[0]);
        private static double Log(double[] args) => HelperFunctions.Log(args.Check(1)[0]);
        private static double Log10(double[] args) => HelperFunctions.Log10(args.Check(1)[0]);
        private static double Square(double[] args) { var x = args.Check(1)[0]; return x * x; }
        private static double Nint(double[] args) => Math.Round(args.Check(1)[0], 0);

        // Two-argument functions
        private static double Pow(double[] args) { args.Check(2); return HelperFunctions.Pow(args[0], args[1]); }
        private static double Pwr(double[] args) { args.Check(2); return HelperFunctions.Power(args[0], args[1]); }
        private static double Pwrs(double[] args) { args.Check(2); return HelperFunctions.Power2(args[0], args[1]); }
        private static double Min(double[] args) { args.Check(2); return Math.Min(args[0], args[1]); }
        private static double Max(double[] args) { args.Check(2); return Math.Max(args[0], args[1]); }
        private static double Round(double[] args) { args.Check(2); return Math.Round(args[0], (int)args[1]); }
        private static double Atan2(double[] args) { args.Check(2); return Math.Atan2(args[0], args[1]); }
        private static double Atanh(double[] args) { args.Check(1); return HelperFunctions.Atanh(args[0]); }
        private static double Hypot(double[] args) { args.Check(2); return HelperFunctions.Hypot(args[0], args[1]); }

        // Three-argument functions
        private static double If(double[] args) { args.Check(3); return args[0] > 0.5 ? args[1] : args[2]; }
        private static double Limit(double[] args) { args.Check(3); return HelperFunctions.Limit(args[0], args[1], args[2]); }

        // N-argument functions
        private static double Pwl(double[] args)
        {
            if (args.Length < 3)
                throw new ArgumentMismatchException(3, args.Length);
            int points = (args.Length - 1) / 2;
            if (args.Length % 2 == 0)
                throw new ArgumentMismatchException(points * 2 + 1, args.Length);

            var data = new Point[points];
            for (var i = 0; i < points; i++)
                data[i] = new Point(args[i * 2 + 1], args[i * 2 + 2]);
            return HelperFunctions.Pwl(args[0], data);
        }
        private static double PwlDerivative(double[] args)
        {
            if (args.Length < 3)
                throw new ArgumentMismatchException(3, args.Length);
            int points = (args.Length - 1) / 2;
            if (args.Length % 2 == 0)
                throw new ArgumentMismatchException(points * 2 + 1, args.Length);

            var data = new Point[points];
            for (var i = 0; i < points; i++)
                data[i] = new Point(args[i * 2 + 1], args[i * 2 + 2]);
            return HelperFunctions.PwlDerivative(args[0], data);
        }
    }
}
