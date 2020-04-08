using SpiceSharp;
using SpiceSharpBehavioral.Diagnostics;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// Helper methods for a <see cref="DoubleBuilder"/>.
    /// </summary>
    public static class DoubleBuilderHelper
    {
        /// <summary>
        /// A set of default functions.
        /// </summary>
        public static readonly Dictionary<string, Func<double[], double>> Defaults = new Dictionary<string, Func<double[], double>>
        {
            { "abs", Abs },
            { "sgn", Sgn },
            { "sqrt", Sqrt },
            { "pwr", Pwr },
            { "log", Log },
            { "log10", Log10 },
            { "exp", Exp },
            { "sin", Sin },
            { "cos", Cos },
            { "tan", Tan },
            { "sinh", Sinh },
            { "cosh", Cosh },
            { "tanh", Tanh },
            { "asin", Asin },
            { "acos", Acos },
            { "atan", Atan },
            { "u", U },
            { "u2", U2 },
            { "uramp", URamp },
            { "ceil", Ceil },
            { "floor", Floor },
            { "nint", Nint }, 
            { "round", Round },
            { "square", Square },
            { "pwl", Pwl },
            { "min", Min },
            { "max", Max }
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
        /// <param name="definitions">The definitions.</param>
        public static void RegisterDefaultFunctions(this Dictionary<string, Func<double[], double>> definitions)
        {
            definitions.ThrowIfNull(nameof(definitions));
            foreach (var pair in Defaults)
                definitions.Add(pair.Key, pair.Value);
        }

        private static double Zero(double[] args) => 0.0;
        private static double Abs(double[] args) => Math.Abs(args.Check(1)[0]);
        private static double Sgn(double[] args) => Math.Sign(args.Check(1)[0]);
        private static double Sqrt(double[] args) => Functions.Sqrt(args.Check(1)[0]);
        private static double URamp(double[] args) => Functions.Ramp(args.Check(1)[0]);
        private static double U(double[] args) => Functions.Step(args.Check(1)[0]);
        private static double U2(double[] args) => Functions.Step2(args.Check(1)[0]);
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
        private static double Log(double[] args) => Functions.Log(args.Check(1)[0]);
        private static double Log10(double[] args) => Functions.Log10(args.Check(1)[0]);
        private static double Square(double[] args) { var x = args.Check(1)[0]; return x * x; }
        private static double Nint(double[] args) => Math.Round(args.Check(1)[0], 0);

        // Two-argument functions
        private static double Pwr(double[] args) { args.Check(2); return Functions.Power2(args[0], args[1]); }
        private static double Min(double[] args) { args.Check(2); return Math.Min(args[0], args[1]); }
        private static double Max(double[] args) { args.Check(2); return Math.Max(args[0], args[1]); }
        private static double Round(double[] args) { args.Check(2); return Math.Round(args[0], (int)args[1]); }

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
            return Functions.Pwl(args[0], data);
        }
    }
}
