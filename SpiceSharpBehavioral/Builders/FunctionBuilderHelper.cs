using SpiceSharp;
using SpiceSharpBehavioral.Diagnostics;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// Helper methods for a <see cref="FunctionBuilder"/>.
    /// </summary>
    public static class FunctionBuilderHelper
    {
        private static readonly MethodInfo
            _sgn = ((Func<double, int>)Math.Sign).GetMethodInfo(),
            _round = ((Func<double, int, double>)Math.Round).GetMethodInfo(),
            _pwl = ((Func<double, Point[], double>)Functions.Pwl).GetMethodInfo();
        private static readonly ConstructorInfo _point = typeof(Point).GetTypeInfo().GetConstructor(new[] { typeof(double), typeof(double) });

        /// <summary>
        /// A set of default functions.
        /// </summary>
        public static readonly Dictionary<string, ApplyFunction> Defaults = new Dictionary<string, ApplyFunction>
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
            { "max", Max },
            { "rnd", Random }
        };

        private static IReadOnlyList<Node> Check(this IReadOnlyList<Node> nodes, int expected)
        {
            if (nodes.Count != expected)
                throw new ArgumentMismatchException(expected, nodes.Count);
            return nodes;
        }

        /// <summary>
        /// Registers the default functions.
        /// </summary>
        /// <param name="definitions">The definitions.</param>
        public static void RegisterDefaultFunctions(this Dictionary<string, ApplyFunction> definitions)
        {
            definitions.ThrowIfNull(nameof(definitions));
            foreach (var pair in Defaults)
                definitions.Add(pair.Key, pair.Value);
        }

        // No-argument functions
        private static void Random(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments)
        {
            Random rnd = new Random();
            fbi.Call(rnd.NextDouble);
        }

        // One-argument functions
        private static void Zero(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Push(0.0);
        private static void Abs(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Math.Abs, arguments);
        private static void Sgn(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) { fbi.Call(null, _sgn, arguments.Check(1)); fbi.Generator.Emit(OpCodes.Conv_R8); }
        private static void Sqrt(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Functions.Sqrt, arguments);
        private static void URamp(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Functions.Ramp, arguments);
        private static void U(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Functions.Step, arguments);
        private static void U2(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Functions.Step2, arguments);
        private static void Sin(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Math.Sin, arguments);
        private static void Cos(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Math.Cos, arguments);
        private static void Tan(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Math.Tan, arguments);
        private static void Asin(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Math.Asin, arguments);
        private static void Acos(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Math.Acos, arguments);
        private static void Atan(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Math.Atan, arguments);
        private static void Sinh(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Math.Sinh, arguments);
        private static void Cosh(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Math.Cosh, arguments);
        private static void Tanh(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Math.Tanh, arguments);
        private static void Ceil(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Math.Ceiling, arguments);
        private static void Floor(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Math.Floor, arguments);
        private static void Exp(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Math.Exp, arguments);
        private static void Log(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Functions.Log, arguments);
        private static void Log10(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Functions.Log10, arguments);
        private static void Square(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) { fbi.Push(arguments.Check(1)[0]); fbi.Generator.Emit(OpCodes.Dup); fbi.Generator.Emit(OpCodes.Mul); }
        private static void Nint(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) { fbi.Push(arguments.Check(1)[0]); fbi.Push(0); fbi.Generator.Emit(OpCodes.Call, _round); }

        // Two-argument functions
        private static void Pwr(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Functions.Power2, arguments);
        private static void Min(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Math.Min, arguments);
        private static void Max(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) => fbi.Call(Math.Max, arguments);
        private static void Round(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments) { fbi.Push(arguments.Check(2)[0]); fbi.Push(arguments[1]); fbi.Generator.Emit(OpCodes.Conv_I4); fbi.Generator.Emit(OpCodes.Call, _round); }

        // N-argument functions
        private static void Pwl(FunctionBuilderInstance fbi, IReadOnlyList<Node> arguments)
        {
            if (arguments.Count < 3)
                throw new ArgumentMismatchException(3, arguments.Count);
            int points = (arguments.Count - 1) / 2;
            if (arguments.Count % 2 == 0)
                throw new ArgumentMismatchException(points * 2 + 1, arguments.Count);

            var il = fbi.Generator;

            // Create our array
            fbi.Push(arguments[0]);
            fbi.Push(points);
            il.Emit(OpCodes.Newarr, typeof(Point));
            for (var i = 0; i < points; i++)
            {
                il.Emit(OpCodes.Dup); // Make another reference to the array
                fbi.Push(i); // Set the index

                // Create the point
                fbi.Push(arguments[i * 2 + 1]);
                fbi.Push(arguments[i * 2 + 2]);
                il.Emit(OpCodes.Newobj, _point);

                // Store the element
                il.Emit(OpCodes.Stelem, typeof(Point));
            }
            il.Emit(OpCodes.Call, _pwl);
        }
    }
}
