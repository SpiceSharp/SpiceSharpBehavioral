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
    public class ExpressionBuilder : IBuilder<Expression>
    {
        private struct Point
        {
            public double X, Y;
            public Point(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        private readonly static Expression _one = Expression.Constant(1.0);
        private readonly static Expression _zero = Expression.Constant(0.0);
        private readonly static MethodInfo _divMethod = typeof(ExpressionBuilder).GetTypeInfo().GetMethod("SafeDivide", BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly static MethodInfo _absMethod = new Func<double, double>(Math.Abs).GetMethodInfo();
        private readonly static MethodInfo _sqrtMethod = new Func<double, double>(Math.Sqrt).GetMethodInfo();
        private readonly static MethodInfo _powMethod = new Func<double, double, double>(SafePower).GetMethodInfo();
        private readonly static MethodInfo _pwrMethod = new Func<double, double, double>(SafePower2).GetMethodInfo();
        private readonly static MethodInfo _logMethod = new Func<double, double>(Math.Log).GetMethodInfo();
        private readonly static MethodInfo _log10Method = new Func<double, double>(Math.Log10).GetMethodInfo();
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
        private readonly static MethodInfo _ustepMethod = new Func<double, double>(Step).GetMethodInfo();
        private readonly static MethodInfo _ustep2Method = new Func<double, double>(Step2).GetMethodInfo();
        private readonly static MethodInfo _urampMethod = new Func<double, double>(Ramp).GetMethodInfo();
        private readonly static MethodInfo _ceilMethod = new Func<double, double>(Math.Ceiling).GetMethodInfo();
        private readonly static MethodInfo _floorMethod = new Func<double, double>(Math.Floor).GetMethodInfo();
        private readonly static MethodInfo _roundMethod = new Func<double, int, double>(Math.Round).GetMethodInfo();
        private readonly static MethodInfo _pwlMethod = new Func<double, Point[], double>(Pwl).GetMethodInfo();
        private readonly static MethodInfo _pwlDerivativeMethod = new Func<double, Point[], double>(PwlDerivative).GetMethodInfo();
        private readonly static MethodInfo _squareMethod = new Func<double, double>(Square).GetMethodInfo();
        private readonly static ConstructorInfo _ptConstructor = typeof(Point).GetTypeInfo().GetConstructor(new[] { typeof(double), typeof(double) });

        private readonly ISimulation _simulation;

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

        private static double SafePower(double left, double right) => Math.Pow(Math.Abs(left), right);
        private static double SafePower2(double left, double right)
        {
            if (left < 0)
                return -Math.Pow(-left, right);
            else
                return Math.Pow(left, right);
        }
        private static double Step(double arg)
        {
            if (arg < 0.0)
                return 0.0;
            else if (arg > 0.0)
                return 1.0;
            else
                return 0.5; /* Ick! */
        }
        private static double Step2(double arg)
        {
            if (arg <= 0.0)
                return 0.0;
            else if (arg <= 1.0)
                return arg;
            else /* if (arg > 1.0) */
                return 1.0;
        }
        private static double Ramp(double arg)
        {
            if (arg < 0)
                return 0.0;
            return arg;
        }
        private static double Pwl(double arg, Point[] data)
        {
            // Narrow in on the index for the piece-wise linear function
            int k0 = 0, k1 = data.Length;
            while (k1 - k0 > 1)
            {
                int k = (k0 + k1) / 2;
                if (data[k].X > arg)
                    k1 = k;
                else
                    k0 = k;
            }
            return data[k0].Y + (arg - data[k0].X) * (data[k1].Y - data[k0].Y) / (data[k1].X - data[k0].X);
        }
        private static double PwlDerivative(double arg, Point[] data)
        {
            // Narrow in on the index for the piece-wise linear function
            int k0 = 0, k1 = data.Length;
            while (k1 - k0 > 1)
            {
                int k = (k0 + k1) / 2;
                if (data[k].X > arg)
                    k1 = k;
                else
                    k0 = k;
            }
            return (data[k1].Y - data[k0].Y) / (data[k1].X - data[k0].X);
        }
        private static double Square(double arg) => arg * arg;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBuilder"/> class.
        /// </summary>
        public ExpressionBuilder()
        {
            _simulation = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBuilder"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public ExpressionBuilder(ISimulation simulation)
        {
            _simulation = simulation;
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
                        return Expression.Constant(SafePower(lv, rv));
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
        Expression IBuilder<Expression>.CreateFunction(string name, IReadOnlyList<Expression> arguments)
        {
            // Check the number of arguments
            switch (name)
            {
                case "abs":
                case "sqrt":
                case "log":
                case "log10":
                case "exp":
                case "sin":
                case "cos":
                case "tan":
                case "asin":
                case "acos":
                case "atan":
                case "sinh":
                case "cosh":
                case "tanh":
                case "u":
                case "u2":
                case "uramp":
                case "ceil":
                case "floor":
                case "nint":
                case "square":
                    if (arguments.Count != 1)
                        throw new Exception("Invalid number of arguments for {0}()".FormatString(name));
                    break;
                case "pwr":
                case "min":
                case "max":
                case "round":
                    if (arguments.Count != 2)
                        throw new Exception("Invalid number of arguments for {0}()".FormatString(name));
                    break;
                case "pwl":
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
                    switch (name)
                    {
                        case "abs": return Expression.Constant(Math.Abs((double)ces[0].Value));
                        case "sqrt": return Expression.Constant(Math.Sqrt((double)ces[0].Value));
                        case "pwr": return Expression.Constant(SafePower2((double)ces[0].Value, (double)ces[1].Value));
                        case "min": return Expression.Constant(Math.Min((double)ces[0].Value, (double)ces[1].Value));
                        case "max": return Expression.Constant(Math.Max((double)ces[0].Value, (double)ces[1].Value));
                        case "log": return Expression.Constant(Math.Log((double)ces[0].Value));
                        case "log10": return Expression.Constant(Math.Log10((double)ces[0].Value));
                        case "exp": return Expression.Constant(Math.Exp((double)ces[0].Value));
                        case "sin": return Expression.Constant(Math.Sin((double)ces[0].Value));
                        case "cos": return Expression.Constant(Math.Cos((double)ces[0].Value));
                        case "tan": return Expression.Constant(Math.Tan((double)ces[0].Value));
                        case "asin": return Expression.Constant(Math.Asin((double)ces[0].Value));
                        case "acos": return Expression.Constant(Math.Acos((double)ces[0].Value));
                        case "atan": return Expression.Constant(Math.Atan((double)ces[0].Value));
                        case "sinh": return Expression.Constant(Math.Sinh((double)ces[0].Value));
                        case "cosh": return Expression.Constant(Math.Cosh((double)ces[0].Value));
                        case "tanh": return Expression.Constant(Math.Tanh((double)ces[0].Value));
                        case "u": return Expression.Constant(Step((double)ces[0].Value));
                        case "u2": return Expression.Constant(Step2((double)ces[0].Value));
                        case "uramp": return Expression.Constant(Ramp((double)ces[0].Value));
                        case "ceil": return Expression.Constant(Math.Ceiling((double)ces[0].Value));
                        case "floor": return Expression.Constant(Math.Floor((double)ces[0].Value));
                        case "nint": return Expression.Constant(Math.Round((double)ces[0].Value, 0));
                        case "round": return Expression.Constant(Math.Round((double)ces[0].Value, (int)(double)ces[1].Value));
                        case "square": return Expression.Constant(Square((double)ces[0].Value));
                        case "pwl":
                        case "pwl_derivative":
                            int points = (arguments.Count - 1) / 2;
                            var initArguments = new Point[points];
                            for (var i = 0; i < points; i++)
                                initArguments[i] = new Point((double)ces[2 * i + 1].Value, (double)ces[2 * i + 2].Value);
                            if (name == "pwl")
                                return Expression.Constant(Pwl((double)ces[0].Value, initArguments));
                            else
                                return Expression.Constant(PwlDerivative((double)ces[0].Value, initArguments));
                    }
                }
            }

            switch (name)
            {
                case "abs": return Expression.Call(_absMethod, arguments[0]);
                case "sqrt": return Expression.Call(_sqrtMethod, arguments[0]);
                case "pwr": return Expression.Call(_pwrMethod, arguments[0], arguments[1]);
                case "min": return Expression.Call(_minMethod, arguments[0], arguments[1]);
                case "max": return Expression.Call(_maxMethod, arguments[0], arguments[1]);
                case "log": return Expression.Call(_logMethod, arguments[0]);
                case "log10": return Expression.Call(_log10Method, arguments[0]);
                case "exp": return Expression.Call(_expMethod, arguments[0]);
                case "sin": return Expression.Call(_sinMethod, arguments[0]);
                case "cos": return Expression.Call(_cosMethod, arguments[0]);
                case "tan": return Expression.Call(_tanMethod, arguments[0]);
                case "asin": return Expression.Call(_asinMethod, arguments[0]);
                case "acos": return Expression.Call(_acosMethod, arguments[0]);
                case "atan": return Expression.Call(_atanMethod, arguments[0]);
                case "sinh": return Expression.Call(_sinhMethod, arguments[0]);
                case "cosh": return Expression.Call(_coshMethod, arguments[0]);
                case "tanh": return Expression.Call(_tanhMethod, arguments[0]);
                case "u": return Expression.Call(_ustepMethod, arguments[0]);
                case "u2": return Expression.Call(_ustep2Method, arguments[0]);
                case "uramp": return Expression.Call(_urampMethod, arguments[0]);
                case "ceil": return Expression.Call(_ceilMethod, arguments[0]);
                case "floor": return Expression.Call(_floorMethod, arguments[0]);
                case "nint": return Expression.Call(_roundMethod, arguments[0], Expression.Constant(0));
                case "round": return Expression.Call(_roundMethod, arguments[0], Expression.Convert(arguments[1], typeof(int)));
                case "square": return Expression.Call(_squareMethod, arguments[0]);
                case "pwl":
                case "pwl_derivative":
                    int points = (arguments.Count - 1) / 2;
                    var initArguments = new Expression[points];
                    for (var i = 0; i < points; i++)
                        initArguments[i] = Expression.New(_ptConstructor, arguments[i * 2 + 1], arguments[i * 2 + 2]);
                    var arr = Expression.NewArrayInit(typeof(Point), initArguments);
                    if (name == "pwl")
                        return Expression.Call(_pwlMethod, arguments[0], arr);
                    return Expression.Call(_pwlDerivativeMethod, arguments[0], arr);
                default:
                    throw new Exception("Unrecognized function {0}".FormatString(name));
            }
        }
        Expression IBuilder<Expression>.CreateNumber(string input)
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
        Expression IBuilder<Expression>.CreateVariable(string name)
        {
            throw new NotImplementedException();
        }
        Expression IBuilder<Expression>.CreateVoltage(string node, string reference, QuantityTypes type)
        {
            if (_simulation == null)
                throw new Exception("No simulation specified");
            node.ThrowIfNull(nameof(node));
            Func<double> result;
            switch (type)
            {
                case QuantityTypes.Raw:
                    var state = _simulation.GetState<IBiasingSimulationState>();
                    var variable = _simulation.Variables.MapNode(node, VariableType.Voltage);
                    var index = state.Map[variable];
                    if (reference == null)
                        result = new Func<double>(() => state.Solution[index]);
                    else
                    {
                        variable = _simulation.Variables.MapNode(reference, VariableType.Voltage);
                        var refIndex = state.Map[variable];
                        result = new Func<double>(() => state.Solution[index] - state.Solution[refIndex]);
                    }
                    break;
                case QuantityTypes.Real:
                    var cstate = _simulation.GetState<IComplexSimulationState>();
                    variable = _simulation.Variables.MapNode(node, VariableType.Voltage);
                    index = cstate.Map[variable];
                    if (reference == null)
                        result = new Func<double>(() => cstate.Solution[index].Real);
                    else
                    {
                        variable = _simulation.Variables.MapNode(reference, VariableType.Voltage);
                        var refIndex = cstate.Map[variable];
                        result = new Func<double>(() => cstate.Solution[index].Real - cstate.Solution[refIndex].Real);
                    }
                    break;
                case QuantityTypes.Imaginary:
                    cstate = _simulation.GetState<IComplexSimulationState>();
                    variable = _simulation.Variables.MapNode(node, VariableType.Voltage);
                    index = cstate.Map[variable];
                    if (reference == null)
                        result = new Func<double>(() => cstate.Solution[index].Imaginary);
                    else
                    {
                        variable = _simulation.Variables.MapNode(reference, VariableType.Voltage);
                        var refIndex = cstate.Map[variable];
                        result = new Func<double>(() => cstate.Solution[index].Imaginary - cstate.Solution[refIndex].Imaginary);
                    }
                    break;
                case QuantityTypes.Magnitude:
                    cstate = _simulation.GetState<IComplexSimulationState>();
                    variable = _simulation.Variables.MapNode(node, VariableType.Voltage);
                    index = cstate.Map[variable];
                    if (reference == null)
                        result = new Func<double>(() => cstate.Solution[index].Magnitude);
                    else
                    {
                        variable = _simulation.Variables.MapNode(reference, VariableType.Voltage);
                        var refIndex = cstate.Map[variable];
                        result = new Func<double>(() => (cstate.Solution[index] - cstate.Solution[refIndex]).Magnitude);
                    }
                    break;
                case QuantityTypes.Phase:
                    cstate = _simulation.GetState<IComplexSimulationState>();
                    variable = _simulation.Variables.MapNode(node, VariableType.Voltage);
                    index = cstate.Map[variable];
                    if (reference == null)
                        result = new Func<double>(() => cstate.Solution[index].Phase);
                    else
                    {
                        variable = _simulation.Variables.MapNode(reference, VariableType.Voltage);
                        var refIndex = cstate.Map[variable];
                        result = new Func<double>(() => (cstate.Solution[index] - cstate.Solution[refIndex]).Phase);
                    }
                    break;
                case QuantityTypes.Decibels:
                    cstate = _simulation.GetState<IComplexSimulationState>();
                    variable = _simulation.Variables.MapNode(node, VariableType.Voltage);
                    index = cstate.Map[variable];
                    if (reference == null)
                        result = new Func<double>(() =>
                        {
                            var c = cstate.Solution[index];
                            return 10.0 * Math.Log10(c.Real * c.Real + c.Imaginary * c.Imaginary);
                        });
                    else
                    {
                        variable = _simulation.Variables.MapNode(reference, VariableType.Voltage);
                        var refIndex = cstate.Map[variable];
                        result = new Func<double>(() =>
                        {
                            var c = cstate.Solution[index] - cstate.Solution[refIndex];
                            return 10.0 * Math.Log10(c.Real * c.Real + c.Imaginary * c.Imaginary);
                        });
                    }
                    break;
                default:
                    throw new Exception("Unrecognized voltage quantity type");
            }
            return Expression.Call(Expression.Constant(result.Target), result.GetMethodInfo());
        }
        Expression IBuilder<Expression>.CreateCurrent(string name, QuantityTypes type)
        {
            if (_simulation == null)
                throw new Exception("No simulation specified");
            name.ThrowIfNull(nameof(name));
            var behaviors = _simulation.EntityBehaviors[name];
            Func<double> result;
            switch (type)
            {
                case QuantityTypes.Raw:
                    if (behaviors.TryGetValue(out IBranchedBehavior behavior))
                    {
                        var state = _simulation.GetState<IBiasingSimulationState>();
                        var index = state.Map[behavior.Branch];
                        result = () => state.Solution[index];
                    }
                    else
                        result = behaviors.CreatePropertyGetter<double>("i");
                    break;
                case QuantityTypes.Real:
                    if (behaviors.TryGetValue(out behavior))
                    {
                        var cstate = _simulation.GetState<IComplexSimulationState>();
                        var index = cstate.Map[behavior.Branch];
                        result = () => cstate.Solution[index].Real;
                    }
                    else
                    {
                        var getter = behaviors.CreatePropertyGetter<Complex>("i");
                        result = () => getter().Real;
                    }
                    break;
                case QuantityTypes.Imaginary:
                    if (behaviors.TryGetValue(out behavior))
                    {
                        var cstate = _simulation.GetState<IComplexSimulationState>();
                        var index = cstate.Map[behavior.Branch];
                        result = () => cstate.Solution[index].Imaginary;
                    }
                    else
                    {
                        var getter = behaviors.CreatePropertyGetter<Complex>("i");
                        result = () => getter().Imaginary;
                    }
                    break;
                case QuantityTypes.Magnitude:
                    if (behaviors.TryGetValue(out behavior))
                    {
                        var cstate = _simulation.GetState<IComplexSimulationState>();
                        var index = cstate.Map[behavior.Branch];
                        result = () => cstate.Solution[index].Magnitude;
                    }
                    else
                    {
                        var getter = behaviors.CreatePropertyGetter<Complex>("i");
                        result = () => getter().Magnitude;
                    }
                    break;
                case QuantityTypes.Phase:
                    if (behaviors.TryGetValue(out behavior))
                    {
                        var cstate = _simulation.GetState<IComplexSimulationState>();
                        var index = cstate.Map[behavior.Branch];
                        result = () => cstate.Solution[index].Phase;
                    }
                    else
                    {
                        var getter = behaviors.CreatePropertyGetter<Complex>("i");
                        result = () => getter().Phase;
                    }
                    break;
                case QuantityTypes.Decibels:
                    if (behaviors.TryGetValue(out behavior))
                    {
                        var cstate = _simulation.GetState<IComplexSimulationState>();
                        var index = cstate.Map[behavior.Branch];
                        result = () =>
                        {
                            var c = cstate.Solution[index];
                            return 10.0 * Math.Log10(c.Real * c.Real + c.Imaginary * c.Imaginary);
                        };
                    }
                    else
                    {
                        var getter = behaviors.CreatePropertyGetter<Complex>("i");
                        result = () =>
                        {
                            var c = getter();
                            return 10.0 * Math.Log10(c.Real * c.Real + c.Imaginary * c.Imaginary);
                        };
                    }
                    break;
                default:
                    throw new Exception("Unrecognized current quantity type");
            }
            return Expression.Call(Expression.Constant(result.Target), result.GetMethodInfo());
        }
        Expression IBuilder<Expression>.CreateProperty(string name, string property, QuantityTypes type)
        {
            if (_simulation == null)
                throw new Exception("No simulation specified");
            name.ThrowIfNull(nameof(name));
            property.ThrowIfNull(nameof(property));

            var behaviors = _simulation.EntityBehaviors[name];
            if (type == QuantityTypes.Raw)
            {
                var getter = behaviors.CreatePropertyGetter<double>(property).ThrowIfNull("property");
                return Expression.Call(Expression.Constant(getter.Target), getter.GetMethodInfo());
            }
            else
            {
                var getter = behaviors.CreatePropertyGetter<Complex>(property).ThrowIfNull("property");
                Func<double> result = null;
                switch (type)
                {
                    case QuantityTypes.Real:
                        result = () => getter().Real;
                        break;
                    case QuantityTypes.Imaginary:
                        result = () => getter().Imaginary;
                        break;
                    case QuantityTypes.Magnitude:
                        result = () => getter().Magnitude;
                        break;
                    case QuantityTypes.Phase:
                        result = () => getter().Phase;
                        break;
                    case QuantityTypes.Decibels:
                        result = () =>
                        {
                            var c = getter();
                            return 10.0 * Math.Log10(c.Real * c.Real + c.Imaginary * c.Imaginary);
                        };
                        break;
                    default:
                        throw new Exception("Unrecognized quantity");
                }
                return Expression.Call(Expression.Constant(result.Target), result.GetMethodInfo());
            }
        }
    }
}
