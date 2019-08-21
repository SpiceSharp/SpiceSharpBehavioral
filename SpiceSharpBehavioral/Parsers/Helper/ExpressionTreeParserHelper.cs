using SpiceSharp;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharpBehavioral.Parsers.Helper
{
    /// <summary>
    /// Helper class for <see cref="ExpressionTreeDerivativeParser"/>.
    /// </summary>
    public static class ExpressionTreeParserHelper
    {
        /// <summary>
        /// The default functions.
        /// </summary>
        public static Dictionary<string, ExpressionFunction> DefaultFunctions { get; set; } = new Dictionary<string, ExpressionFunction>()
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
        public static void RegisterDefaultFunctions(this ExpressionTreeParser parser)
        {
            parser.FunctionFound += DefaultFunctionFound;
        }

        /// <summary>
        /// Remove the default functions from the parser.
        /// </summary>
        /// <param name="parser">The parser.</param>
        public static void UnregisterDefaultFunctions(this ExpressionTreeParser parser)
        {
            parser.FunctionFound -= DefaultFunctionFound;
        }

        /// <summary>
        /// Default functions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DefaultFunctionFound(object sender, FunctionFoundEventArgs<Expression> e)
        {
            if (DefaultFunctions.TryGetValue(e.Name, out var function))
            {
                var arguments = new Expression[e.ArgumentCount];
                for (var i = 0; i < e.ArgumentCount; i++)
                    arguments[i] = e[i];
                e.Result = function?.Invoke(arguments);
            }
        }

        /// <summary>
        /// Exponentials
        /// </summary>
        private static readonly MethodInfo ExpMethod = typeof(Math).GetTypeInfo().GetMethod("Exp", new[] { typeof(double) });

        /// <summary>
        /// Applies the exponent.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The exponent result.</returns>
        public static Expression ApplyExp(Expression[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            return Expression.Call(ExpMethod, arg);
        }

        /// <summary>
        /// Logarithms
        /// </summary>
        private static readonly MethodInfo LogMethod = typeof(Math).GetTypeInfo().GetMethod("Log", new[] { typeof(double) });
        private static readonly MethodInfo Log2Method = typeof(Math).GetTypeInfo().GetMethod("Log", new[] { typeof(double), typeof(double) });

        /// <summary>
        /// Applies the logarithm.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The logarithm result.</returns>
        public static Expression ApplyLog(Expression[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            if (arguments.Length == 1)
                return Expression.Call(LogMethod, arguments[0]);
            if (arguments.Length == 2)
                return Expression.Call(Log2Method, arguments[0], arguments[1]);
            throw new CircuitException("Invalid number of arguments, {0} given but 2 expected".FormatString(arguments.Length));
        }

        /// <summary>
        /// Applies the base-10 logarithm.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The logarithm result.</returns>
        public static Expression ApplyLog10(Expression[] arguments)
        {
            var result = ApplyLog(arguments);
            return Expression.Divide(result, Expression.Constant(Math.Log(10.0)));
        }

        /// <summary>
        /// Power method.
        /// </summary>
        private static readonly MethodInfo PowMethod = typeof(Math).GetTypeInfo().GetMethod("Pow", new[] { typeof(double), typeof(double) });

        /// <summary>
        /// Raises to a power.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The power result.</returns>
        public static Expression ApplyPow(Expression[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 2);
            return Expression.Call(PowMethod, arguments[0], arguments[1]);
        }

        /// <summary>
        /// Square root method.
        /// </summary>
        private static readonly MethodInfo SqrtMethod = typeof(Math).GetTypeInfo().GetMethod("Sqrt", new[] { typeof(double) });

        /// <summary>
        /// Applies the square root.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The square root result.</returns>
        public static Expression ApplySqrt(Expression[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            return Expression.Call(SqrtMethod, arguments[0]);
        }

        /// <summary>
        /// Trigonometry
        /// </summary>
        private static readonly MethodInfo SinMethod = typeof(Math).GetTypeInfo().GetMethod("Sin", new[] { typeof(double) });
        private static readonly MethodInfo CosMethod = typeof(Math).GetTypeInfo().GetMethod("Cos", new[] { typeof(double) });
        private static readonly MethodInfo TanMethod = typeof(Math).GetTypeInfo().GetMethod("Tan", new[] { typeof(double) });

        /// <summary>
        /// Applies the sine.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The sine result.</returns>
        public static Expression ApplySin(Expression[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            return Expression.Call(SinMethod, arguments[0]);
        }

        /// <summary>
        /// Applies the cosine.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The cosine result.</returns>
        private static Expression ApplyCos(Expression[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            return Expression.Call(CosMethod, arguments[0]);
        }

        /// <summary>
        /// Applies the tangent.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The tangent result.</returns>
        private static Expression ApplyTan(Expression[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            return Expression.Call(TanMethod, arguments[0]);
        }

        /// <summary>
        /// Inverse trigonometry.
        /// </summary>
        private static readonly MethodInfo AsinMethod = typeof(Math).GetTypeInfo().GetMethod("Asin", new[] { typeof(double) });
        private static readonly MethodInfo AcosMethod = typeof(Math).GetTypeInfo().GetMethod("Acos", new[] { typeof(double) });
        private static readonly MethodInfo AtanMethod = typeof(Math).GetTypeInfo().GetMethod("Atan", new[] { typeof(double) });

        /// <summary>
        /// Applies the arcsine.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The arcsine result.</returns>
        public static Expression ApplyAsin(Expression[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            return Expression.Call(AsinMethod, arguments[0]);
        }

        /// <summary>
        /// Applies the arccosine.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The arccosine result.</returns>
        public static Expression ApplyAcos(Expression[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            return Expression.Call(AcosMethod, arguments[0]);
        }

        /// <summary>
        /// Applies the arctangent.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The arctangent result.</returns>
        public static Expression ApplyAtan(Expression[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            return Expression.Call(AtanMethod, arguments[0]);
        }

        /// <summary>
        /// Miscellaneous
        /// </summary>
        private static readonly MethodInfo AbsMethod = typeof(Math).GetTypeInfo().GetMethod("Abs", new[] { typeof(double) });
        private static readonly MethodInfo RoundMethod = typeof(Math).GetTypeInfo().GetMethod("Round", new[] { typeof(double) });
        private static readonly MethodInfo Round2Method = typeof(Math).GetTypeInfo().GetMethod("Round", new[] { typeof(double), typeof(int) });
        private static readonly MethodInfo MinMethod = typeof(Math).GetTypeInfo().GetMethod("Min", new[] { typeof(double), typeof(double) });
        private static readonly MethodInfo MaxMethod = typeof(Math).GetTypeInfo().GetMethod("Max", new[] { typeof(double), typeof(double) });

        /// <summary>
        /// Applies the absolute value.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The absolute value result.</returns>
        public static Expression ApplyAbs(Expression[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            return Expression.Call(AbsMethod, arguments[0]);
        }

        /// <summary>
        /// Rounds the value.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The rounded result.</returns>
        public static Expression ApplyRound(Expression[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            if (arguments.Length == 1)
                return Expression.Call(RoundMethod, arguments[0]);
            if (arguments.Length == 2)
                return Expression.Call(Round2Method, arguments[0], Expression.Convert(Expression.Call(RoundMethod, arguments[1]), typeof(int)));
            throw new CircuitException("Invalid number of arguments for Abs()");
        }

        /// <summary>
        /// Applies the minimum.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The minimum result.</returns>
        public static Expression ApplyMin(Expression[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            var result = arguments[0];
            for (var i = 1; i < arguments.Length; i++)
                result = Expression.Call(MinMethod, result, arguments[i]);
            return result;
        }

        /// <summary>
        /// Applies the maximum.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The maximum result.</returns>
        public static Expression ApplyMax(Expression[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            var result = arguments[0];
            for (var i = 1; i < arguments.Length; i++)
                result = Expression.Call(MaxMethod, result, arguments[i]);
            return result;
        }

        /// <summary>
        /// Delegate for applying a function.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The result of the function.</returns>
        public delegate Expression ExpressionFunction(Expression[] arguments);
    }
}
