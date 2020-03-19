using System;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// Supporting functions for behavioral models.
    /// </summary>
    public static class Functions
    {
        /// <summary>
        /// Divides two numbers while avoiding division by 0 using a fudge factor.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <param name="fudgeFactor">The fudge factor.</param>
        /// <returns>
        /// The division.
        /// </returns>
        public static double SafeDivide(double left, double right, double fudgeFactor)
        {
            if (right < 0)
                right -= fudgeFactor;
            else
                right += fudgeFactor;
            if (right.Equals(0.0))
                return double.PositiveInfinity;
            return left / right;
        }

        /// <summary>
        /// Takes the natural logarithm.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The natural logarithm.</returns>
        public static double Log(double arg)
        {
            if (arg < 0)
                return double.PositiveInfinity;
            return Math.Log(arg);
        }

        /// <summary>
        /// Takes the logarithm base 10.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The logarithm baes 10.</returns>
        public static double Log10(double arg)
        {
            if (arg < 0)
                return double.PositiveInfinity;
            return Math.Log10(arg);
        }

        /// <summary>
        /// Raises a number to a power. The function is made symmetrical.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The result.</returns>
        public static double Power(double left, double right) => Math.Pow(Math.Abs(left), right);

        /// <summary>
        /// Raises a number to a power. The function is made antisymmetrical.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The result.</returns>
        public static double Power2(double left, double right)
        {
            if (left < 0)
                return -Math.Pow(-left, right);
            else
                return Math.Pow(left, right);
        }

        /// <summary>
        /// The step function.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static double Step(double arg)
        {
            if (arg < 0.0)
                return 0.0;
            else if (arg > 0.0)
                return 1.0;
            else
                return 0.5; /* Ick! */
        }

        /// <summary>
        /// The step function with an initial ramp.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static double Step2(double arg)
        {
            if (arg <= 0.0)
                return 0.0;
            else if (arg <= 1.0)
                return arg;
            else /* if (arg > 1.0) */
                return 1.0;
        }

        /// <summary>
        /// The derivative of <see cref="Step2(double)"/>.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        public static double Step2Derivative(double arg)
        {
            if (arg <= 0.0)
                return 0.0;
            else if (arg <= 1.0)
                return 1.0;
            else
                return 0.0;
        }

        /// <summary>
        /// The ramp function.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static double Ramp(double arg)
        {
            if (arg < 0)
                return 0.0;
            return arg;
        }

        /// <summary>
        /// The derivative of <see cref="RampDerivative(double)"/>
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        public static double RampDerivative(double arg)
        {
            if (arg < 0)
                return 0.0;
            return 1.0;
        }

        /// <summary>
        /// Piece-wise linear interpolation.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="data">The interpolation data.</param>
        /// <returns>The interpolated value.</returns>
        public static double Pwl(double arg, Point[] data)
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

        /// <summary>
        /// The derivative of <see cref="Pwl(double, Point[])" />.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="data">The interpolation data.</param>
        /// <returns>The interpolated value.</returns>
        public static double PwlDerivative(double arg, Point[] data)
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

        /// <summary>
        /// Squares a value.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static double Square(double arg) => arg * arg;

        /// <summary>
        /// Calculates the square root of a number.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static double Sqrt(double arg)
        {
            if (arg < 0.0)
                return double.PositiveInfinity;
            return Math.Sqrt(arg);
        }

        /// <summary>
        /// Returns the sign of the argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>-1.0 if negative; 1.0 if positive; otherwise 0.0.</returns>
        public static double Sign(double arg)
        {
            if (arg < 0)
                return -1.0;
            if (arg > 0)
                return 1.0;
            return 0.0;
        }
    }
}
