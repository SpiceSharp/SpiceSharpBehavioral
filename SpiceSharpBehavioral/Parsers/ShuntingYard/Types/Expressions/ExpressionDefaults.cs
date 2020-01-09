using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharpBehavioral.Parsers.ShuntingYard
{
    /// <summary>
    /// Defaults for double parsers.
    /// </summary>
    public static class ExpressionDefaults
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
        public static IDictionary<string, Expression> Variables { get; set; } = new Dictionary<string, Expression>();

        /// <summary>
        /// Called when a function was found.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments containing the event data.</param>
        public static void FunctionFound(object sender, FunctionFoundEventArgs<Expression> args)
        {
            if (Functions != null && Functions.TryGetValue(args.Name, out var fd))
                args.Result = fd(args.Arguments);
        }

        /// <summary>
        /// Called when a variable was found.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments containing the event data.</param>
        public static void VariableFound(object sender, VariableFoundEventArgs<Expression> args)
        {
            if (Variables != null && Variables.TryGetValue(args.Name, out var value))
                args.Result = value;
        }

        private static Expression Abs(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for abs()");
            return Expression.Call(null, GetMethodInfo(Math.Abs), arguments[0]);
        }
        private static Expression Exp(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for exp()");
            return Expression.Call(null, GetMethodInfo(Math.Exp), arguments[0]);
        }
        private static Expression Log(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for log()");
            return Expression.Call(null, GetMethodInfo((Func<double, double>)Math.Log), arguments[0]);
        }
        private static Expression Log10(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for log10()");
            return Expression.Call(null, GetMethodInfo(Math.Log10), arguments[0]);
        }
        private static Expression Sqrt(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for sqrt()");
            return Expression.Call(null, GetMethodInfo(Math.Sqrt), arguments[0]);
        }
        private static Expression Sin(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for sin()");
            return Expression.Call(null, GetMethodInfo(Math.Sin), arguments[0]);
        }
        private static Expression Cos(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for cos()");
            return Expression.Call(null, GetMethodInfo(Math.Cos), arguments[0]);
        }
        private static Expression Tan(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for tan()");
            return Expression.Call(null, GetMethodInfo(Math.Tan), arguments[0]);
        }
        private static Expression Asin(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for asin()");
            return Expression.Call(null, GetMethodInfo(Math.Asin), arguments[0]);
        }
        private static Expression Acos(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for acos()");
            return Expression.Call(null, GetMethodInfo(Math.Acos), arguments[0]);
        }
        private static Expression Atan(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for atan()");
            return Expression.Call(null, GetMethodInfo(Math.Atan), arguments[0]);
        }
        private static Expression Sinh(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for sinh()");
            return Expression.Call(null, GetMethodInfo(Math.Sinh), arguments[0]);
        }
        private static Expression Cosh(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for cosh()");
            return Expression.Call(null, GetMethodInfo(Math.Cosh), arguments[0]);
        }
        private static Expression Tanh(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ParserException("Invalid number of arguments for tanh()");
            return Expression.Call(null, GetMethodInfo(Math.Tanh), arguments[0]);
        }
        private static Expression Max(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count < 2)
                throw new ParserException("Invalid number of arguments for max()");
            var method = GetMethodInfo(Math.Max);
            Expression result = arguments[0];
            for (var i = 1; i < arguments.Count; i++)
                result = Expression.Call(null, method, result, arguments[i]);
            return result;
        }
        private static Expression Min(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count < 2)
                throw new ParserException("Invalid number of arguments for max()");
            var method = GetMethodInfo(Math.Min);
            Expression result = arguments[0];
            for (var i = 1; i < arguments.Count; i++)
                result = Expression.Call(null, method, result, arguments[i]);
            return result;
        }
        private static Expression Round(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count == 1)
                return Expression.Call(null, GetMethodInfo(Math.Round), arguments[0]);
            else if (arguments.Count == 2)
                return Expression.Call(null, GetMethodInfo(Round2), arguments[0], arguments[1]);
            throw new ParserException("Invalid number of arguments for round()");
        }
        private static double Round2(double left, double right) => Math.Round(left, (int)(right + 0.1));
        private static Expression Pow(IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 2)
                throw new ParserException("Invalid number of arguments for pow()");
            return Expression.Call(null, GetMethodInfo(Math.Pow), arguments[0], arguments[1]);
        }

        /// <summary>
        /// Describes a function.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The function value.</returns>
        public delegate Expression FunctionDescription(IReadOnlyList<Expression> arguments);

        /// <summary>
        /// Gets the method information.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>The method information.</returns>
        private static MethodInfo GetMethodInfo(Func<double, double> method) => method.GetMethodInfo();

        /// <summary>
        /// Gets the method information.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>The method information.</returns>
        private static MethodInfo GetMethodInfo(Func<double, double, double> method) => method.GetMethodInfo();
    }
}
