using SpiceSharp;
using SpiceSharpBehavioral.Diagnostics;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// Helper methods for a <see cref="RealBuilder"/>.
    /// </summary>
    public static class ComplexBuilderHelper
    {
        private static readonly Random _rnd = new Random();

        /// <summary>
        /// A set of default functions.
        /// </summary>
        public static readonly Dictionary<string, Func<Complex[], Complex>> Defaults = new Dictionary<string, Func<Complex[], Complex>>
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
            { "hypot", Hypot },
            { "rnd", Random }, { "rand", Random },
            { "if", If }
        };

        private static Complex[] Check(this Complex[] args, int expected)
        {
            if (args == null || args.Length != expected)
                throw new ArgumentMismatchException(expected, args?.Length ?? 0);
            return args;
        }

        /// <summary>
        /// Registers the default functions.
        /// </summary>
        /// <param name="definitions">The definitions.</param>
        public static void RegisterDefaultFunctions(this Dictionary<string, Func<Complex[], Complex>> definitions)
        {
            definitions.ThrowIfNull(nameof(definitions));
            foreach (var pair in Defaults)
                definitions.Add(pair.Key, pair.Value);
        }

        // No-argument functions
        private static Complex Random(Complex[] args) { args.Check(0); return _rnd.NextDouble(); }

        // One-argument functions
        private static Complex Abs(Complex[] args) => args.Check(1)[0].Magnitude;
        private static Complex Sgn(Complex[] args) => Math.Sign(args.Check(1)[0].Real);
        private static Complex Sqrt(Complex[] args) => Functions.Sqrt(args.Check(1)[0]);
        private static Complex URamp(Complex[] args) => Functions.Ramp(args.Check(1)[0]);
        private static Complex U(Complex[] args) => Functions.Step(args.Check(1)[0]);
        private static Complex U2(Complex[] args) => Functions.Step2(args.Check(1)[0]);
        private static Complex Sin(Complex[] args) => Complex.Sin(args.Check(1)[0]);
        private static Complex Cos(Complex[] args) => Complex.Cos(args.Check(1)[0]);
        private static Complex Tan(Complex[] args) => Complex.Tan(args.Check(1)[0]);
        private static Complex Asin(Complex[] args) => Complex.Asin(args.Check(1)[0]);
        private static Complex Acos(Complex[] args) => Complex.Acos(args.Check(1)[0]);
        private static Complex Atan(Complex[] args) => Complex.Atan(args.Check(1)[0]);
        private static Complex Sinh(Complex[] args) => Complex.Sinh(args.Check(1)[0]);
        private static Complex Cosh(Complex[] args) => Complex.Cosh(args.Check(1)[0]);
        private static Complex Tanh(Complex[] args) => Complex.Tanh(args.Check(1)[0]);
        private static Complex Ceil(Complex[] args)
        {
            var arg = args.Check(1)[0];
            return new Complex(Math.Ceiling(arg.Real), Math.Ceiling(arg.Imaginary));
        }
        private static Complex Floor(Complex[] args)
        {
            var arg = args.Check(1)[0];
            return new Complex(Math.Floor(arg.Real), Math.Floor(arg.Imaginary));
        }
        private static Complex Exp(Complex[] args) => Complex.Exp(args.Check(1)[0]);
        private static Complex Log(Complex[] args) => Functions.Log(args.Check(1)[0]);
        private static Complex Log10(Complex[] args) => Functions.Log10(args.Check(1)[0]);
        private static Complex Square(Complex[] args) { var x = args.Check(1)[0]; return x * x; }
        private static Complex Nint(Complex[] args)
        {
            var arg = args.Check(1)[0];
            return new Complex(Math.Round(arg.Real, 0), Math.Round(arg.Imaginary, 0));
        }

        // Two-argument functions
        private static Complex Pow(Complex[] args) { args.Check(2); return Complex.Pow(args[0], args[1]); }
        private static Complex Pwr(Complex[] args) { args.Check(2); return Functions.Power(args[0], args[1]); }
        private static Complex Pwrs(Complex[] args) { args.Check(2); return Functions.Power2(args[0], args[1]); }
        private static Complex Min(Complex[] args) { args.Check(2); return Math.Min(args[0].Real, args[1].Real); }
        private static Complex Max(Complex[] args) { args.Check(2); return Math.Max(args[0].Real, args[1].Real); }
        private static Complex Round(Complex[] args)
        {
            var arg = args.Check(2)[0];
            var n = (int)args[1].Real;
            return new Complex(Math.Round(arg.Real, n), Math.Round(arg.Imaginary, n));
        }
        private static Complex Atan2(Complex[] args) { args.Check(2); return Math.Atan2(args[0].Real, args[1].Real); }
        private static Complex Hypot(Complex[] args) { args.Check(2); return Functions.Hypot(args[0], args[1]); }

        // Three-argument functions
        private static Complex If(Complex[] args) { args.Check(3); return args[0].Real > 0.5 ? args[1] : args[2]; }

        // N-argument functions
        private static Complex Pwl(Complex[] args)
        {
            if (args.Length < 3)
                throw new ArgumentMismatchException(3, args.Length);
            int points = (args.Length - 1) / 2;
            if (args.Length % 2 == 0)
                throw new ArgumentMismatchException(points * 2 + 1, args.Length);

            var data = new Point[points];
            for (var i = 0; i < points; i++)
                data[i] = new Point(args[i * 2 + 1].Real, args[i * 2 + 2].Real);
            return Functions.Pwl(args[0].Real, data);
        }
        private static Complex PwlDerivative(Complex[] args)
        {
            if (args.Length < 3)
                throw new ArgumentMismatchException(3, args.Length);
            int points = (args.Length - 1) / 2;
            if (args.Length % 2 == 0)
                throw new ArgumentMismatchException(points * 2 + 1, args.Length);

            var data = new Point[points];
            for (var i = 0; i < points; i++)
                data[i] = new Point(args[i * 2 + 1].Real, args[i * 2 + 2].Real);
            return Functions.PwlDerivative(args[0].Real, data);
        }
    }
}
