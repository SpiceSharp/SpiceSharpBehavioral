﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers.Helper
{
    /// <summary>
    /// Helper methods for a <see cref="ExpressionTreeDerivativeParser"/>.
    /// </summary>
    public static class ExpressionTreeDerivativesParserHelper
    {
        /// <summary>
        /// The default functions.
        /// </summary>
        public static Dictionary<string, ExpressionTreeDerivativesFunction> DefaultFunctions { get; set; } = new Dictionary<string, ExpressionTreeDerivativesFunction>()
        {
            { "Exp", ApplyExp },
            { "Log", ApplyLog },
            { "Pow", ApplyPow },
            { "Log10", ApplyLog10 },
            { "Sqrt", ApplySqrt },
            { "Sin", ApplySin },
            { "Sinh", ApplySinh },
            { "Cos", ApplyCos },
            { "Cosh", ApplyCosh },
            { "Tan", ApplyTan },
            { "Tanh", ApplyTanh },
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
        public static void RegisterDefaultFunctions(this ExpressionTreeDerivativeParser parser)
        {
            parser.FunctionFound += DefaultFunctionFound;
        }

        /// <summary>
        /// Remove the default functions from the parser.
        /// </summary>
        /// <param name="parser">The parser.</param>
        public static void UnregisterDefaultFunctions(this ExpressionTreeDerivativeParser parser)
        {
            parser.FunctionFound -= DefaultFunctionFound;
        }

        /// <summary>
        /// Default functions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DefaultFunctionFound(object sender, FunctionFoundEventArgs<Derivatives<Expression>> e)
        {
            if (DefaultFunctions.TryGetValue(e.Name, out var function))
            {
                var arguments = new Derivatives<Expression>[e.ArgumentCount];
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
        /// Applies the exponent operator.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns></returns>
        public static Derivatives<Expression> ApplyExp(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new ExpressionTreeDerivatives(arg.Count);
            result[0] = Expression.Call(ExpMethod, new[] { arg[0] });

            // Chain rule for derivatives
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                    result[i] = Expression.Multiply(result[0], arg[i]);
            }
            return result;
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
        /// <returns>The log result.</returns>
        public static Derivatives<Expression> ApplyLog(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            if (arguments.Length == 1)
            {
                // Ln(f(x))' = 1/f(x)*f'(x)
                var arg = arguments[0];
                var result = new ExpressionTreeDerivatives(arg.Count);
                result[0] = Expression.Call(LogMethod, arg[0]);

                // Chain rule
                for (var i = 1; i < arg.Count; i++)
                {
                    if (arg[i] != null)
                        result[i] = Expression.Divide(arg[i], arg[0]);
                }
                return result;
            }

            if (arguments.Length == 2)
            {
                // Log(g(x), f(x)) = Ln(g(x)) / Ln(f(x))
                var g = arguments[0];
                var f = arguments[1];
                var size = Math.Max(f.Count, g.Count);
                var result = new ExpressionTreeDerivatives(size);
                result[0] = Expression.Call(Log2Method, g[0], f[0]);

                // Chain rule
                for (var i = 1; i < size; i++)
                {
                    if (g[i] != null)
                    {
                        result[i] = Expression.Divide(
                            Expression.Divide(g[i], g[0]),
                            Expression.Call(LogMethod, f[0]));
                    }
                    if (f[i] != null)
                    {
                        var term = Expression.Multiply(
                            Expression.Divide(f[i], f[0]), result[0]);
                        if (result[i] == null)
                            result[i] = Expression.Negate(term);
                        else
                            result[i] = Expression.Subtract(result[i], term);
                    }
                }
                return result;
            }

            throw new CircuitException("Invalid number of arguments, {0} given but 2 expected".FormatString(arguments.Length));
        }

        /// <summary>
        /// Applies the base-10 logarithm.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The logarithm result.</returns>
        public static Derivatives<Expression> ApplyLog10(Derivatives<Expression>[] arguments)
        {
            var result = ApplyLog(arguments);
            double a = Math.Log(10.0);
            for (var i = 0; i < result.Count; i++)
                result[i] = Expression.Divide(result[i], Expression.Constant(a));
            return result;
        }

        /// <summary>
        /// Power method.
        /// </summary>
        private static readonly MethodInfo PowMethod = typeof(Math).GetTypeInfo().GetMethod("Pow", new[] { typeof(double), typeof(double) });

        /// <summary>
        /// Raises to a power.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The result of the power.</returns>
        public static Derivatives<Expression> ApplyPow(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 2);
            var f = arguments[0];
            var g = arguments[1];
            var size = Math.Max(f.Count, g.Count);
            var result = new ExpressionTreeDerivatives(size);
            result[0] = Expression.Call(PowMethod, f[0], g[0]);

            // Apply chain rule
            for (var i = 1; i < size; i++)
            {
                if (f[i] != null)
                    result[i] = Expression.Multiply(Expression.Multiply(g[0], f[i]),
                        Expression.Call(PowMethod, f[0], Expression.Subtract(g[0], Expression.Constant(1.0))));

                if (g[i] != null)
                {
                    var term = Expression.Multiply(Expression.Multiply(g[i], result[0]),
                        Expression.Call(LogMethod, f[0]));
                    if (result[i] == null)
                        result[i] = term;
                    else
                        result[i] = Expression.Add(result[i], term);
                }
            }
            return result;
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
        public static Derivatives<Expression> ApplySqrt(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new ExpressionTreeDerivatives(arg.Count);
            result[0] = Expression.Call(SqrtMethod, arg[0]);

            // Apply the chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                    result[i] = Expression.Divide(arg[i], 
                        Expression.Multiply(Expression.Constant(-2.0), result[0]));
            }
            return result;
        }

        /// <summary>
        /// Trigonometry
        /// </summary>
        private static readonly MethodInfo SinMethod = typeof(Math).GetTypeInfo().GetMethod("Sin", new[] { typeof(double) });
        private static readonly MethodInfo CosMethod = typeof(Math).GetTypeInfo().GetMethod("Cos", new[] { typeof(double) });
        private static readonly MethodInfo TanMethod = typeof(Math).GetTypeInfo().GetMethod("Tan", new[] { typeof(double) });
        private static readonly MethodInfo SquareMethod = typeof(ExpressionTreeDerivativesParserHelper).GetTypeInfo().GetMethod("Square", new[] { typeof(double) });

        /// <summary>
        /// Applies the sine.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The sine result.</returns>
        public static Derivatives<Expression> ApplySin(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new ExpressionTreeDerivatives(arg.Count);
            result[0] = Expression.Call(SinMethod, arg[0]);

            // Apply the chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                    result[i] = Expression.Multiply(Expression.Call(CosMethod, arg[0]), arg[i]);
            }
            return result;
        }

        /// <summary>
        /// Applies the cosine.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The cosine result.</returns>
        private static Derivatives<Expression> ApplyCos(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new ExpressionTreeDerivatives(arg.Count);
            result[0] = Expression.Call(CosMethod, arg[0]);

            // Apply the chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                    result[i] = Expression.Negate(Expression.Multiply(Expression.Call(SinMethod, arg[0]), arg[i]));
            }
            return result;
        }

        /// <summary>
        /// Applies the tangent.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The tangent result.</returns>
        private static Derivatives<Expression> ApplyTan(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new ExpressionTreeDerivatives(arg.Count);
            result[0] = Expression.Call(TanMethod, arg[0]);

            // Apply chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                    result[i] = Expression.Divide(arg[i],
                        Expression.Call(SquareMethod, Expression.Call(CosMethod, arg[0])));
            }
            return result;
        }

        /// <summary>
        /// Hyperbolic Trigonometry
        /// </summary>
        private static readonly MethodInfo SinhMethod = typeof(Math).GetTypeInfo().GetMethod("Sinh", new[] { typeof(double) });
        private static readonly MethodInfo CoshMethod = typeof(Math).GetTypeInfo().GetMethod("Cosh", new[] { typeof(double) });
        private static readonly MethodInfo TanhMethod = typeof(Math).GetTypeInfo().GetMethod("Tanh", new[] { typeof(double) });

        /// <summary>
        /// Applies the hyperbolic sine.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The hyperbolic sine result.</returns>
        public static Derivatives<Expression> ApplySinh(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new ExpressionTreeDerivatives(arg.Count);
            result[0] = Expression.Call(SinhMethod, arg[0]);

            // Apply the chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                    result[i] = Expression.Multiply(Expression.Call(CoshMethod, arg[0]), arg[i]);
            }
            return result;
        }

        /// <summary>
        /// Applies the hyperbolic cosine.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The hyperbolic cosine result.</returns>
        private static Derivatives<Expression> ApplyCosh(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new ExpressionTreeDerivatives(arg.Count);
            result[0] = Expression.Call(CoshMethod, arg[0]);

            // Apply the chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                    result[i] = Expression.Multiply(Expression.Call(SinhMethod, arg[0]), arg[i]);
            }
            return result;
        }

        /// <summary>
        /// Applies the hyperbolic tangent.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The hyperbolic tangent result.</returns>
        private static Derivatives<Expression> ApplyTanh(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new ExpressionTreeDerivatives(arg.Count);
            result[0] = Expression.Call(TanhMethod, arg[0]);

            // Apply chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                    result[i] = Expression.Divide(arg[i],
                        (Expression.Multiply(Expression.Call(CoshMethod, arg[0]),
                            Expression.Call(CoshMethod, arg[0]))));
            }
            return result;
        }

        /// <summary>
        /// Inverse trigonometry.
        /// </summary>
        private static readonly MethodInfo AsinMethod = typeof(Math).GetTypeInfo().GetMethod("Asin", new[] { typeof(double) });
        private static readonly MethodInfo AcosMethod = typeof(Math).GetTypeInfo().GetMethod("Acos", new[] { typeof(double) });
        private static readonly MethodInfo AtanMethod = typeof(Math).GetTypeInfo().GetMethod("Atan", new[] { typeof(double) });

        /// <summary>
        /// Applies the arcsin.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The arcsine result.</returns>
        public static Derivatives<Expression> ApplyAsin(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new ExpressionTreeDerivatives(arg.Count);
            result[0] = Expression.Call(AsinMethod, arg[0]);

            // Apply chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                    result[i] = Expression.Divide(arg[i],
                        Expression.Call(SqrtMethod, Expression.Subtract(Expression.Constant(1.0),
                        Expression.Call(SquareMethod, arg[0]))));
            }
            return result;
        }

        /// <summary>
        /// Applies the arccosine.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The arccosine result.</returns>
        public static Derivatives<Expression> ApplyAcos(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new ExpressionTreeDerivatives(arg.Count);
            result[0] = Expression.Call(AcosMethod, arg[0]);

            // Apply chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                    result[i] = Expression.Divide(Expression.Negate(arg[i]),
                        Expression.Call(SqrtMethod, Expression.Subtract(Expression.Constant(1.0),
                        Expression.Call(SquareMethod, arg[0]))));
            }
            return result;
        }

        /// <summary>
        /// Applies the arctangent.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The arctangent result.</returns>
        public static Derivatives<Expression> ApplyAtan(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new ExpressionTreeDerivatives(arg.Count);
            result[0] = Expression.Call(AtanMethod, arg[0]);

            // Apply chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                    result[i] = Expression.Divide(arg[i], Expression.Add(Expression.Constant(1.0),
                        Expression.Call(SquareMethod, arg[0])));
            }
            return result;
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
        public static Derivatives<Expression> ApplyAbs(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfNot(nameof(arguments), 1);
            var arg = arguments[0];
            var result = new ExpressionTreeDerivatives(arg.Count);
            result[0] = Expression.Call(AbsMethod, arg[0]);

            // Apply chain rule
            for (var i = 1; i < arg.Count; i++)
            {
                if (arg[i] != null)
                    result[i] = Expression.Multiply(arg[i], 
                        Expression.Condition(
                            Expression.GreaterThan(arg[0], Expression.Constant(0.0)),
                            Expression.Constant(1.0),
                            Expression.Condition(
                                Expression.LessThan(arg[0], Expression.Constant(0.0)),
                                Expression.Constant(-1.0),
                                Expression.Constant(0.0))));
            }
            return result;
        }

        /// <summary>
        /// Rounds the derivatives.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The rounding result.</returns>
        public static Derivatives<Expression> ApplyRound(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            var arg = arguments[0];
            if (arguments.Length == 1)
            {
                var result = new ExpressionTreeDerivatives();
                result[0] = Expression.Call(RoundMethod, arg[0]);

                for (var i = 1; i < arg.Count; i++)
                    if (arg[i] != null)
                        throw new CircuitException("Cannot differentiate Round()");
                return result;
            }
            if (arguments.Length == 2)
            {
                var result = new ExpressionTreeDerivatives();
                result[0] = Expression.Call(Round2Method, arg[0], Expression.Convert(Expression.Call(RoundMethod, arguments[1][0]), typeof(int)));

                for (var i = 1; i < arg.Count; i++)
                    if (arg[i] != null)
                        throw new CircuitException("Cannot differentiate Round()");
                for (var i = 1; i < arguments[1].Count; i++)
                    if (arguments[1][i] != null)
                        throw new CircuitException("Cannot differentiate Round()");
                return result;
            }
            throw new CircuitException("Invalid number of arguments for Round()");
        }

        /// <summary>
        /// Applies the minimum.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The minimum result.</returns>
        public static Derivatives<Expression> ApplyMin(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            var size = 1;
            for (var i = 0; i < arguments.Length; i++)
                size = Math.Max(size, arguments[i].Count);
            var result = new ExpressionTreeDerivatives(size);
            if (arguments.Length == 1)
            {
                result[0] = arguments[0][0];
                return result;
            }

            {
                // First two arguments
                var a = arguments[0][0];
                var b = arguments[1][0];
                result[0] = Expression.Call(MinMethod, a, b);
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
                            result[k] = Expression.Condition(Expression.LessThan(funca, funcb), tmpda, tmpdb); // Use the derivative of the function that is currently the smallest
                        else if (tmpda != null)
                            result[k] = Expression.Condition(Expression.LessThan(funca, funcb), tmpda, Expression.Constant(0.0));
                        else if (tmpdb != null)
                            result[k] = Expression.Condition(Expression.LessThan(funca, funcb), Expression.Constant(0.0), tmpdb);
                    }
                }
            }

            for (var i = 2; i < arguments.Length; i++)
            {
                // First two arguments
                var a = result[0];
                var b = arguments[i][0];
                result[0] = Expression.Call(MinMethod, a, b);
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
                            result[k] = Expression.Condition(Expression.LessThan(funca, funcb), tmpda, tmpdb); // Use the derivative of the function that is currently the smallest
                        else if (tmpda != null)
                            result[k] = Expression.Condition(Expression.LessThan(funca, funcb), tmpda, Expression.Constant(0.0));
                        else if (tmpdb != null)
                            result[k] = Expression.Condition(Expression.LessThan(funca, funcb), Expression.Constant(0.0), tmpdb);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Applies the maximum.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The maximum result.</returns>
        public static Derivatives<Expression> ApplyMax(Derivatives<Expression>[] arguments)
        {
            arguments.ThrowIfEmpty(nameof(arguments));
            var size = 1;
            for (var i = 0; i < arguments.Length; i++)
                size = Math.Max(size, arguments[i].Count);
            var result = new ExpressionTreeDerivatives(size);
            if (arguments.Length == 1)
            {
                result[0] = arguments[0][0];
                return result;
            }

            {
                // First two arguments
                var a = arguments[0][0];
                var b = arguments[1][0];
                result[0] = Expression.Call(MaxMethod, a, b);
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
                            result[k] = Expression.Condition(Expression.GreaterThan(funca, funcb), tmpda, tmpdb); // Use the derivative of the function that is currently the smallest
                        else if (tmpda != null)
                            result[k] = Expression.Condition(Expression.GreaterThan(funca, funcb), tmpda, Expression.Constant(0.0));
                        else if (tmpdb != null)
                            result[k] = Expression.Condition(Expression.GreaterThan(funca, funcb), Expression.Constant(0.0), tmpdb);
                    }
                }
            }

            for (var i = 2; i < arguments.Length; i++)
            {
                // First two arguments
                var a = result[0];
                var b = arguments[i][0];
                result[0] = Expression.Call(MaxMethod, a, b);
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
                            result[k] = Expression.Condition(Expression.GreaterThan(funca, funcb), tmpda, tmpdb); // Use the derivative of the function that is currently the smallest
                        else if (tmpda != null)
                            result[k] = Expression.Condition(Expression.GreaterThan(funca, funcb), tmpda, Expression.Constant(0.0));
                        else if (tmpdb != null)
                            result[k] = Expression.Condition(Expression.GreaterThan(funca, funcb), Expression.Constant(0.0), tmpdb);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Squares the specified value.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The squared value.</returns>
        public static double Square(double x) => x * x;

        /// <summary>
        /// Delegate for applying a function.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The result of the function.</returns>
        public delegate Derivatives<Expression> ExpressionTreeDerivativesFunction(Derivatives<Expression>[] arguments);
    }
}
