using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using System;
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
    public class ExpressionBuilder : BaseBuilder<Expression>, IEventBuilder<Expression>
    {
        private readonly static Expression _one = Expression.Constant(1.0);
        private readonly static Expression _zero = Expression.Constant(0.0);
        private readonly static MethodInfo _divMethod = new Func<double, double, double, double>(Functions.SafeDivide).GetMethodInfo();
        private readonly static MethodInfo _powMethod = new Func<double, double, double>(Functions.Power).GetMethodInfo();

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
                        return Expression.Constant(Functions.SafeDivide((double)ceLeft.Value, rv, FudgeFactor));
                    else if (rv.Equals(0.0))
                        return Expression.Constant(double.PositiveInfinity);
                    else
                        return Expression.Divide(left, ceRight);
                }
            }
            return Expression.Call(_divMethod, left, right, Expression.Constant(FudgeFactor));
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
