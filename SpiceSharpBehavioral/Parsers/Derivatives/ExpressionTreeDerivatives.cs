using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A specific implementation of derivatives for expression trees.
    /// </summary>
    public class ExpressionTreeDerivatives : Derivatives<Expression>
    {
        public static readonly Expression Zero = Expression.Constant(0.0);
        public static readonly Expression One = Expression.Constant(1.0);
        public static readonly MethodInfo LogInfo = typeof(Math).GetTypeInfo().GetMethod("Log", new[] { typeof(double) });
        public static readonly MethodInfo PowInfo = typeof(Math).GetTypeInfo().GetMethod("Pow", new[] { typeof(double), typeof(double) });
        public static readonly MethodInfo SquareInfo = typeof(ExpressionTreeDerivatives).GetTypeInfo().GetMethod("Square", new[] { typeof(double) });

        public ExpressionTreeDerivatives()
        { }

        public ExpressionTreeDerivatives(int capacity)
            : base(capacity)
        { }

        /// <summary>
        /// Compile an expression tree.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public T Compile<T>(int index) => Expression.Lambda<T>(this[index] ?? Zero).Compile();

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Squares an input.
        /// </summary>
        /// <param name="x">The input.</param>
        /// <returns></returns>
        public static double Square(double x) => x * x;

        public ExpressionTreeDerivatives Or(ExpressionTreeDerivatives b)
        {
            var result = new ExpressionTreeDerivatives();
            result[0] = Expression.Condition(
                Expression.And(
                    Expression.Equal(this[0], Zero),
                    Expression.Equal(b[0], Zero)), Zero, One);
            return result;
        }

        public ExpressionTreeDerivatives And(ExpressionTreeDerivatives b)
        {
            var result = new ExpressionTreeDerivatives();
            result[0] = Expression.Condition(
                Expression.Or(
                    Expression.Equal(this[0], Zero),
                    Expression.Equal(b[0], Zero)), Zero, One);
            return result;
        }

        public ExpressionTreeDerivatives IfThen(ExpressionTreeDerivatives iftrue, ExpressionTreeDerivatives iffalse)
        {
            if (this[0] == null)
                return iffalse;
            var size = Math.Max(iftrue.Count, iffalse.Count);
            var result = new ExpressionTreeDerivatives(size);
            var notcondition = Expression.Equal(this[0], Zero);
            for (var i = 0; i < size; i++)
            {
                var iftruei = iftrue[i];
                var iffalsei = iffalse[i];
                if (iftruei == null && iffalsei == null)
                    continue;
                result[i] = Expression.Condition(notcondition, iffalsei ?? Zero, iftruei ?? Zero);
            }
            return result;
        }

        public ExpressionTreeDerivatives Pow(ExpressionTreeDerivatives exponent)
        {
            var size = Math.Max(Count, exponent.Count);
            var result = new ExpressionTreeDerivatives(size);
            Expression a0 = this[0], b0 = exponent[0];
            if (a0 == null && b0 == null)
            {
                // This doesn't make much sense (0^0 is invalid) but Math.Pow does it like that so we will too
                result[0] = Expression.Constant(1.0);
                return result;
            }
            if (a0 == null)
                a0 = Expression.Constant(0.0);
            if (b0 == null)
            {
                // (fx)^0 = 1
                result[0] = One;
                return result;
            }
            result[0] = Expression.Call(PowInfo, a0, b0);
            for (var i = 1; i < size; i++)
            {
                // (fx^b)' = b * fx^(b-1) * f'x
                if (this[i] != null)
                {
                    result[i] = Expression.Multiply(b0, Expression.Multiply(this[i],
                            Expression.Call(PowInfo, a0, Expression.Subtract(b0, One))));
                }

                // (fx^gx)' = (e^(gx*ln(fx)))'
                // = fx^(gx-1)*f'x + fx^gx*ln(fx)*g'x
                if (exponent[i] != null)
                {
                    var contribution = Expression.Multiply(result[0], Expression.Multiply(
                        Expression.Call(LogInfo, a0), exponent[i]));
                    result[i] = result[i] == null ? contribution :
                        Expression.Add(result[i], contribution);
                }
            }
            return result;
        }

        public ExpressionTreeDerivatives Equal(ExpressionTreeDerivatives other)
        {
            var result = new ExpressionTreeDerivatives();
            result[0] = Expression.Condition(Expression.Equal(this[0], other[0]), One, Zero);
            return result;
        }

        public ExpressionTreeDerivatives NotEqual(ExpressionTreeDerivatives other)
        {
            var result = new ExpressionTreeDerivatives();
            result[0] = Expression.Condition(Expression.NotEqual(this[0], other[0]), One, Zero);
            return result;
        }

        public static ExpressionTreeDerivatives operator -(ExpressionTreeDerivatives a)
        {
            var result = new ExpressionTreeDerivatives(a.Count);
            for (var i = 0; i < a.Count; i++)
            {
                if (a[i] != null)
                    result[i] = Expression.Negate(a[i]);
            }
            return result;
        }

        public static ExpressionTreeDerivatives operator !(ExpressionTreeDerivatives a)
        {
            var result = new ExpressionTreeDerivatives(a.Count);
            if (a[0] != null)
                result[0] = Expression.Condition(Expression.Equal(a[0], Zero), One, Zero);
            else
                result[0] = One;
            return result;
        }

        public static ExpressionTreeDerivatives operator +(ExpressionTreeDerivatives a, ExpressionTreeDerivatives b)
        {
            var size = Math.Max(a.Count, b.Count);
            var result = new ExpressionTreeDerivatives(size);
            for (var i = 0; i < size; i++)
            {
                if (a[i] == null)
                    result[i] = b[i];
                else
                {
                    if (b[i] == null)
                        result[i] = a[i];
                    else
                        result[i] = Expression.Add(a[i], b[i]);
                }
            }
            return result;
        }

        public static ExpressionTreeDerivatives operator -(ExpressionTreeDerivatives a, ExpressionTreeDerivatives b)
        {
            var size = Math.Max(a.Count, b.Count);
            var result = new ExpressionTreeDerivatives(size);
            for (var i = 0; i < size; i++)
            {
                if (a[i] == null)
                {
                    if (b[i] == null)
                        result[i] = null;
                    else
                        result[i] = Expression.Negate(b[i]);
                }
                else
                {
                    if (b[i] == null)
                        result[i] = a[i];
                    else
                        result[i] = Expression.Subtract(a[i], b[i]);
                }
            }
            return result;
        }

        public static ExpressionTreeDerivatives operator *(ExpressionTreeDerivatives a, ExpressionTreeDerivatives b)
        {
            var size = Math.Max(a.Count, b.Count);
            var result = new ExpressionTreeDerivatives(size);
            Expression a0 = a[0], b0 = b[0];

            if (a0 == null || b0 == null)
                return result; // x*0 = 0 and so is all its derivatives
            else
                result[0] = Expression.Multiply(a0, b0);

            for (var i = 1; i < size; i++)
            {
                if (a[i] == null)
                {
                    if (b[i] == null)
                        result[i] = null;
                    else
                        // (a*gx)' = a*g'x
                        result[i] = Expression.Multiply(a0, b[i]);
                }
                else
                {
                    if (b[i] == null)
                        // (fx*b)' = f'x*b
                        result[i] = Expression.Multiply(a[i], b0);
                    else
                        // (fx*gx)' = fx*g'x + f'x*gx
                        result[i] = Expression.Add(
                            Expression.Multiply(a0, b[i]),
                            Expression.Multiply(a[i], b0));
                }
            }
            return result;
        }

        public static ExpressionTreeDerivatives operator /(ExpressionTreeDerivatives a, ExpressionTreeDerivatives b)
        {
            var size = Math.Max(a.Count, b.Count);
            var result = new ExpressionTreeDerivatives(size);

            Expression a0 = a[0], b0 = b[0];
            if (a0 == null)
                return null;
            if (b0 == null)
                throw new DivideByZeroException();
            result[0] = Expression.Divide(a0, b0);
            for (var i = 1; i < size; i++)
            {
                if (a[i] == null)
                {
                    if (b[i] == null)
                        result[i] = null;
                    else
                        // (a/gx)' = -a/gx^2*g'x
                        result[i] = Expression.Divide(
                            Expression.Negate(Expression.Multiply(a0, b[i])),
                            Expression.Call(SquareInfo, b0));
                }
                else
                {
                    if (b[i] == null)
                        // (fx/b)' = f'x/b
                        result[i] = Expression.Divide(a[i], b0);
                    else
                        // (fx/gx)' = (f'x*gx-fx*g'x)/gx^2
                        result[i] = Expression.Divide(
                            Expression.Subtract(
                                Expression.Multiply(a[i], b0),
                                Expression.Multiply(a0, b[i])),
                            Expression.Call(SquareInfo, b0));
                }
            }
            return result;
        }

        public static ExpressionTreeDerivatives operator %(ExpressionTreeDerivatives a, ExpressionTreeDerivatives b)
        {
            var result = new ExpressionTreeDerivatives();
            if (a[0] == null)
                return result;
            if (b[0] == null)
                throw new Exception("Modulo by 0");
            result[0] = Expression.Modulo(a[0], b[0]);
            return result;
        }

        public static ExpressionTreeDerivatives operator >(ExpressionTreeDerivatives a, ExpressionTreeDerivatives b)
        {
            var result = new ExpressionTreeDerivatives();
            Expression a0 = a[0], b0 = b[0];
            if (a0 == null && b0 == null)
                return result;
            a0 = a0 ?? Zero;
            b0 = b0 ?? Zero;
            result[0] = Expression.Condition(Expression.GreaterThan(a0, b0), One, Zero);
            return result;
        }

        public static ExpressionTreeDerivatives operator <(ExpressionTreeDerivatives a, ExpressionTreeDerivatives b)
        {
            var result = new ExpressionTreeDerivatives();
            Expression a0 = a[0], b0 = b[0];
            if (a0 == null && b0 == null)
                return result;
            a0 = a0 ?? Zero;
            b0 = b0 ?? Zero;
            result[0] = Expression.Condition(Expression.LessThan(a0, b0), One, Zero); return result;
        }

        public static ExpressionTreeDerivatives operator >=(ExpressionTreeDerivatives a, ExpressionTreeDerivatives b)
        {
            var result = new ExpressionTreeDerivatives();
            Expression a0 = a[0], b0 = b[0];
            if (a0 == null && b0 == null)
                return result;
            a0 = a0 ?? Zero;
            b0 = b0 ?? Zero;
            result[0] = Expression.Condition(Expression.GreaterThanOrEqual(a0, b0), One, Zero); return result;
        }

        public static ExpressionTreeDerivatives operator <=(ExpressionTreeDerivatives a, ExpressionTreeDerivatives b)
        {
            var result = new ExpressionTreeDerivatives();
            Expression a0 = a[0], b0 = b[0];
            if (a0 == null && b0 == null)
                return result;
            a0 = a0 ?? Zero;
            b0 = b0 ?? Zero;
            result[0] = Expression.Condition(Expression.LessThanOrEqual(a0, b0), One, Zero);
            return result;
        }
    }
}
