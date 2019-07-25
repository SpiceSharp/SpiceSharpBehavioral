using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A specific implementation of derivatives for doubles.
    /// </summary>
    public class DoubleDerivatives : Derivatives<Func<double>>, IDerivatives<double>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DoubleDerivatives"/> class.
        /// </summary>
        public DoubleDerivatives()
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="DoubleDerivatives"/> class.
        /// </summary>
        /// <param name="capacity"></param>
        public DoubleDerivatives(int capacity)
            : base(capacity)
        { }

        /// <summary>
        /// Gets the derivative.
        /// </summary>
        /// <param name="index">The index of the derivative. 0 for the value itself.</param>
        /// <returns></returns>
        public Func<double> GetDerivative(int index) => this[index];

        /// <summary>
        /// Check equality.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns></returns>
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
        /// Calculate the power.
        /// </summary>
        /// <param name="exponent">The exponent.</param>
        /// <returns></returns>
        public DoubleDerivatives Pow(DoubleDerivatives exponent)
        {
            var size = Math.Max(Count, exponent.Count);
            var result = new DoubleDerivatives(size);
            var a0 = this[0];
            var b0 = exponent[0];
            result[0] = () => Math.Pow(a0(), b0());
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
                        var tmpa0 = a0();
                        var tmpb0 = b0();
                        return tmpb0 * Math.Pow(tmpa0, tmpb0 - 1) * ai() + Math.Pow(tmpa0, tmpb0) * Math.Log(tmpa0) * bi();
                    };
                }
                else if (this[i] != null)
                {
                    // (fx^b)' = b * fx^(b-1) * f'x
                    var ai = this[i];
                    result[i] = () =>
                    {
                        var tmp = b0();
                        return tmp * ai() * Math.Pow(a0(), tmp - 1);
                    };
                }
                else if (exponent[i] != null)
                {
                    // (a^gx)' = a^gx * ln(a) * g'x
                    var bi = exponent[i];
                    result[i] = () =>
                    {
                        var tmpa0 = a0();
                        var tmpb0 = b0();
                        return Math.Pow(tmpa0, tmpb0) * Math.Log(tmpa0) * bi();
                    };
                }
            }
            return result;
        }

        public DoubleDerivatives Or(DoubleDerivatives b)
        {
            var result = new DoubleDerivatives();
            var arg1 = this[0];
            var arg2 = b[0];
            result[0] = () => arg1().Equals(0.0) && arg2().Equals(0.0) ? 0 : 1;
            return result;
        }

        public DoubleDerivatives And(DoubleDerivatives b)
        {
            var result = new DoubleDerivatives();
            var arg1 = this[0];
            var arg2 = b[0];
            result[0] = () => arg1().Equals(0.0) || arg2().Equals(0.0) ? 0 : 1;
            return result;
        }

        public DoubleDerivatives IfThen(DoubleDerivatives iftrue, DoubleDerivatives iffalse)
        {
            var size = Math.Max(iftrue.Count, iffalse.Count);
            var result = new DoubleDerivatives(size);
            var arg = this[0];
            for (var i = 0; i < size; i++)
            {
                var arg2 = iftrue[i] ?? (() => 0.0);
                var arg3 = iffalse[i] ?? (() => 0.0);
                result[i] = () => arg().Equals(0.0) ? arg2() : arg3();
            }
            return result;
        }

        public DoubleDerivatives Equal(DoubleDerivatives other)
        {
            var result = new DoubleDerivatives();
            var arg1 = this[0];
            var arg2 = other[0];
            result[0] = () => arg1.Equals(arg2) ? 1 : 0;
            return result;
        }

        public DoubleDerivatives NotEqual(DoubleDerivatives other)
        {
            var result = new DoubleDerivatives();
            var arg1 = this[0];
            var arg2 = other[0];
            result[0] = () => arg1().Equals(arg2()) ? 0 : 1;
            return result;
        }

        public static DoubleDerivatives operator -(DoubleDerivatives a)
        {
            var result = new DoubleDerivatives(a.Count);
            for (var i = 0; i < a.Count; i++)
            {
                var arg = a[i];
                if (arg != null)
                    result[i] = () => -arg();
            }
            return result;
        }

        public static DoubleDerivatives operator !(DoubleDerivatives a)
        {
            var result = new DoubleDerivatives();
            var arg = a[0];
            result[0] = () => arg().Equals(0.0) ? 1.0 : 0.0;
            return result;
        }

        public static DoubleDerivatives operator +(DoubleDerivatives a, DoubleDerivatives b)
        {
            var size = Math.Max(a.Count, b.Count);
            var result = new DoubleDerivatives(size);
            for (var i = 0; i < size; i++)
            {
                var arg1 = a[i];
                var arg2 = b[i];
                if (arg1 != null && arg2 != null)
                    result[i] = () => arg1() + arg2();
                else if (arg1 != null)
                    result[i] = arg1;
                else if (arg2 != null)
                    result[i] = arg2;
            }
            return result;
        }

        public static DoubleDerivatives operator -(DoubleDerivatives a, DoubleDerivatives b)
        {
            var size = Math.Max(a.Count, b.Count);
            var result = new DoubleDerivatives(size);
            for (var i = 0; i < size; i++)
            {
                var arg1 = a[i];
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

        public static DoubleDerivatives operator *(DoubleDerivatives a, DoubleDerivatives b)
        {
            var size = Math.Max(a.Count, b.Count);
            var result = new DoubleDerivatives(size);
            var a0 = a[0];
            var b0 = b[0];
            result[0] = () => a0() * b0();
            for (var i = 1; i < size; i++)
            {
                var arg1 = a[i];
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

        public static DoubleDerivatives operator /(DoubleDerivatives a, DoubleDerivatives b)
        {
            var size = Math.Max(a.Count, b.Count);
            var result = new DoubleDerivatives(size);
            var a0 = a[0];
            var b0 = b[0];
            result[0] = () => a0() / b0();
            for (var i = 1; i < size; i++)
            {
                var ai = a[i];
                var bi = b[i];
                if (ai != null && bi != null)
                    result[i] = () =>
                    {
                        var denom = b0();
                        return (denom * ai() - a0() * bi()) / denom / denom;
                    };
                else if (ai != null)
                    result[i] = () => ai() / b0();
                else if (bi != null)
                    result[i] = () =>
                    {
                        var denom = b0();
                        return -a0() * bi() / denom / denom;
                    };
            }
            return result;
        }

        public static DoubleDerivatives operator %(DoubleDerivatives a, DoubleDerivatives b)
        {
            var result = new DoubleDerivatives();
            var arg1 = a[0];
            var arg2 = b[0];
            result[0] = () => arg1() % arg2();
            return result;
        }

        public static DoubleDerivatives operator >(DoubleDerivatives a, DoubleDerivatives b)
        {
            var result = new DoubleDerivatives();
            var arg1 = a[0];
            var arg2 = b[0];
            result[0] = () => arg1() > arg2() ? 1 : 0;
            return result;
        }

        public static DoubleDerivatives operator <(DoubleDerivatives a, DoubleDerivatives b)
        {
            var result = new DoubleDerivatives();
            var arg1 = a[0];
            var arg2 = b[0];
            result[0] = () => arg1() < arg2() ? 1 : 0;
            return result;
        }

        public static DoubleDerivatives operator >=(DoubleDerivatives a, DoubleDerivatives b)
        {
            var result = new DoubleDerivatives();
            var arg1 = a[0];
            var arg2 = b[0];
            result[0] = () => arg1() >= arg2() ? 1 : 0;
            return result;
        }

        public static DoubleDerivatives operator <=(DoubleDerivatives a, DoubleDerivatives b)
        {
            var result = new DoubleDerivatives();
            var arg1 = a[0];
            var arg2 = b[0];
            result[0] = () => arg1() <= arg2() ? 1 : 0;
            return result;
        }
    }
}
