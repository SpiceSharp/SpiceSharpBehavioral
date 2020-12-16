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
            _safeDiv = ((Func<double, double, double, double>)Functions.SafeDivide).GetMethodInfo(),
            _sgn = ((Func<double, int>)Math.Sign).GetMethodInfo(),
            _round = ((Func<double, int, double>)Math.Round).GetMethodInfo(),
            _pwl = ((Func<double, Point[], double>)Functions.Pwl).GetMethodInfo(),
            _pwlDerivative = ((Func<double, Point[], double>)Functions.PwlDerivative).GetMethodInfo();
        private static readonly ConstructorInfo _point = typeof(Point).GetTypeInfo().GetConstructor(new[] { typeof(double), typeof(double) });

        /// <summary>
        /// A set of default functions.
        /// </summary>
        public static readonly Dictionary<string, ApplyFunction> Defaults = new Dictionary<string, ApplyFunction>(StringComparer.OrdinalIgnoreCase)
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
            { "u", U }, { "du(0)", Zero },
            { "u2", U2 }, { "du2(0)", DU2 },
            { "uramp", URamp }, { "duramp(0)", DURampDerivative },
            { "ceil", Ceil },
            { "floor", Floor },
            { "nint", Nint },
            { "round", Round },
            { "square", Square },
            { "pwl", Pwl }, { "dpwl(0)", PwlDerivative },
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
        private static void Random(ILState ils, IReadOnlyList<Node> arguments)
        {
            Random rnd = new Random();
            ils.Call(rnd.NextDouble);
        }

        // One-argument functions
        private static void Zero(ILState ils, IReadOnlyList<Node> arguments) => ils.Push(0.0);
        private static void Abs(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Abs, arguments);
        private static void Sgn(ILState ils, IReadOnlyList<Node> arguments) { ils.Call(null, _sgn, arguments.Check(1)); ils.Generator.Emit(OpCodes.Conv_R8); }
        private static void Sqrt(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Sqrt, arguments);
        private static void DSqrt0(ILState ils, IReadOnlyList<Node> arguments)
        {
            ils.Push(0.5);
            ils.Call(Functions.Sqrt, arguments);
            ils.Push(ils.Builder.FudgeFactor);
            ils.Generator.Emit(OpCodes.Call, _safeDiv);
        }
        private static void URamp(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Ramp, arguments);
        private static void DURampDerivative(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.RampDerivative, arguments);
        private static void U(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Step, arguments);
        private static void U2(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Step2, arguments);
        private static void DU2(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Step2Derivative, arguments);
        private static void Sin(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Sin, arguments);
        private static void Cos(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Cos, arguments);
        private static void Tan(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Tan, arguments);
        private static void Asin(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Asin, arguments);
        private static void Acos(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Acos, arguments);
        private static void Atan(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Atan, arguments);
        private static void Sinh(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Sinh, arguments);
        private static void Cosh(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Cosh, arguments);
        private static void Tanh(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Tanh, arguments);
        private static void Ceil(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Ceiling, arguments);
        private static void Floor(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Floor, arguments);
        private static void Exp(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Exp, arguments);
        private static void Log(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Log, arguments);
        private static void Log10(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Log10, arguments);
        private static void Square(ILState ils, IReadOnlyList<Node> arguments) { ils.Push(arguments.Check(1)[0]); ils.Generator.Emit(OpCodes.Dup); ils.Generator.Emit(OpCodes.Mul); }
        private static void Nint(ILState ils, IReadOnlyList<Node> arguments) { ils.Push(arguments.Check(1)[0]); ils.Push(0); ils.Generator.Emit(OpCodes.Call, _round); }

        // Two-argument functions
        private static void Pwr(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Power2, arguments);
        private static void Min(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Min, arguments);
        private static void Max(ILState ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Max, arguments);
        private static void Round(ILState ils, IReadOnlyList<Node> arguments) 
        { 
            ils.Push(arguments.Check(2)[0]); 
            ils.Push(arguments[1]); 
            ils.Generator.Emit(OpCodes.Conv_I4); 
            ils.Generator.Emit(OpCodes.Call, _round); 
        }

        // N-argument functions
        private static void Pwl(ILState ils, IReadOnlyList<Node> arguments)
        {
            if (arguments.Count < 3)
                throw new ArgumentMismatchException(3, arguments.Count);
            int points = (arguments.Count - 1) / 2;
            if (arguments.Count % 2 == 0)
                throw new ArgumentMismatchException(points * 2 + 1, arguments.Count);

            var il = ils.Generator;

            // Create our array
            ils.Push(arguments[0]);
            ils.Push(points);
            il.Emit(OpCodes.Newarr, typeof(Point));
            for (var i = 0; i < points; i++)
            {
                il.Emit(OpCodes.Dup); // Make another reference to the array
                ils.Push(i); // Set the index

                // Create the point
                ils.Push(arguments[i * 2 + 1]);
                ils.Push(arguments[i * 2 + 2]);
                il.Emit(OpCodes.Newobj, _point);

                // Store the element
                il.Emit(OpCodes.Stelem, typeof(Point));
            }
            il.Emit(OpCodes.Call, _pwl);
        }
        private static void PwlDerivative(ILState ils, IReadOnlyList<Node> arguments)
        {
            if (arguments.Count < 3)
                throw new ArgumentMismatchException(3, arguments.Count);
            int points = (arguments.Count - 1) / 2;
            if (arguments.Count % 2 == 0)
                throw new ArgumentMismatchException(points * 2 + 1, arguments.Count);

            var il = ils.Generator;

            // Create our array
            ils.Push(arguments[0]);
            ils.Push(points);
            il.Emit(OpCodes.Newarr, typeof(Point));
            for (var i = 0; i < points; i++)
            {
                il.Emit(OpCodes.Dup); // Make another reference to the array
                ils.Push(i); // Set the index

                // Create the point
                ils.Push(arguments[i * 2 + 1]);
                ils.Push(arguments[i * 2 + 2]);
                il.Emit(OpCodes.Newobj, _point);

                // Store the element
                il.Emit(OpCodes.Stelem, typeof(Point));
            }
            il.Emit(OpCodes.Call, _pwlDerivative);
        }
    }
}
