using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// This is an expression builder for double values for Spice behavioral models.
    /// </summary>
    /// <remarks>
    /// In order to avoid numerical problems, some functions are slightly modified. For
    /// example, division by 0 can be avoided using a fudge factor. Functions are also
    /// made antisymmetric.
    /// </remarks>
    /// <seealso cref="IBuilder{Expression}" />
    public class ExpressionBuilder : BaseBuilder<Expression>, IBuilder<Expression>
    {
        private readonly static Expression _one = Expression.Constant(1.0);
        private readonly static Expression _zero = Expression.Constant(0.0);
        private readonly static MethodInfo _divMethod = typeof(ExpressionBuilder).GetTypeInfo().GetMethod(nameof(SafeDivide), BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly static MethodInfo _absMethod = new Func<double, double>(Math.Abs).GetMethodInfo();
        private readonly static MethodInfo _sgnMethod = new Func<double, double>(Functions.Sign).GetMethodInfo();
        private readonly static MethodInfo _sqrtMethod = new Func<double, double>(Functions.Sqrt).GetMethodInfo();
        private readonly static MethodInfo _powMethod = new Func<double, double, double>(Functions.Power).GetMethodInfo();
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
        /// Gets or sets the simulation that the builder should use.
        /// </summary>
        /// <value>
        /// The simulation.
        /// </value>
        public ISimulation Simulation { get; set; }

        /// <summary>
        /// Creates an expression from the specified function.
        /// </summary>
        /// <param name="func">The function.</param>
        /// <returns>The result.</returns>
        public static Expression From(Func<double> func)
        {
            return Expression.Call(Expression.Constant(func.Target), func.GetMethodInfo());
        }

        /// <summary>
        /// Gets or sets a value indicating whether operations on constants should be simplified.
        /// </summary>
        /// <value>
        ///   <c>true</c> if operations on constants should be simplified; otherwise, <c>false</c>.
        /// </value>
        public bool SimplifyConstants { get; set; } = true;

        /// <summary>
        /// Gets or sets the fudge factor.
        /// </summary>
        /// <value>
        /// The fudge factor.
        /// </value>
        public double FudgeFactor { get; set; } = 1e-20;

        /// <summary>
        /// Safe division using a fudge factor.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the division.</returns>
        protected double SafeDivide(double left, double right)
        {
            if (right < 0)
                right -= FudgeFactor;
            else
                right += FudgeFactor;
            if (right.Equals(0.0))
                return double.PositiveInfinity;
            return left / right;
        }

        Expression IBuilder<Expression>.Plus(Expression argument) => argument;
        Expression IBuilder<Expression>.Minus(Expression argument)
        {
            if (SimplifyConstants)
            {
                if (argument is ConstantExpression ceArgument)
                    return Expression.Constant(-(double)ceArgument.Value);
            }
            return Expression.Negate(argument);
        }
        Expression IBuilder<Expression>.Add(Expression left, Expression right)
        {
            if (SimplifyConstants)
            {
                if (left is ConstantExpression ceLeft)
                {
                    // Left and Right are constant, calculate the result immediately
                    if (right is ConstantExpression ceRight)
                        return Expression.Constant((double)ceLeft.Value + (double)ceRight.Value);

                    // 0 + Right = Right
                    else if (((double)ceLeft.Value).Equals(0.0))
                        return right;
                }

                // Left + 0 = Left
                else if (right is ConstantExpression ceRight && ((double)ceRight.Value).Equals(0.0))
                    return left;
            }

            // General addition
            return Expression.Add(left, right);
        }
        Expression IBuilder<Expression>.Subtract(Expression left, Expression right)
        {
            if (SimplifyConstants)
            {
                if (left is ConstantExpression ceLeft)
                {
                    double lv = (double)ceLeft.Value;

                    // Both are constant, pre-evaluate
                    if (right is ConstantExpression ceRight)
                        return Expression.Constant(lv - (double)ceRight.Value);

                    // 0 - Right = -Right
                    if (lv.Equals(0.0))
                        return Expression.Negate(right);
                }
                else if (right is ConstantExpression ceRight)
                {
                    double rv = (double)ceRight.Value;

                    // Left - 0 = Left
                    if (rv.Equals(0.0))
                        return left;
                }
            }

            // General subtraction
            return Expression.Subtract(left, right);
        }
        Expression IBuilder<Expression>.Multiply(Expression left, Expression right)
        {
            if (SimplifyConstants)
            {
                if (left is ConstantExpression ceLeft)
                {
                    double lv = (double)ceLeft.Value;
                    if (right is ConstantExpression ceRight)
                    {
                        double rv = (double)ceRight.Value;
                        return Expression.Constant(lv * rv);
                    }
                    else if (lv.Equals(0.0))
                        return _zero;
                    else if (lv.Equals(1.0))
                        return right;
                    else if (lv.Equals(-1.0))
                        return Expression.Negate(right);
                }
                else if (right is ConstantExpression ceRight)
                {
                    double rv = (double)ceRight.Value;
                    if (rv.Equals(0.0))
                        return _zero;
                    else if (rv.Equals(1.0))
                        return left;
                    else if (rv.Equals(-1.0))
                        return Expression.Negate(left);
                }
            }
            return Expression.Multiply(left, right);
        }
        Expression IBuilder<Expression>.Divide(Expression left, Expression right)
        {
            if (SimplifyConstants)
            {
                if (right is ConstantExpression ceRight)
                {
                    double rv = (double)ceRight.Value;
                    if (left is ConstantExpression ceLeft)
                        return Expression.Constant((double)ceLeft.Value / rv);
                    else if (rv.Equals(0.0))
                        return Expression.Constant(double.PositiveInfinity);
                    else
                        return Expression.Divide(left, ceRight);
                }
            }
            return Expression.Call(Expression.Constant(this), _divMethod, left, right);
        }
        Expression IBuilder<Expression>.Pow(Expression left, Expression right)
        {
            if (SimplifyConstants)
            {
                if (right is ConstantExpression ceRight)
                {
                    double rv = (double)ceRight.Value;
                    if (left is ConstantExpression ceLeft)
                    {
                        double lv = (double)ceLeft.Value;
                        return Expression.Constant(Functions.Power(lv, rv));
                    }
                    if (rv.Equals(0.0))
                        return Expression.Constant(1.0);
                    else if (rv.Equals(1.0))
                        return left;
                }
                else if (left is ConstantExpression ceLeft)
                {
                    double lv = (double)ceLeft.Value;
                    if (lv.Equals(1.0))
                        return _one;
                }
            }
            return Expression.Call(null, _powMethod, left, right);
        }
        Expression IBuilder<Expression>.Mod(Expression left, Expression right)
        {
            if (SimplifyConstants)
            {
                if (left is ConstantExpression ceLeft && right is ConstantExpression ceRight)
                    return Expression.Constant((double)ceLeft.Value % (double)ceRight.Value);
            }
            return Expression.Modulo(left, right);
        }
        Expression IBuilder<Expression>.And(Expression left, Expression right)
        {
            if (SimplifyConstants)
            {
                if (left is ConstantExpression ceLeft)
                {
                    double lv = (double)ceLeft.Value;

                    // Both left and right are constants
                    if (right is ConstantExpression ceRight)
                        return lv.Equals(0.0) || ((double)ceRight.Value).Equals(0.0) ? _zero : _one;
                    else
                    {
                        // Left && 0 = 0
                        if (lv.Equals(0.0))
                            return _zero;

                        // Left && 1 == Left
                        else
                            return Expression.Condition(Expression.Equal(right, _zero), _zero, _one);
                    }
                }
                else if (right is ConstantExpression ceRight)
                {
                    double rv = (double)ceRight.Value;

                    // 0 && Right = 0
                    if (rv.Equals(0.0))
                        return _zero;

                    // 1 && Right = Right
                    else
                        return Expression.Condition(Expression.Equal(left, _zero), _zero, _one);
                }
            }

            // General case
            return Expression.Condition(
                Expression.Or(
                    Expression.Equal(left, _zero),
                    Expression.Equal(right, _zero)
                ), _zero, _one);
        }
        Expression IBuilder<Expression>.Or(Expression left, Expression right)
        {
            if (SimplifyConstants)
            {
                if (left is ConstantExpression ceLeft)
                {
                    double lv = (double)ceLeft.Value;

                    // Both left and right are constant
                    if (right is ConstantExpression ceRight)
                        return lv.Equals(0.0) && ((double)ceRight.Value).Equals(0.0) ? _zero : _one;

                    // 0 || Right = Right
                    else if (lv.Equals(0.0))
                        return Expression.Condition(Expression.Equal(right, _zero), _zero, _one);

                    // 1 || Right = 1
                    else
                        return _one;
                }
                else if (right is ConstantExpression ceRight)
                {
                    double rv = (double)ceRight.Value;

                    // Left || 0 = Left
                    if (rv.Equals(0.0))
                        return Expression.Condition(Expression.Equal(left, _zero), _zero, _one);

                    // Left || 1 = 1
                    else
                        return _one;
                }
            }

            // General case
            return Expression.Condition(
                Expression.And(
                    Expression.Equal(left, _zero),
                    Expression.Equal(right, _zero)
                ), _zero, _one);
        }
        Expression IBuilder<Expression>.Conditional(Expression condition, Expression ifTrue, Expression ifFalse)
        {
            if (SimplifyConstants)
            {
                // Condition is already determined
                if (condition is ConstantExpression ceCondition)
                    return ((double)ceCondition.Value).Equals(0.0) ? ifFalse : ifTrue;
            }
            return Expression.Condition(Expression.Equal(condition, _zero), ifFalse, ifTrue);
        }
        Expression IBuilder<Expression>.Equals(Expression left, Expression right)
        {
            if (SimplifyConstants)
            {
                if (left is ConstantExpression ceLeft && right is ConstantExpression ceRight)
                    return (double)ceLeft.Value == (double)ceRight.Value ? _one : _zero;
            }
            return Expression.Condition(Expression.Equal(left, right), _one, _zero);
        }
        Expression IBuilder<Expression>.NotEquals(Expression left, Expression right)
        {
            if (SimplifyConstants)
            {
                if (left is ConstantExpression ceLeft && right is ConstantExpression ceRight)
                    return (double)ceLeft.Value != (double)ceRight.Value ? _one : _zero;
            }
            return Expression.Condition(Expression.NotEqual(left, right), _one, _zero);
        }
        Expression IBuilder<Expression>.GreaterThanOrEqual(Expression left, Expression right)
        {
            if (SimplifyConstants)
            {
                if (left is ConstantExpression ceLeft && right is ConstantExpression ceRight)
                    return (double)ceLeft.Value >= (double)ceRight.Value ? _one : _zero;
            }
            return Expression.Condition(Expression.GreaterThanOrEqual(left, right), _one, _zero);
        }
        Expression IBuilder<Expression>.GreaterThan(Expression left, Expression right)
        {
            if (SimplifyConstants)
            {
                if (left is ConstantExpression ceLeft && right is ConstantExpression ceRight)
                    return (double)ceLeft.Value > (double)ceRight.Value ? _one : _zero;
            }
            return Expression.Condition(Expression.GreaterThan(left, right), _one, _zero);
        }
        Expression IBuilder<Expression>.LessThanOrEqual(Expression left, Expression right)
        {
            if (SimplifyConstants)
            {
                if (left is ConstantExpression ceLeft && right is ConstantExpression ceRight)
                    return (double)ceLeft.Value <= (double)ceRight.Value ? _one : _zero;
            }
            return Expression.Condition(Expression.LessThanOrEqual(left, right), _one, _zero);
        }
        Expression IBuilder<Expression>.LessThan(Expression left, Expression right)
        {
            if (SimplifyConstants)
            {
                if (left is ConstantExpression ceLeft && right is ConstantExpression ceRight)
                    return (double)ceLeft.Value < (double)ceRight.Value ? _one : _zero;
            }
            return Expression.Condition(Expression.LessThan(left, right), _one, _zero);
        }

        /// <summary>
        /// Creates the value of a function.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The value of the function.
        /// </returns>
        public override Expression CreateFunction(string name, IReadOnlyList<Expression> arguments)
        {
            // Check the number of arguments
            switch (name)
            {
                case "abs":
                case "sgn":
                case "dabs(0)":
                case "sqrt":
                case "dsqrt(0)":
                case "log":
                case "dlog(0)":
                case "log10":
                case "dlog10(0)":
                case "exp":
                case "dexp(0)":
                case "sin":
                case "dsin(0)":
                case "cos":
                case "dcos(0)":
                case "tan":
                case "dtan(0)":
                case "asin":
                case "dasin(0)":
                case "acos":
                case "dacos(0)":
                case "atan":
                case "datan(0)":
                case "sinh":
                case "dsinh(0)":
                case "cosh":
                case "dcosh(0)":
                case "tanh":
                case "dtanh(0)":
                case "u":
                case "du(0)":
                case "u2":
                case "du2(0)":
                case "uramp":
                case "duramp(0)":
                case "ceil":
                case "dceil(0)":
                case "floor":
                case "dfloor(0)":
                case "nint":
                case "dnint(0)":
                case "square":
                case "dsquare(0)":
                    if (arguments.Count != 1)
                        throw new Exception("Invalid number of arguments for {0}()".FormatString(name));
                    break;
                case "pwr":
                case "dpwr(0)":
                case "dpwr(1)":
                case "min":
                case "max":
                case "round":
                case "dround(0)":
                case "dround(1)":
                    if (arguments.Count != 2)
                        throw new Exception("Invalid number of arguments for {0}()".FormatString(name));
                    break;
                case "pwl":
                case "dpwl(0)":
                    if (arguments.Count < 3 || arguments.Count % 2 != 1)
                        throw new Exception("Invalid number of arguments for pwl()");
                    break;
            }
            if (SimplifyConstants)
            {
                bool isConstant = true;
                ConstantExpression[] ces = new ConstantExpression[arguments.Count];
                for (var i = 0; i < arguments.Count; i++)
                {
                    if (arguments[i] is ConstantExpression ce)
                        ces[i] = ce;
                    else
                    {
                        isConstant = false;
                        break;
                    }
                }
                if (isConstant)
                {
                    double v = (double)ces[0].Value, v2;
                    switch (name)
                    {
                        case "abs": return Expression.Constant(Math.Abs(v));
                        case "sgn":
                        case "dabs(0)": return Expression.Constant(Functions.Sign(v)); // abs(x)' = sgn(x)
                        case "sqrt": return Expression.Constant(Functions.Sqrt(v));
                        case "dsqrt(0)": return Expression.Constant(SafeDivide(0.5, Functions.Sqrt(v))); // sqrt(x)' = 1/(2*sqrt(x))
                        case "pwr": return Expression.Constant(Functions.Power2(v, (double)ces[1].Value));
                        case "dpwr(0)":
                            v2 = (double)ces[1].Value;
                            return Expression.Constant(v2 * Functions.Power2(v, v2 - 1.0)); // (x^n)' = n*x^(n-1)
                        case "dpwr(1)":
                            v2 = (double)ces[1].Value;
                            return Expression.Constant(Functions.Power2(v, v2) * Functions.Log(v)); // (a^x)' = a^x*ln(a)
                        case "min": return Expression.Constant(Math.Min(v, (double)ces[1].Value));
                        case "max": return Expression.Constant(Math.Max(v, (double)ces[1].Value));
                        case "log": return Expression.Constant(Functions.Log(v));
                        case "dlog(0)": return Expression.Constant(SafeDivide(1.0, v));
                        case "log10": return Expression.Constant(Functions.Log10(v));
                        case "dlog10(0)": return Expression.Constant(SafeDivide(1.0 / Functions.Log(10.0), v));
                        case "exp":
                        case "dexp(0)": return Expression.Constant(Math.Exp(v));
                        case "sin": return Expression.Constant(Math.Sin(v));
                        case "dsin(0)":
                        case "cos": return Expression.Constant(Math.Cos(v));
                        case "dcos(0)": return Expression.Constant(-Math.Sin(v));
                        case "tan": return Expression.Constant(Math.Tan(v));
                        case "dtan(0)": return Expression.Constant(SafeDivide(1.0, Functions.Square(Math.Cos(v))));
                        case "asin": return Expression.Constant(Math.Asin(v));
                        case "dasin(0)": return Expression.Constant(SafeDivide(1.0, Functions.Sqrt(1 - v * v)));
                        case "acos": return Expression.Constant(Math.Acos(v));
                        case "dacos(0)": return Expression.Constant(SafeDivide(-1.0, Functions.Sqrt(1 - v * v)));
                        case "atan": return Expression.Constant(Math.Atan(v));
                        case "datan(0)": return Expression.Constant(SafeDivide(1.0, (1 + v * v)));
                        case "sinh":
                        case "dcosh(0)": return Expression.Constant(Math.Sinh(v));
                        case "cosh":
                        case "dsinh(0)": return Expression.Constant(Math.Cosh(v));
                        case "tanh": return Expression.Constant(Math.Tanh(v));
                        case "dtanh(0)": return Expression.Constant(SafeDivide(1, Functions.Square(Math.Cosh(v))));
                        case "u": return Expression.Constant(Functions.Step(v));
                        case "u2": return Expression.Constant(Functions.Step2(v));
                        case "du2(0)": return Expression.Constant(Functions.Step2Derivative(v));
                        case "uramp": return Expression.Constant(Functions.Ramp(v));
                        case "duramp(0)": return Expression.Constant(Functions.RampDerivative(v));
                        case "ceil": return Expression.Constant(Math.Ceiling(v));
                        case "floor": return Expression.Constant(Math.Floor(v));
                        case "nint": return Expression.Constant(Math.Round(v, 0));
                        case "round": return Expression.Constant(Math.Round(v, (int)(double)ces[1].Value));
                        case "square": return Expression.Constant(Functions.Square(v));
                        case "dsquare(0)": return Expression.Constant(2 * v);
                        case "pwl":
                        case "dpwl(0)":
                            int points = (arguments.Count - 1) / 2;
                            var initArguments = new Point[points];
                            for (var i = 0; i < points; i++)
                                initArguments[i] = new Point((double)ces[2 * i + 1].Value, (double)ces[2 * i + 2].Value);
                            if (name == "pwl")
                                return Expression.Constant(Functions.Pwl(v, initArguments));
                            else
                                return Expression.Constant(Functions.PwlDerivative(v, initArguments));

                        case "du(0)":
                        case "dceil(0)":
                        case "dfloor(0)":
                        case "dnint(0)":
                        case "dround(0)":
                        case "dround(1)":
                            return Expression.Constant(0.0);
                    }
                }
            }

            var builder = (IBuilder<Expression>)this;
            switch (name)
            {
                case "abs": return Expression.Call(_absMethod, arguments[0]);
                case "sgn":
                case "dabs(0)": return Expression.Call(_sgnMethod, arguments[0]);
                case "sqrt": return Expression.Call(_sqrtMethod, arguments[0]);
                case "dsqrt(0)": return builder.Divide(Expression.Constant(0.5), Expression.Call(_sqrtMethod, arguments[0]));
                case "pwr": return Expression.Call(_pwrMethod, arguments[0], arguments[1]);
                case "dpwr(0)": return Expression.Multiply(arguments[1], Expression.Call(_pwrMethod, arguments[0], builder.Subtract(arguments[1], _one)));
                case "dpwr(1)": return Expression.Multiply(Expression.Call(_pwrMethod, arguments[0], arguments[1]), Expression.Call(_logMethod, arguments[0]));
                case "min": return Expression.Call(_minMethod, arguments[0], arguments[1]);
                case "max": return Expression.Call(_maxMethod, arguments[0], arguments[1]);
                case "log": return Expression.Call(_logMethod, arguments[0]);
                case "dlog(0)": return builder.Divide(_one, arguments[0]);
                case "log10": return Expression.Call(_log10Method, arguments[0]);
                case "dlog10(0)": return builder.Divide(Expression.Constant(1.0 / Functions.Log(10.0)), arguments[0]);
                case "exp":
                case "dexp(0)": return Expression.Call(_expMethod, arguments[0]);
                case "sin": return Expression.Call(_sinMethod, arguments[0]);
                case "dsin(0)":
                case "cos": return Expression.Call(_cosMethod, arguments[0]);
                case "dcos(0)": return Expression.Negate(Expression.Call(_sinMethod, arguments[0]));
                case "tan": return Expression.Call(_tanMethod, arguments[0]);
                case "dtan(0)": return builder.Divide(_one, Expression.Call(_squareMethod, new[] { Expression.Call(_cosMethod, arguments[0]) }));
                case "asin": return Expression.Call(_asinMethod, arguments[0]);
                case "dasin(0)": return builder.Divide(
                    _one, 
                    Expression.Call(_sqrtMethod, new[] { Expression.Subtract(_one, Expression.Call(_squareMethod, new[] { arguments[0] })) }));
                case "acos": return Expression.Call(_acosMethod, arguments[0]);
                case "dacos(0)": return builder.Divide(
                    Expression.Constant(-1.0), 
                    Expression.Call(_sqrtMethod, new[] { Expression.Subtract(_one, Expression.Call(_squareMethod, new[] { arguments[0] })) }));
                case "atan": return Expression.Call(_atanMethod, arguments[0]);
                case "datan(0)": return builder.Divide(_one, Expression.Add(_one, Expression.Call(_squareMethod, new[] { arguments[0] })));
                case "dcosh(0)":
                case "sinh": return Expression.Call(_sinhMethod, arguments[0]);
                case "dsinh(0)":
                case "cosh": return Expression.Call(_coshMethod, arguments[0]);
                case "tanh": return Expression.Call(_tanhMethod, arguments[0]);
                case "dtanh(0)": return builder.Divide(_one, Expression.Call(_squareMethod, new[] { Expression.Call(_coshMethod, new[] { arguments[0] }) }));
                case "u": return Expression.Call(_ustepMethod, arguments[0]);
                case "u2": return Expression.Call(_ustep2Method, arguments[0]);
                case "du2(0)": return Expression.Call(_ustep2DerivativeMethod, arguments[0]);
                case "uramp": return Expression.Call(_urampMethod, arguments[0]);
                case "duramp(0)": return Expression.Call(_urampDerivativeMethod, arguments[0]);
                case "ceil": return Expression.Call(_ceilMethod, arguments[0]);
                case "floor": return Expression.Call(_floorMethod, arguments[0]);
                case "nint": return Expression.Call(_roundMethod, arguments[0], Expression.Constant(0));
                case "round": return Expression.Call(_roundMethod, arguments[0], Expression.Convert(arguments[1], typeof(int)));
                case "square": return Expression.Call(_squareMethod, arguments[0]);
                case "dsquare(0)": return Expression.Multiply(Expression.Constant(2.0), arguments[0]);
                case "pwl":
                case "dpwl(0)":
                    int points = (arguments.Count - 1) / 2;
                    var initArguments = new Expression[points];
                    for (var i = 0; i < points; i++)
                        initArguments[i] = Expression.New(_ptConstructor, arguments[i * 2 + 1], arguments[i * 2 + 2]);
                    var arr = Expression.NewArrayInit(typeof(Point), initArguments);
                    if (name == "pwl")
                        return Expression.Call(_pwlMethod, arguments[0], arr);
                    return Expression.Call(_pwlDerivativeMethod, arguments[0], arr);
                case "du(0)":
                case "dceil(0)":
                case "dfloor(0)":
                case "dnint(0)":
                case "dround(0)":
                case "dround(1)":
                    return _zero;
                default:
                    return base.CreateFunction(name, arguments);
            }
        }

        /// <summary>
        /// Creates the value of a number.
        /// </summary>
        /// <param name="input">The number.</param>
        /// <returns>
        /// The value of the number.
        /// </returns>
        public override Expression CreateNumber(string input)
        {
            double value = 0.0;
            int index = 0;
            if ((input[index] < '0' || input[index] > '9') && input[index] != '.')
                throw new Exception("Cannot read the number '{0}'".FormatString(input));

            // Read integer part
            while (index < input.Length && (input[index] >= '0' && input[index] <= '9'))
                value = (value * 10.0) + (input[index++] - '0');

            // Read decimal part
            if (index < input.Length && (input[index] == '.'))
            {
                index++;
                double mult = 1.0;
                while (index < input.Length && (input[index] >= '0' && input[index] <= '9'))
                {
                    value = (value * 10.0) + (input[index++] - '0');
                    mult *= 10.0;
                }

                value /= mult;
            }

            if (index < input.Length)
            {
                // Scientific notation
                if (input[index] == 'e' || input[index] == 'E')
                {
                    index++;
                    var exponent = 0;
                    var neg = false;
                    if (index < input.Length && (input[index] == '+' || input[index] == '-'))
                    {
                        if (input[index] == '-')
                            neg = true;
                        index++;
                    }

                    // Get the exponent
                    while (index < input.Length && (input[index] >= '0' && input[index] <= '9'))
                        exponent = (exponent * 10) + (input[index++] - '0');

                    // Integer exponentation
                    var mult = 1.0;
                    var b = 10.0;
                    while (exponent != 0)
                    {
                        if ((exponent & 0x01) == 0x01)
                            mult *= b;

                        b *= b;
                        exponent >>= 1;
                    }

                    if (neg)
                        value /= mult;
                    else
                        value *= mult;
                }
                else
                {
                    // Spice modifiers
                    switch (input[index])
                    {
                        case 't':
                        case 'T': value *= 1.0e12; index++; break;
                        case 'g':
                        case 'G': value *= 1.0e9; index++; break;
                        case 'x':
                        case 'X': value *= 1.0e6; index++; break;
                        case 'k':
                        case 'K': value *= 1.0e3; index++; break;
                        case 'u':
                        case 'µ':
                        case 'U': value /= 1.0e6; index++; break;
                        case 'n':
                        case 'N': value /= 1.0e9; index++; break;
                        case 'p':
                        case 'P': value /= 1.0e12; index++; break;
                        case 'f':
                        case 'F': value /= 1.0e15; index++; break;
                        case 'm':
                        case 'M':
                            if (index + 2 < input.Length &&
                                (input[index + 1] == 'e' || input[index + 1] == 'E') &&
                                (input[index + 2] == 'g' || input[index + 2] == 'G'))
                            {
                                value *= 1.0e6;
                                index += 3;
                            }
                            else if (index + 2 < input.Length &&
                                (input[index + 1] == 'i' || input[index + 1] == 'I') &&
                                (input[index + 2] == 'l' || input[index + 2] == 'L'))
                            {
                                value *= 25.4e-6;
                                index += 3;
                            }
                            else
                            {
                                value /= 1.0e3;
                                index++;
                            }
                            break;
                    }
                }

                // Any trailing letters are ignored
                while (index < input.Length && ((input[index] >= 'a' && input[index] <= 'z') || (input[index] >= 'A' && input[index] <= 'Z')))
                    index++;
            }

            return Expression.Constant(value);
        }

        /// <summary>
        /// Creates the value of a voltage.
        /// </summary>
        /// <param name="node">The name of the node.</param>
        /// <param name="reference">The name of the reference node.</param>
        /// <param name="type">The quantity type.</param>
        /// <returns>
        /// The value of the voltage.
        /// </returns>
        public override Expression CreateVoltage(string node, string reference, QuantityTypes type)
        {
            if (Simulation != null)
            {
                var nodeVariable = Simulation.Variables.MapNode(node, VariableType.Voltage);
                var refVariable = reference != null ? Simulation.Variables.MapNode(reference, VariableType.Voltage) : null;

                if (type == QuantityTypes.Raw)
                {
                    var state = Simulation.GetState<IBiasingSimulationState>();
                    var nodeIndex = state.Map[nodeVariable];
                    var refIndex = refVariable != null ? state.Map[refVariable] : 0;
                    return refIndex > 0 ?
                        From(() => state.Solution[nodeIndex] - state.Solution[refIndex]) :
                        From(() => state.Solution[nodeIndex]);
                }
                else
                {
                    // Complex voltages
                    var state = Simulation.GetState<IComplexSimulationState>();
                    var nodeIndex = state.Map[nodeVariable];
                    var refIndex = refVariable != null ? state.Map[refVariable] : 0;
                    switch (type)
                    {
                        case QuantityTypes.Real:
                            return refIndex > 0 ?
                                From(() => state.Solution[nodeIndex].Real - state.Solution[refIndex].Real) :
                                From(() => state.Solution[nodeIndex].Real);
                        case QuantityTypes.Imaginary:
                            return refIndex > 0 ?
                                From(() => state.Solution[nodeIndex].Imaginary - state.Solution[refIndex].Imaginary) :
                                From(() => state.Solution[nodeIndex].Imaginary);
                        case QuantityTypes.Magnitude:
                            return refIndex > 0 ?
                                From(() => (state.Solution[nodeIndex] - state.Solution[refIndex]).Magnitude) :
                                From(() => state.Solution[nodeIndex].Magnitude);
                        case QuantityTypes.Phase:
                            return refIndex > 0 ?
                                From(() => (state.Solution[nodeIndex] - state.Solution[refIndex]).Phase) :
                                From(() => state.Solution[nodeIndex].Phase);
                        case QuantityTypes.Decibels:
                            return refIndex > 0 ?
                                From(() =>
                                {
                                    var c = state.Solution[nodeIndex] - state.Solution[refIndex];
                                    return 10.0 * Math.Log10(c.Real * c.Real + c.Imaginary * c.Imaginary);
                                }) :
                                From(() =>
                                {
                                    var c = state.Solution[nodeIndex];
                                    return 10.0 * Math.Log10(c.Real * c.Real + c.Imaginary * c.Imaginary);
                                });
                        default:
                            throw new Exception("Unknown quantity type");
                    }
                }
            }
            else
                return base.CreateVoltage(node, reference, type);
        }

        /// <summary>
        /// Creates the value of a current.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="type">The quantity type.</param>
        /// <returns>
        /// The value of the current.
        /// </returns>
        public override Expression CreateCurrent(string name, QuantityTypes type)
        {
            if (Simulation != null)
            {
                var eb = Simulation.EntityBehaviors[name];
                if (type == QuantityTypes.Raw)
                {
                    var state = Simulation.GetState<IBiasingSimulationState>();
                    if (eb.TryGetValue(out IBranchedBehavior behavior))
                    {
                        var index = state.Map[behavior.Branch];
                        return From(() => state.Solution[index]);
                    }
                    else
                    {
                        var getter = eb.CreatePropertyGetter<double>("i");
                        return base.CreateCurrent(name, type);
                    }
                }
                else
                {
                    var state = Simulation.GetState<IComplexSimulationState>();
                    if (eb.TryGetValue(out IBranchedBehavior behavior))
                    {
                        var index = state.Map[behavior.Branch];
                        switch (type)
                        {
                            case QuantityTypes.Real:
                                return From(() => state.Solution[index].Real);
                            case QuantityTypes.Imaginary:
                                return From(() => state.Solution[index].Imaginary);
                            case QuantityTypes.Magnitude:
                                return From(() => state.Solution[index].Magnitude);
                            case QuantityTypes.Phase:
                                return From(() => state.Solution[index].Phase);
                            case QuantityTypes.Decibels:
                                return From(() =>
                                {
                                    var c = state.Solution[index];
                                    return 10.0 * Math.Log10(c.Real * c.Real + c.Imaginary * c.Imaginary);
                                });
                        }
                    }
                    else
                    {
                        var getter = eb.CreatePropertyGetter<Complex>("i");
                        if (getter != null)
                        {
                            switch (type)
                            {
                                case QuantityTypes.Real:
                                    return From(() => getter().Real);
                                case QuantityTypes.Imaginary:
                                    return From(() => getter().Imaginary);
                                case QuantityTypes.Magnitude:
                                    return From(() => getter().Magnitude);
                                case QuantityTypes.Phase:
                                    return From(() => getter().Phase);
                                case QuantityTypes.Decibels:
                                    return From(() =>
                                    {
                                        var c = getter();
                                        return 10.0 * Math.Log10(c.Real * c.Real + c.Imaginary * c.Imaginary);
                                    });
                            }
                        }
                    }
                }
            }
            
            return base.CreateCurrent(name, type);
        }
    }
}
