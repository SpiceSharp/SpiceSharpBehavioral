using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.ShuntingYard
{
    /// <summary>
    /// Defaults for double parsers.
    /// </summary>
    public static class DoubleDefaults
    {
        /// <summary>
        /// Gets or sets the default functions.
        /// </summary>
        /// <value>
        /// The default functions.
        /// </value>
        public static IDictionary<string, FunctionDescription> Functions { get; set; } = new Dictionary<string, FunctionDescription>
        {
            { "abs", Abs }, { "exp", Exp }, { "log", Log }, { "ln", Log }, { "log10", Log10 }, { "sqrt", Sqrt },
            { "sin", Sin }, { "cos", Cos }, { "tan", Tan },
            { "asin", Asin }, { "acos", Acos }, { "atan", Atan },
            { "sinh", Sinh }, { "cosh", Cosh }, { "tanh", Tanh },
            { "max", Max }, { "min", Min }, { "round", Round }, { "pow", Pow }
        };

        /// <summary>
        /// Gets or sets the default variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public static IDictionary<string, double> Variables { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Called when a function was found.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments containing the event data.</param>
        public static void FunctionFound(object sender, FunctionFoundEventArgs<double> args)
        {
            if (Functions != null && Functions.TryGetValue(args.Name, out var fd))
                args.Result = fd(args.Arguments);
        }

        /// <summary>
        /// Called when a variable was found.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments containing the event data.</param>
        public static void VariableFound(object sender, VariableFoundEventArgs<double> args)
        {
            if (Variables != null && Variables.TryGetValue(args.Name, out var value))
                args.Result = value;
        }

        private static double Abs(IReadOnlyList<double> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for abs()");
            return Math.Abs(arguments[0]);
        }
        private static double Exp(IReadOnlyList<double> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for exp()");
            return Math.Exp(arguments[0]);
        }
        private static double Log(IReadOnlyList<double> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for log()");
            return Math.Log(arguments[0]);
        }
        private static double Log10(IReadOnlyList<double> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for log10()");
            return Math.Log10(arguments[0]);
        }
        private static double Sqrt(IReadOnlyList<double> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for sqrt()");
            return Math.Sqrt(arguments[0]);
        }
        private static double Sin(IReadOnlyList<double> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for sin()");
            return Math.Sin(arguments[0]);
        }
        private static double Cos(IReadOnlyList<double> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for cos()");
            return Math.Cos(arguments[0]);
        }
        private static double Tan(IReadOnlyList<double> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for tan()");
            return Math.Tan(arguments[0]);
        }
        private static double Asin(IReadOnlyList<double> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for asin()");
            return Math.Asin(arguments[0]);
        }
        private static double Acos(IReadOnlyList<double> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for acos()");
            return Math.Acos(arguments[0]);
        }
        private static double Atan(IReadOnlyList<double> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for atan()");
            return Math.Atan(arguments[0]);
        }
        private static double Sinh(IReadOnlyList<double> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for sinh()");
            return Math.Sinh(arguments[0]);
        }
        private static double Cosh(IReadOnlyList<double> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for cosh()");
            return Math.Cosh(arguments[0]);
        }
        private static double Tanh(IReadOnlyList<double> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for tanh()");
            return Math.Tan(arguments[0]);
        }
        private static double Max(IReadOnlyList<double> arguments)
        {
            if (arguments.Count < 2)
                throw new ParserException("Invalid number of arguments for max()");
            double result = arguments[0];
            for (var i = 1; i < arguments.Count; i++)
                result = Math.Max(arguments[i], result);
            return result;
        }
        private static double Min(IReadOnlyList<double> arguments)
        {
            if (arguments.Count < 2)
                throw new ParserException("Invalid number of arguments for min()");
            double result = arguments[0];
            for (var i = 1; i < arguments.Count; i++)
                result = Math.Min(arguments[i], result);
            return result;
        }
        private static double Round(IReadOnlyList<double> arguments)
        {
            if (arguments.Count == 1)
                return Math.Round(arguments[0]);
            else if (arguments.Count == 2)
                return Math.Round(arguments[0], (int)(arguments[1] + 0.1));
            throw new ParserException("Invalid number of arguments for round()");
        }
        private static double Pow(IReadOnlyList<double> arguments)
        {
            if (arguments.Count != 2)
                throw new ParserException("Invalid number of arguments for pow()");
            return Math.Pow(arguments[0], arguments[1]);
        }

        /// <summary>
        /// Describes a function.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The function value.</returns>
        public delegate double FunctionDescription(IReadOnlyList<double> arguments);
    }
}
