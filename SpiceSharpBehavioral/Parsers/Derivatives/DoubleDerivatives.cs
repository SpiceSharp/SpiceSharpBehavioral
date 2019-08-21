using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A specific implementation of derivatives for doubles.
    /// </summary>
    public class DoubleDerivatives : Derivatives<Func<double>>
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
        public override Derivatives<Func<double>> Pow(Derivatives<Func<double>> exponent)
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

        /// <summary>
        /// Binary OR.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns></returns>
        public override Derivatives<Func<double>> Or(Derivatives<Func<double>> b)
        {
            var result = new DoubleDerivatives();
            var arg1 = this[0];
            var arg2 = b[0];
            result[0] = () => arg1().Equals(0.0) && arg2().Equals(0.0) ? 0 : 1;
            return result;
        }

        public override Derivatives<Func<double>> And(Derivatives<Func<double>> b)
        {
            var result = new DoubleDerivatives();
            var arg1 = this[0];
            var arg2 = b[0];
            result[0] = () => arg1().Equals(0.0) || arg2().Equals(0.0) ? 0 : 1;
            return result;
        }

        public override Derivatives<Func<double>> IfThenElse(Derivatives<Func<double>> iftrue, Derivatives<Func<double>> iffalse)
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

        public override Derivatives<Func<double>> Equal(Derivatives<Func<double>> other)
        {
            var result = new DoubleDerivatives();
            var arg1 = this[0];
            var arg2 = other[0];
            result[0] = () => arg1.Equals(arg2) ? 1 : 0;
            return result;
        }

        public override Derivatives<Func<double>> NotEqual(Derivatives<Func<double>> other)
        {
            var result = new DoubleDerivatives();
            var arg1 = this[0];
            var arg2 = other[0];
            result[0] = () => arg1().Equals(arg2()) ? 0 : 1;
            return result;
        }

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

        public override Derivatives<Func<double>> Not()
        {
            var result = new DoubleDerivatives();
            var ai = this[0];
            result[0] = () => ai().Equals(0.0) ? 1.0 : 0.0;
            return result;
        }

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

        public override Derivatives<Func<double>> Divide(Derivatives<Func<double>> b)
        {
            var size = Math.Max(Count, b.Count);
            var result = new DoubleDerivatives(size);
            var a0 = this[0];
            var b0 = b[0];
            result[0] = () => a0() / b0();
            for (var i = 1; i < size; i++)
            {
                var ai = this[i];
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

        public override Derivatives<Func<double>> Modulo(Derivatives<Func<double>> b)
        {
            var result = new DoubleDerivatives();
            var a0 = this[0];
            var b0 = b[0];
            result[0] = () => a0() % b0();
            return result;
        }

        public override Derivatives<Func<double>> GreaterThan(Derivatives<Func<double>> b)
        {
            var result = new DoubleDerivatives();
            var a0 = this[0];
            var b0 = b[0];
            result[0] = () => a0() > b0() ? 1 : 0;
            return result;
        }

        public override Derivatives<Func<double>> LessThan(Derivatives<Func<double>> b)
        {
            var result = new DoubleDerivatives();
            var a0 = this[0];
            var b0 = b[0];
            result[0] = () => a0() < b0() ? 1 : 0;
            return result;
        }

        public override Derivatives<Func<double>> GreaterOrEqual(Derivatives<Func<double>> b)
        {
            var result = new DoubleDerivatives();
            var a0 = this[0];
            var b0 = b[0];
            result[0] = () => a0() >= b0() ? 1 : 0;
            return result;
        }

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
