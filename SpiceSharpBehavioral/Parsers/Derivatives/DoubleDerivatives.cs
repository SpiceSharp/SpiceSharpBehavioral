using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A specific implementation of derivatives for doubles.
    /// </summary>
    public class DoubleDerivatives : Derivatives<double>
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
        public DoubleDerivatives Pow(DoubleDerivatives exponent)
        {
            var size = Math.Max(Count, exponent.Count);
            var result = new DoubleDerivatives(size);
            var a0 = this[0];
            var b0 = exponent[0];
            result[0] = Math.Pow(a0, b0);
            for (var i = 1; i < size; i++)
            {
                // (fx^b)' = b * fx^(b-1) * f'x
                if (!this[i].Equals(0.0))
                    result[i] = b0 * this[i] * Math.Pow(a0, b0 - 1);

                // (fx^gx)' = (e^(gx*ln(fx)))'
                // = fx^(gx-1)*f'x + fx^gx*ln(fx)*g'x
                if (!exponent[i].Equals(0.0))
                    result[i] += result[0] * Math.Log(a0) * exponent[i];
            }
            return result;
        }

        public DoubleDerivatives Or(DoubleDerivatives b)
        {
            var result = new DoubleDerivatives();
            result[0] = this[0].Equals(0.0) && b[0].Equals(0.0) ? 0 : 1;
            return result;
        }

        public DoubleDerivatives And(DoubleDerivatives b)
        {
            var result = new DoubleDerivatives();
            result[0] = this[0].Equals(0.0) || b[0].Equals(0.0) ? 0 : 1;
            return result;
        }

        public DoubleDerivatives IfThen(DoubleDerivatives iftrue, DoubleDerivatives iffalse)
        {
            return this[0].Equals(0.0) ? iffalse : iftrue;
        }

        public DoubleDerivatives Equal(DoubleDerivatives other)
        {
            var result = new DoubleDerivatives();
            result[0] = this[0].Equals(other[0]) ? 1 : 0;
            return result;
        }

        public DoubleDerivatives NotEqual(DoubleDerivatives other)
        {
            var result = new DoubleDerivatives();
            result[0] = this[0].Equals(other[0]) ? 0 : 1;
            return result;
        }

        public static DoubleDerivatives operator -(DoubleDerivatives a)
        {
            var result = new DoubleDerivatives(a.Count);
            for (var i = 0; i < a.Count; i++)
                result[i] = -a[i];
            return result;
        }

        public static DoubleDerivatives operator !(DoubleDerivatives a)
        {
            var result = new DoubleDerivatives();
            result[0] = a[0].Equals(0.0) ? 1.0 : 0.0;
            return result;
        }

        public static DoubleDerivatives operator +(DoubleDerivatives a, DoubleDerivatives b)
        {
            var size = Math.Max(a.Count, b.Count);
            var result = new DoubleDerivatives(size);
            for (var i = 0; i < size; i++)
                result[i] = a[i] + b[i];
            return result;
        }

        public static DoubleDerivatives operator -(DoubleDerivatives a, DoubleDerivatives b)
        {
            var size = Math.Max(a.Count, b.Count);
            var result = new DoubleDerivatives(size);
            for (var i = 0; i < size; i++)
                result[i] = a[i] - b[i];
            return result;
        }

        public static DoubleDerivatives operator *(DoubleDerivatives a, DoubleDerivatives b)
        {
            var size = Math.Max(a.Count, b.Count);
            var result = new DoubleDerivatives(size);
            double a0 = a[0], b0 = b[0];
            result[0] = a0 * b0;
            for (var i = 1; i < size; i++)
                result[i] = a0 * b[i] + a[i] * b0;
            return result;
        }

        public static DoubleDerivatives operator /(DoubleDerivatives a, DoubleDerivatives b)
        {
            var size = Math.Max(a.Count, b.Count);
            var result = new DoubleDerivatives(size);
            double a0 = a[0], b0 = b[0];
            result[0] = a0 / b0;
            for (var i = 1; i < size; i++)
                result[i] = (b0 * a[i] - a0 * b[i]) / b0 / b0;
            return result;
        }

        public static DoubleDerivatives operator %(DoubleDerivatives a, DoubleDerivatives b)
        {
            var result = new DoubleDerivatives();
            result[0] = a[0] % b[0];
            return result;
        }

        public static DoubleDerivatives operator >(DoubleDerivatives a, DoubleDerivatives b)
        {
            var result = new DoubleDerivatives();
            result[0] = a[0] > b[0] ? 1 : 0;
            return result;
        }

        public static DoubleDerivatives operator <(DoubleDerivatives a, DoubleDerivatives b)
        {
            var result = new DoubleDerivatives();
            result[0] = a[0] < b[0] ? 1 : 0;
            return result;
        }

        public static DoubleDerivatives operator >=(DoubleDerivatives a, DoubleDerivatives b)
        {
            var result = new DoubleDerivatives();
            result[0] = a[0] >= b[0] ? 1 : 0;
            return result;
        }

        public static DoubleDerivatives operator <=(DoubleDerivatives a, DoubleDerivatives b)
        {
            var result = new DoubleDerivatives();
            result[0] = a[0] <= b[0] ? 1 : 0;
            return result;
        }
    }
}
