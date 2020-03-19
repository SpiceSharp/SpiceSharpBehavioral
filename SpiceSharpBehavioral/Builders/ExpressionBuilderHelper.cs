using SpiceSharpBehavioral.Parsers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SpiceSharpBehavioral.Diagnostics;
using System.Reflection;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// A helper class for <see cref="IBuilder{Expression}"/>.
    /// </summary>
    public static class ExpressionBuilderHelper
    {
        private readonly static Expression _one = Expression.Constant(1.0);
        private readonly static MethodInfo _absMethod = new Func<double, double>(Math.Abs).GetMethodInfo();
        private readonly static MethodInfo _sgnMethod = new Func<double, double>(Functions.Sign).GetMethodInfo();
        private readonly static MethodInfo _sqrtMethod = new Func<double, double>(Functions.Sqrt).GetMethodInfo();
        private readonly static MethodInfo _pwrMethod = new Func<double, double, double>(Functions.Power2).GetMethodInfo();
        private readonly static MethodInfo _logMethod = new Func<double, double>(Functions.Log).GetMethodInfo();
        private readonly static MethodInfo _log10Method = new Func<double, double>(Functions.Log10).GetMethodInfo();
        private readonly static MethodInfo _expMethod = new Func<double, double>(Math.Exp).GetMethodInfo();
        private readonly static MethodInfo _minMethod = new Func<double, double, double>(Math.Min).GetMethodInfo();
        private readonly static MethodInfo _maxMethod = new Func<double, double, double>(Math.Max).GetMethodInfo();
        private readonly static MethodInfo _sinMethod = new Func<double, double>(Math.Sin).GetMethodInfo();
        private readonly static MethodInfo _cosMethod = new Func<double, double>(Math.Cos).GetMethodInfo();
        private readonly static MethodInfo _tanMethod = new Func<double, double>(Math.Tan).GetMethodInfo();
        private readonly static MethodInfo _sinhMethod = new Func<double, double>(Math.Sinh).GetMethodInfo();
        private readonly static MethodInfo _coshMethod = new Func<double, double>(Math.Cosh).GetMethodInfo();
        private readonly static MethodInfo _tanhMethod = new Func<double, double>(Math.Tanh).GetMethodInfo();
        private readonly static MethodInfo _asinMethod = new Func<double, double>(Math.Asin).GetMethodInfo();
        private readonly static MethodInfo _acosMethod = new Func<double, double>(Math.Acos).GetMethodInfo();
        private readonly static MethodInfo _atanMethod = new Func<double, double>(Math.Atan).GetMethodInfo();
        private readonly static MethodInfo _ustepMethod = new Func<double, double>(Functions.Step).GetMethodInfo();
        private readonly static MethodInfo _ustep2Method = new Func<double, double>(Functions.Step2).GetMethodInfo();
        private readonly static MethodInfo _ustep2DerivativeMethod = new Func<double, double>(Functions.Step2Derivative).GetMethodInfo();
        private readonly static MethodInfo _urampMethod = new Func<double, double>(Functions.Ramp).GetMethodInfo();
        private readonly static MethodInfo _urampDerivativeMethod = new Func<double, double>(Functions.RampDerivative).GetMethodInfo();
        private readonly static MethodInfo _ceilMethod = new Func<double, double>(Math.Ceiling).GetMethodInfo();
        private readonly static MethodInfo _floorMethod = new Func<double, double>(Math.Floor).GetMethodInfo();
        private readonly static MethodInfo _roundMethod = new Func<double, int, double>(Math.Round).GetMethodInfo();
        private readonly static MethodInfo _pwlMethod = new Func<double, Point[], double>(Functions.Pwl).GetMethodInfo();
        private readonly static MethodInfo _pwlDerivativeMethod = new Func<double, Point[], double>(Functions.PwlDerivative).GetMethodInfo();
        private readonly static MethodInfo _squareMethod = new Func<double, double>(Functions.Square).GetMethodInfo();
        private readonly static ConstructorInfo _ptConstructor = typeof(Point).GetTypeInfo().GetConstructor(new[] { typeof(double), typeof(double) });

        /// <summary>
        /// Gets or sets a value indicating whether function calls should be evaluated immediately.
        /// </summary>
        /// <value>
        ///   <c>true</c> if constants should be simplified; otherwise, <c>false</c>.
        /// </value>
        public static bool SimplifyConstants { get; set; } = true;

        /// <summary>
        /// Gets or sets the fudge factor.
        /// </summary>
        /// <value>
        /// The fudge factor.
        /// </value>
        public static double FudgeFactor { get; set; } = 1e-20;

        /// <summary>
        /// The function factories.
        /// </summary>
        public static Dictionary<string, FunctionBuilder> FunctionFactories = new Dictionary<string, FunctionBuilder>
        {
            { "abs", Abs }, { "dabs(0)", Sgn },
            { "sgn", Sgn }, { "dsgn(0)", Zero },
            { "sqrt", Sqrt }, { "dsqrt(0)", DSqrt0 },
            { "pwr", Pwr }, { "dpwr(0)", DPwr0 }, { "dpwr(1)", DPwr1 },
            { "log", Log }, { "dlog(0)", DLog0 },
            { "log10", Log10 }, { "dlog10(0)", DLog100 },
            { "exp", Exp }, { "dexp(0)", Exp },
            { "sin", Sin }, { "dsin(0)", Cos },
            { "cos", Cos }, { "dcos(0)", DCos0 },
            { "tan", Tan }, { "dtan(0)", DTan0 },
            { "sinh", Sinh }, { "dsinh(0)", Cosh },
            { "cosh", Cosh }, { "dcosh(0)", Sinh },
            { "tanh", Tanh }, { "dtanh(0)", DTanh0 },
            { "asin", Asin }, { "dasin(0)", DAsin0 },
            { "acos", Acos }, { "dacos(0)", DAcos0 },
            { "atan", Atan }, { "datan(0)", DAtan0 },
            { "u", U }, { "du(0)", Zero },
            { "u2", U2 }, { "du2(0)", DU20 },
            { "uramp", URamp }, { "duramp(0)", DUramp0 },
            { "ceil", Ceil }, { "dceil(0)", Zero },
            { "floor", Floor }, { "dfloor(0)", Zero },
            { "nint", Nint }, { "dnint(0)", Zero },
            { "round", Round }, { "dround(0)", Zero },
            { "square", Square }, { "dsquare(0)", DSquare0 },
            { "pwl", Pwl }, { "dpwl(0)", DPwl0 },
            { "min", Min },
            { "max", Max }
        };

        /// <summary>
        /// Called when a function was found.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="FunctionFoundEventArgs{Expression}"/> instance containing the event data.</param>
        public static void OnFunctionFound(object sender, FunctionFoundEventArgs<Expression> args)
        {
            if (sender is IBuilder<Expression> builder)
            {
                if (FunctionFactories.TryGetValue(args.Name, out var factory))
                    args.Result = factory(builder, args.Arguments);
            }
        }

        /// <summary>
        /// Registers the functions.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static void RegisterFunctions(this IEventBuilder<Expression> builder)
        {
            builder.FunctionFound += OnFunctionFound;
        }

        private static Expression Zero(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments) => Expression.Constant(0.0);
        private static Expression Abs(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Math.Abs((double)ce.Value));
            return Expression.Call(_absMethod, arguments[0]);
        }
        private static Expression Sgn(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Functions.Sign((double)ce.Value));
            return Expression.Call(_sgnMethod, arguments[0]);
        }
        private static Expression Sqrt(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Functions.Sqrt((double)ce.Value));
            return Expression.Call(_sqrtMethod, arguments[0]);
        }
        private static Expression DSqrt0(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(0.5 / Functions.Sqrt((double)ce.Value));
            return builder.Divide(Expression.Constant(0.5), Expression.Call(_sqrtMethod, arguments[0]));
        }
        private static Expression Pwr(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 2)
                throw new ArgumentMismatchException(2, arguments.Count);
            if (SimplifyConstants)
            {
                if (arguments[0] is ConstantExpression ce1 && arguments[1] is ConstantExpression ce2)
                    return Expression.Constant(Functions.Power2((double)ce1.Value, (double)ce2.Value));
            }
            return Expression.Call(_pwrMethod, arguments[0], arguments[1]);
        }
        private static Expression DPwr0(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 2)
                throw new ArgumentMismatchException(2, arguments.Count);
            if (SimplifyConstants)
            {
                if (arguments[0] is ConstantExpression ce1 && arguments[1] is ConstantExpression ce2)
                    return Expression.Constant((double)ce2.Value * Functions.Power2((double)ce1.Value, (double)ce2.Value - 1.0));
            }
            return Expression.Multiply(arguments[1], Expression.Call(_pwrMethod, arguments[0], builder.Subtract(arguments[1], _one)));
        }
        private static Expression DPwr1(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 2)
                throw new ArgumentMismatchException(2, arguments.Count);
            if (SimplifyConstants)
            {
                if (arguments[0] is ConstantExpression ce1 && arguments[1] is ConstantExpression ce2)
                    return Expression.Constant(Functions.Power2((double)ce1.Value, (double)ce2.Value) * Functions.Log((double)ce1.Value));
            }
            return Expression.Multiply(Expression.Call(_pwrMethod, arguments[0], arguments[1]), Expression.Call(_logMethod, arguments[0]));
        }
        private static Expression Log(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants)
            {
                if (arguments[0] is ConstantExpression ce)
                    return Expression.Constant(Functions.Log((double)ce.Value));
            }
            return Expression.Call(_logMethod, arguments[0]);
        }
        private static Expression DLog0(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Functions.SafeDivide(1.0, (double)ce.Value, FudgeFactor));
            return builder.Divide(_one, arguments[0]);
        }
        private static Expression Log10(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants)
            {
                if (arguments[0] is ConstantExpression ce)
                    return Expression.Constant(Functions.Log10((double)ce.Value));
            }
            return Expression.Call(_log10Method, arguments[0]);
        }
        private static Expression DLog100(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Functions.SafeDivide(1.0 / Functions.Log(10.0), (double)ce.Value, FudgeFactor));
            return builder.Divide(Expression.Constant(1.0 / Functions.Log(10.0)), arguments[0]);
        }
        private static Expression Exp(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Math.Exp((double)ce.Value));
            return Expression.Call(_expMethod, arguments[0]);
        }
        private static Expression Sin(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Math.Sin((double)ce.Value));
            return Expression.Call(_sinMethod, arguments[0]);
        }
        private static Expression Cos(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Math.Cos((double)ce.Value));
            return Expression.Call(_cosMethod, arguments[0]);
        }
        private static Expression DCos0(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments) => builder.Minus(Sin(builder, arguments));
        private static Expression Tan(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Math.Tan((double)ce.Value));
            return Expression.Call(_tanMethod, arguments[0]);
        }
        private static Expression DTan0(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(1.0 / Functions.Square(Math.Cos((double)ce.Value)));
            return builder.Divide(_one, Expression.Call(_squareMethod, Expression.Call(_cosMethod, arguments[0])));
        }
        private static Expression Sinh(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Math.Sinh((double)ce.Value));
            return Expression.Call(_sinhMethod, arguments[0]);
        }
        private static Expression Cosh(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Math.Cosh((double)ce.Value));
            return Expression.Call(_coshMethod, arguments[0]);
        }
        private static Expression Tanh(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Math.Tanh((double)ce.Value));
            return Expression.Call(_tanhMethod, arguments[0]);
        }
        private static Expression DTanh0(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(1.0 / Functions.Square(Math.Cosh((double)ce.Value)));
            return builder.Divide(_one, Expression.Call(_squareMethod, Expression.Call(_coshMethod, arguments[0])));
        }
        private static Expression Asin(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Math.Asin((double)ce.Value));
            return Expression.Call(_asinMethod, arguments[0]);
        }
        private static Expression DAsin0(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(1.0 / Functions.Sqrt(1 - Functions.Square((double)ce.Value)));
            return builder.Divide(
                    _one,
                    Expression.Call(_sqrtMethod, Expression.Subtract(
                        _one, 
                        Expression.Call(_squareMethod, arguments[0]))));
        }
        private static Expression Acos(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Math.Acos((double)ce.Value));
            return Expression.Call(_acosMethod, arguments[0]);
        }
        private static Expression DAcos0(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(-1.0 / Functions.Sqrt(1 - Functions.Square((double)ce.Value)));
            return builder.Divide(
                    Expression.Constant(-1.0),
                    Expression.Call(_sqrtMethod, Expression.Subtract(
                        _one,
                        Expression.Call(_squareMethod, arguments[0]))));
        }
        private static Expression Atan(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Math.Atan((double)ce.Value));
            return Expression.Call(_atanMethod, arguments[0]);
        }
        private static Expression DAtan0(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(1.0 / (1 + Functions.Square((double)ce.Value)));
            return builder.Divide(_one, Expression.Add(_one, Expression.Call(_squareMethod, arguments[0])));
        }
        private static Expression U(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Functions.Step((double)ce.Value));
            return Expression.Call(_ustepMethod, arguments[0]);
        }
        private static Expression U2(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Functions.Step2((double)ce.Value));
            return Expression.Call(_ustep2Method, arguments[0]);
        }
        private static Expression DU20(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Functions.Step2Derivative((double)ce.Value));
            return Expression.Call(_ustep2DerivativeMethod, arguments[0]);
        }
        private static Expression URamp(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Functions.Ramp((double)ce.Value));
            return Expression.Call(_urampMethod, arguments[0]);
        }
        private static Expression DUramp0(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Functions.RampDerivative((double)ce.Value));
            return Expression.Call(_urampDerivativeMethod, arguments[0]);
        }
        private static Expression Ceil(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Math.Ceiling((double)ce.Value));
            return Expression.Call(_ceilMethod, arguments[0]);
        }
        private static Expression Floor(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Math.Floor((double)ce.Value));
            return Expression.Call(_floorMethod, arguments[0]);
        }
        private static Expression Nint(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Math.Round((double)ce.Value, 0));
            return Expression.Call(_roundMethod, arguments[0], Expression.Constant(0));
        }
        private static Expression Round(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 2)
                throw new ArgumentMismatchException(2, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce1 && arguments[1] is ConstantExpression ce2)
                return Expression.Constant(Math.Round((double)ce1.Value, (int)(double)ce2.Value));
            return Expression.Call(_roundMethod, arguments[0], Expression.Convert(arguments[1], typeof(int)));
        }
        private static Expression Square(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Functions.Square((double)ce.Value));
            return Expression.Call(_squareMethod, arguments[0]);
        }
        private static Expression DSquare0(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 1)
                throw new ArgumentMismatchException(1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(2 * (double)ce.Value);
            return builder.Multiply(Expression.Constant(2.0), arguments[0]);
        }
        private static Expression Pwl(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count < 3)
                throw new ArgumentMismatchException(3, arguments.Count);
            int points = (arguments.Count - 1) / 2;
            if (arguments.Count % 2 == 0)
                throw new ArgumentMismatchException(points * 2 + 1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Math.Asin((double)ce.Value));

            var initArguments = new Expression[points];
            for (var i = 0; i < points; i++)
                initArguments[i] = Expression.New(_ptConstructor, arguments[i * 2 + 1], arguments[i * 2 + 2]);
            var arr = Expression.NewArrayInit(typeof(Point), initArguments);
            return Expression.Call(_pwlMethod, arguments[0], arr);
        }
        private static Expression DPwl0(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count < 3)
                throw new ArgumentMismatchException(3, arguments.Count);
            int points = (arguments.Count - 1) / 2;
            if (arguments.Count % 2 == 0)
                throw new ArgumentMismatchException(points * 2 + 1, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce)
                return Expression.Constant(Math.Asin((double)ce.Value));

            var initArguments = new Expression[points];
            for (var i = 0; i < points; i++)
                initArguments[i] = Expression.New(_ptConstructor, arguments[i * 2 + 1], arguments[i * 2 + 2]);
            var arr = Expression.NewArrayInit(typeof(Point), initArguments);
            return Expression.Call(_pwlDerivativeMethod, arguments[0], arr);
        }
        private static Expression Min(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 2)
                throw new ArgumentMismatchException(2, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce1 && arguments[1] is ConstantExpression ce2)
                return Expression.Constant(Math.Min((double)ce1.Value, (double)ce2.Value));
            return Expression.Call(_minMethod, arguments[0], arguments[1]);
        }
        private static Expression Max(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != 2)
                throw new ArgumentMismatchException(2, arguments.Count);
            if (SimplifyConstants && arguments[0] is ConstantExpression ce1 && arguments[1] is ConstantExpression ce2)
                return Expression.Constant(Math.Max((double)ce1.Value, (double)ce2.Value));
            return Expression.Call(_maxMethod, arguments[0], arguments[1]);
        }

        /// <summary>
        /// A delegate for building functions.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The expression.</returns>
        public delegate Expression FunctionBuilder(IBuilder<Expression> builder, IReadOnlyList<Expression> arguments);
    }
}
