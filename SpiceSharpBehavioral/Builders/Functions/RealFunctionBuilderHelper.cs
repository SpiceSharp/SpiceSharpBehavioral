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
    /// Helper methods for a <see cref="RealFunctionBuilder"/>.
    /// </summary>
    public static class RealFunctionBuilderHelper
    {
        private static readonly Random _rnd = new Random();
        private static readonly MethodInfo
            _sgn = ((Func<double, int>)Math.Sign).GetMethodInfo(),
            _round = ((Func<double, int, double>)Math.Round).GetMethodInfo(),
            _pwl = ((Func<double, Point[], double>)Functions.Pwl).GetMethodInfo(),
            _pwlDerivative = ((Func<double, Point[], double>)Functions.PwlDerivative).GetMethodInfo();
        private static readonly ConstructorInfo _point = typeof(Point).GetTypeInfo().GetConstructor(new[] { typeof(double), typeof(double) });

        /// <summary>
        /// Delegjate for applying a function to the IL state.
        /// </summary>
        /// <param name="il">The IL state.</param>
        /// <param name="arguments">The function arguments.</param>
        public delegate void ApplyFunction(IILState<double> il, IReadOnlyList<Node> arguments);

        /// <summary>
        /// A set of default functions.
        /// </summary>
        public static readonly Dictionary<string, ApplyFunction> Defaults = new Dictionary<string, ApplyFunction>(StringComparer.OrdinalIgnoreCase)
        {
            { "abs", Abs },
            { "sgn", Sgn },
            { "sqrt", Sqrt },
            { "pow", Pow },
            { "pwr", Pwr },
            { "log", Log }, { "ln", Log },
            { "pwrs", Pwrs },
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
            { "atan2", Atan2 },
            { "hypot", Hypot },
            { "u", U }, { "du(0)", Zero },
            { "u2", U2 }, { "du2(0)", DU2 },
            { "uramp", URamp }, { "duramp(0)", DURampDerivative },
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
            { "rnd", Random }, { "rand", Random },
            { "if", If }
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
        /// <param name="builder">The function builder.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> is <c>null</c>.</exception>
        public static void RegisterDefaultFunctions(this IFunctionBuilder<double> builder)
        {
            builder.ThrowIfNull(nameof(builder));
            builder.FunctionFound += OnFunctionFound;
        }

        private static void OnFunctionFound(object sender, FunctionFoundEventArgs<double> args)
        {
            if (Defaults.TryGetValue(args.Function.Name, out var func))
            {
                func(args.ILState, args.Function.Arguments);
                args.Created = true;
            }
        }

        // No-argument functions
        private static void Random(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(_rnd.NextDouble);

        // One-argument functions
        private static void Zero(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Push(0.0);
        private static void Abs(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Abs, arguments);
        private static void Sgn(IILState<double> ils, IReadOnlyList<Node> arguments) { ils.Call(null, _sgn, arguments.Check(1)); ils.Generator.Emit(OpCodes.Conv_R8); }
        private static void Sqrt(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Sqrt, arguments);
        private static void URamp(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Ramp, arguments);
        private static void DURampDerivative(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.RampDerivative, arguments);
        private static void U(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Step, arguments);
        private static void U2(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Step2, arguments);
        private static void DU2(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Step2Derivative, arguments);
        private static void Sin(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Sin, arguments);
        private static void Cos(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Cos, arguments);
        private static void Tan(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Tan, arguments);
        private static void Asin(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Asin, arguments);
        private static void Acos(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Acos, arguments);
        private static void Atan(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Atan, arguments);
        private static void Sinh(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Sinh, arguments);
        private static void Cosh(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Cosh, arguments);
        private static void Tanh(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Tanh, arguments);
        private static void Ceil(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Ceiling, arguments);
        private static void Floor(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Floor, arguments);
        private static void Exp(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Exp, arguments);
        private static void Log(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Log, arguments);
        private static void Log10(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Log10, arguments);
        private static void Square(IILState<double> ils, IReadOnlyList<Node> arguments) { ils.Push(arguments.Check(1)[0]); ils.Generator.Emit(OpCodes.Dup); ils.Generator.Emit(OpCodes.Mul); }
        private static void Nint(IILState<double> ils, IReadOnlyList<Node> arguments) { ils.Push(arguments.Check(1)[0]); ils.PushInt(0); ils.Generator.Emit(OpCodes.Call, _round); }

        // Two-argument functions
        private static void Pow(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Pow, arguments);
        private static void Pwr(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Power, arguments);
        private static void Pwrs(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Power2, arguments);
        private static void Min(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Min, arguments);
        private static void Max(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Max, arguments);
        private static void Round(IILState<double> ils, IReadOnlyList<Node> arguments) 
        {
            ils.Push(arguments.Check(2)[0]); 
            ils.Push(arguments[1]); 
            ils.Generator.Emit(OpCodes.Conv_I4); 
            ils.Generator.Emit(OpCodes.Call, _round); 
        }
        private static void Atan2(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Math.Atan2, arguments);
        private static void Hypot(IILState<double> ils, IReadOnlyList<Node> arguments) => ils.Call(Functions.Hypot, arguments);

        // Three-argument functions
        private static void If(IILState<double> ils, IReadOnlyList<Node> arguments)
        {
            arguments.Check(3);
            ils.Push(arguments[0]);
            ils.Push(0.5);
            var lblElse = ils.Generator.DefineLabel();
            var lblEnd = ils.Generator.DefineLabel();
            ils.Generator.Emit(OpCodes.Ble_S, lblElse);
            ils.Push(arguments[1]);
            ils.Generator.Emit(OpCodes.Br_S, lblEnd);
            ils.Generator.MarkLabel(lblElse);
            ils.Push(arguments[2]);
            ils.Generator.MarkLabel(lblEnd);
        }

        // N-argument functions
        private static void Pwl(IILState<double> ils, IReadOnlyList<Node> arguments)
        {
            if (arguments.Count < 3)
                throw new ArgumentMismatchException(3, arguments.Count);
            int points = (arguments.Count - 1) / 2;
            if (arguments.Count % 2 == 0)
                throw new ArgumentMismatchException(points * 2 + 1, arguments.Count);

            var il = ils.Generator;

            // Create our array
            ils.Push(arguments[0]);
            ils.PushInt(points);
            il.Emit(OpCodes.Newarr, typeof(Point));
            for (var i = 0; i < points; i++)
            {
                il.Emit(OpCodes.Dup); // Make another reference to the array
                ils.PushInt(i); // Set the index

                // Create the point
                ils.Push(arguments[i * 2 + 1]);
                ils.Push(arguments[i * 2 + 2]);
                il.Emit(OpCodes.Newobj, _point);

                // Store the element
                il.Emit(OpCodes.Stelem, typeof(Point));
            }
            il.Emit(OpCodes.Call, _pwl);
        }
        private static void PwlDerivative(IILState<double> ils, IReadOnlyList<Node> arguments)
        {
            if (arguments.Count < 3)
                throw new ArgumentMismatchException(3, arguments.Count);
            int points = (arguments.Count - 1) / 2;
            if (arguments.Count % 2 == 0)
                throw new ArgumentMismatchException(points * 2 + 1, arguments.Count);

            var il = ils.Generator;

            // Create our array
            ils.Push(arguments[0]);
            ils.PushInt(points);
            il.Emit(OpCodes.Newarr, typeof(Point));
            for (var i = 0; i < points; i++)
            {
                il.Emit(OpCodes.Dup); // Make another reference to the array
                ils.PushInt(i); // Set the index

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
