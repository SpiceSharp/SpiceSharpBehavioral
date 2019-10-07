using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A specific implementation of derivatives for doubles.
    /// </summary>
    public class DoubleDerivatives : Derivatives<Func<double>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleDerivatives"/> class.
        /// </summary>
        public DoubleDerivatives()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleDerivatives"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public DoubleDerivatives(int capacity)
            : base(capacity)
        { }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Get a hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Raises the derivatives to a power.
        /// </summary>
        /// <param name="exponent">The exponent.</param>
        /// <returns>The raised power.</returns>
        public override Derivatives<Func<double>> Pow(Derivatives<Func<double>> exponent)
        {
            var size = Math.Max(Count, exponent.Count);
            var result = new DoubleDerivatives(size);
            var a0 = this[0];
            var b0 = exponent[0];
            result[0] = () => SafePow(a0(), b0());
            for (var i = 1; i < size; i++)
            {
                if (this[i] != null && exponent[i] != null)
                {
                    // (fx^gx)' = (e^(gx*ln(fx)))'
                    // = gx*fx^(gx-1)*f'x + fx^gx*ln(fx)*g'x
                    var ai = this[i];
                    var bi = exponent[i];
                    result[i] = () =>
                    {
                        var tmpa0 = Math.Abs(a0());
                        var tmpb0 = b0();
                        return SafePow(tmpa0, tmpb0 - 1) * tmpb0 * ai() + SafePow(tmpa0, tmpb0) * Math.Log(tmpa0) * bi();
                    };
                }
                else if (this[i] != null)
                {
                    // (fx^b)' = b * fx^(b-1) * f'x
                    var ai = this[i];
                    result[i] = () =>
                    {
                        var tmpa0 = Math.Abs(a0());
                        var tmpb0 = b0();
                        return SafePow(tmpa0, tmpb0 - 1) * tmpb0 * ai();
                    };
                }
                else if (exponent[i] != null)
                {
                    // (a^gx)' = a^gx * ln(a) * g'x
                    var bi = exponent[i];
                    result[i] = () =>
                    {
                        var tmpa0 = Math.Abs(a0());
                        var tmpb0 = b0();
                        return SafePow(tmpa0, tmpb0) * Math.Log(tmpa0) * bi();
                    };
                }
            }
            return result;
        }

        /// <summary>
        /// Safe version of computing powers.
        /// </summary>
        /// <param name="a">The base.</param>
        /// <param name="b">The exponent.</param>
        /// <returns></returns>
        public static double SafePow(double a, double b)
        {
            if (a.Equals(0.0) && b < 0)
                a += FudgeFactor;
            if (a < 0)
                return -Math.Pow(-a, b);
            return Math.Pow(a, b);
        }

        /// <summary>
        /// Or the derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// The derivatives.
        /// </returns>
        public override Derivatives<Func<double>> Or(Derivatives<Func<double>> b)
        {
            var result = new DoubleDerivatives();
            var arg1 = this[0];
            var arg2 = b[0];
            result[0] = () => arg1().Equals(0.0) && arg2().Equals(0.0) ? 0 : 1;
            return result;
        }

        /// <summary>
        /// And the derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// The derivatives.
        /// </returns>
        public override Derivatives<Func<double>> And(Derivatives<Func<double>> b)
        {
            var result = new DoubleDerivatives();
            var arg1 = this[0];
            var arg2 = b[0];
            result[0] = () => arg1().Equals(0.0) || arg2().Equals(0.0) ? 0 : 1;
            return result;
        }

        /// <summary>
        /// Conditional derivatives.
        /// </summary>
        /// <param name="iftrue">Argument if true.</param>
        /// <param name="iffalse">Argument if false.</param>
        /// <returns>
        /// The derivatives.
        /// </returns>
        public override Derivatives<Func<double>> IfThenElse(Derivatives<Func<double>> iftrue, Derivatives<Func<double>> iffalse)
        {
            var size = Math.Max(iftrue.Count, iffalse.Count);
            var result = new DoubleDerivatives(size);
            var arg = this[0];
            for (var i = 0; i < size; i++)
            {
                var arg2 = iftrue[i] ?? (() => 0.0);
                var arg3 = iffalse[i] ?? (() => 0.0);
                result[i] = () => arg().Equals(0.0) ? arg3() : arg2();
            }
            return result;
        }

        /// <summary>
        /// Equals the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public override Derivatives<Func<double>> Equal(Derivatives<Func<double>> other)
        {
            var result = new DoubleDerivatives();
            var arg1 = this[0];
            var arg2 = other[0];
            result[0] = () => arg1.Equals(arg2) ? 1 : 0;
            return result;
        }

        /// <summary>
        /// Nots the equal.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public override Derivatives<Func<double>> NotEqual(Derivatives<Func<double>> other)
        {
            var result = new DoubleDerivatives();
            var arg1 = this[0];
            var arg2 = other[0];
            result[0] = () => arg1().Equals(arg2()) ? 0 : 1;
            return result;
        }

        /// <summary>
        /// Negate the derivatives.
        /// </summary>
        /// <returns>
        /// The derivatives.
        /// </returns>
        public override Derivatives<Func<double>> Negate()
        {
            var result = new DoubleDerivatives(Count);
            for (var i = 0; i < Count; i++)
            {
                var ai = this[i];
                if (ai != null)
                    result[i] = () => -ai();
            }
            return result;
        }

        /// <summary>
        /// Not (binary) the derivatives.
        /// </summary>
        /// <returns>
        /// The derivatives.
        /// </returns>
        public override Derivatives<Func<double>> Not()
        {
            var result = new DoubleDerivatives();
            var ai = this[0];
            result[0] = () => ai().Equals(0.0) ? 1.0 : 0.0;
            return result;
        }

        /// <summary>
        /// Add derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// The sum.
        /// </returns>
        public override Derivatives<Func<double>> Add(Derivatives<Func<double>> b)
        {
            var size = Math.Max(Count, b.Count);
            var result = new DoubleDerivatives(size);
            for (var i = 0; i < size; i++)
            {
                var ai = this[i];
                var bi = b[i];
                if (ai != null && bi != null)
                    result[i] = () => ai() + bi();
                else if (ai != null)
                    result[i] = ai;
                else if (bi != null)
                    result[i] = bi;
            }
            return result;
        }

        /// <summary>
        /// Subtract derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// The difference.
        /// </returns>
        public override Derivatives<Func<double>> Subtract(Derivatives<Func<double>> b)
        {
            var size = Math.Max(Count, b.Count);
            var result = new DoubleDerivatives(size);
            for (var i = 0; i < size; i++)
            {
                var arg1 = this[i];
                var arg2 = b[i];
                if (arg1 != null && arg2 != null)
                    result[i] = () => arg1() - arg2();
                else if (arg1 != null)
                    result[i] = arg1;
                else if (arg2 != null)
                    result[i] = () => -arg2();
            }
            return result;
        }

        /// <summary>
        /// Multiply derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// The multiplied result.
        /// </returns>
        public override Derivatives<Func<double>> Multiply(Derivatives<Func<double>> b)
        {
            var size = Math.Max(Count, b.Count);
            var result = new DoubleDerivatives(size);
            var a0 = this[0];
            var b0 = b[0];
            result[0] = () => a0() * b0();
            for (var i = 1; i < size; i++)
            {
                var arg1 = this[i];
                var arg2 = b[i];
                if (arg1 != null && arg2 != null)
                    result[i] = () => a0() * arg1() + arg2() * b0();
                else if (arg1 != null)
                    result[i] = () => arg1() * b0();
                else if (arg2 != null)
                    result[i] = () => a0() * arg2();
            }
            return result;
        }

        /// <summary>
        /// Divide derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// The divided result.
        /// </returns>
        public override Derivatives<Func<double>> Divide(Derivatives<Func<double>> b)
        {
            var size = Math.Max(Count, b.Count);
            var result = new DoubleDerivatives(size);
            var a0 = this[0];
            var b0 = b[0];
            result[0] = () => SafeDivide(a0(), b0());
            for (var i = 1; i < size; i++)
            {
                var ai = this[i];
                var bi = b[i];
                if (ai != null && bi != null)
                    result[i] = () =>
                    {
                        var denom = b0();
                        return SafeDivide(denom * ai() - a0() * bi(), denom * denom);
                    };
                else if (ai != null)
                    result[i] = () => SafeDivide(ai(), b0());
                else if (bi != null)
                    result[i] = () =>
                    {
                        var denom = b0();
                        return -SafeDivide(a0() * bi(), denom * denom);
                    };
            }
            return result;
        }

        /// <summary>
        /// Safe version of division where division by 0 is avoided.
        /// </summary>
        /// <param name="a">The numerator.</param>
        /// <param name="b">The denominator.</param>
        /// <returns></returns>
        private static double SafeDivide(double a, double b)
        {
            if (b >= 0)
                b += FudgeFactor;
            else
                b -= FudgeFactor;
            return a / b;
        }

        /// <summary>
        /// Modulo operation on derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// The remainder of the division.
        /// </returns>
        public override Derivatives<Func<double>> Modulo(Derivatives<Func<double>> b)
        {
            var result = new DoubleDerivatives();
            var a0 = this[0];
            var b0 = b[0];
            result[0] = () => a0() % b0();
            return result;
        }

        /// <summary>
        /// Check greater than.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// A value representing true if this is greater.
        /// </returns>
        public override Derivatives<Func<double>> GreaterThan(Derivatives<Func<double>> b)
        {
            var result = new DoubleDerivatives();
            var a0 = this[0];
            var b0 = b[0];
            result[0] = () => a0() > b0() ? 1 : 0;
            return result;
        }

        /// <summary>
        /// Check less than.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// A value representing true if this is less.
        /// </returns>
        public override Derivatives<Func<double>> LessThan(Derivatives<Func<double>> b)
        {
            var result = new DoubleDerivatives();
            var a0 = this[0];
            var b0 = b[0];
            result[0] = () => a0() < b0() ? 1 : 0;
            return result;
        }

        /// <summary>
        /// Check greater or equal.
        /// </summary>
        /// <param name="b">The operand.</param>
        /// <returns>
        /// A value representing true if this is greater or equal.
        /// </returns>
        public override Derivatives<Func<double>> GreaterOrEqual(Derivatives<Func<double>> b)
        {
            var result = new DoubleDerivatives();
            var a0 = this[0];
            var b0 = b[0];
            result[0] = () => a0() >= b0() ? 1 : 0;
            return result;
        }

        /// <summary>
        /// Check less or equal.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>
        /// A value representing true if this is less or equal.
        /// </returns>
        public override Derivatives<Func<double>> LessOrEqual(Derivatives<Func<double>> b)
        {
            var result = new DoubleDerivatives();
            var a0 = this[0];
            var b0 = b[0];
            result[0] = () => a0() <= b0() ? 1 : 0;
            return result;
        }
    }
}
