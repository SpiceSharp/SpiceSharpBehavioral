using System;
using System.Numerics;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// Supporting functions for behavioral models.
    /// </summary>
    public static class HelperFunctions
    {
        /// <summary>
        /// Gets or sets a fudge factor for avoid edge cases (like division by 0).
        /// </summary>
        public static double FudgeFactor { get; set; } = 1e-32;

        /// <summary>
        /// Divides two numbers while avoiding division by 0 using a fudge factor.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        ///     The division.
        /// </returns>
        public static double SafeDivide(double left, double right)
        {
            if (right < 0)
                right -= FudgeFactor;
            else
                right += FudgeFactor;
            if (right.Equals(0.0))
                return double.PositiveInfinity;
            return left / right;
        }

        /// <summary>
        /// Divides two complex numbers while avoiding division by 0 using a fudge factor.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        ///     The division.
        /// </returns>
        public static Complex SafeDivide(Complex left, Complex right)
        {
            if (Math.Abs(right.Imaginary).Equals(0.0))
            {
                if (right.Real < 0)
                    right -= FudgeFactor;
                else
                    right += FudgeFactor;
            }
            if (right.Real.Equals(0.0) && right.Imaginary.Equals(0.0))
                return double.PositiveInfinity;
            return left / right;
        }

        /// <summary>
        /// Checks if two numbers are equal.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <param name="relTol">The relative tolerance.</param>
        /// <param name="absTol">The absolute tolerance.</param>
        /// <returns>
        ///     <c>true</c> if the two numbers are within tolerance; otherwise <c>false</c>.
        /// </returns>
        public static bool Equals(double left, double right, double relTol, double absTol)
        {
            var tol = Math.Max(Math.Abs(left), Math.Abs(right)) * relTol + absTol;
            if (Math.Abs(left - right) <= tol)
                return true;
            return false;
        }

        /// <summary>
        /// Checks if two complex numbers are equal.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <param name="relTol">The relative tolerance.</param>
        /// <param name="absTol">The absolute tolerance.</param>
        /// <returns>
        ///     <c>true</c> if the two numbers are within tolerance; otherwise <c>false</c>.
        /// </returns>
        public static bool Equals(Complex left, Complex right, double relTol, double absTol)
        {
            var tol = Math.Max(Math.Abs(left.Real), Math.Abs(right.Real)) * relTol + absTol;
            if (Math.Abs(left.Real - right.Real) > tol)
                return false;
            tol = Math.Max(Math.Abs(left.Imaginary), Math.Abs(right.Imaginary)) * relTol + absTol;
            if (Math.Abs(left.Imaginary - right.Imaginary) > tol)
                return false;
            return true;
        }

        /// <summary>
        /// Gets the real part of a complex number.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The real part.</returns>
        public static Complex Real(Complex arg) => arg.Real;

        /// <summary>
        /// Gets the imaginary part of a complex number.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The imaginary part.</returns>
        public static Complex Imag(Complex arg) => arg.Imaginary;

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
        /// Takes the natural logarithm.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The natural logarithm.</returns>
        public static Complex Log(Complex arg)
        {
            if (arg.Imaginary.Equals(0.0))
                return Log(arg.Real);
            return Complex.Log(arg);
        }

        /// <summary>
        /// Takes the logarithm base 10.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The logarithm base 10.</returns>
        public static double Log10(double arg)
        {
            if (arg < 0)
                return double.PositiveInfinity;
            return Math.Log10(arg);
        }

        /// <summary>
        /// Takes the logarithm base 10.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The logarithm base 10.</returns>
        public static Complex Log10(Complex arg)
        {
            if (arg.Imaginary.Equals(0.0))
                return Log10(arg.Real);
            return Complex.Log10(arg);
        }

        /// <summary>
        /// Gets the magnitude of the complex value in decibels.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The magnitude in decibels.</returns>
        public static Complex Decibels(Complex arg) => 10 * Log10(arg.Real * arg.Real + arg.Imaginary * arg.Imaginary);

        /// <summary>
        /// Gets the phase of the complex value in degrees.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The phase in degrees.</returns>
        public static Complex Phase(Complex arg) => arg.Phase * 180.0 / Math.PI;

        /// <summary>
        /// Computes the inverse arc tangent hyperbolic.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static double Atanh(double arg) => 0.5 * Log(SafeDivide(1 + arg, 1 - arg));

        /// <summary>
        /// Computes the inverse arc tangent hyperbolic.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static Complex Atanh(Complex arg) => 0.5 * Log(SafeDivide(1 + arg, 1 - arg));

        /// <summary>
        /// Raises a number to a power.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns></returns>
        public static double Pow(double left, double right)
        {
            if (left.Equals(0.0) && right <= 0.0)
                left += FudgeFactor;
            return Math.Pow(left, right);
        }

        /// <summary>
        /// Raises a number to a power. The function is made symmetrical. Also known as "pwr".
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The result.</returns>
        public static double Power(double left, double right)
        {
            if (left.Equals(0.0) && right <= 0.0)
                left += FudgeFactor;
            return Math.Pow(Math.Abs(left), right);
        }

        /// <summary>
        /// Raises a number to a power. The function is made radially symmetrical. Also known as "pwr".
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The result.</returns>
        public static Complex Power(Complex left, Complex right) => Complex.Pow(left.Magnitude, right);

        /// <summary>
        /// Raises a number to a power. The function is made antisymmetrical. Also known as "pwrs".
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The result.</returns>
        public static double Power2(double left, double right)
        {
            if (left < 0)
                return -Math.Pow(-left, right);
            else
            {
                if (left.Equals(0.0) && right < 0.0)
                    left += FudgeFactor;
                return Math.Pow(left, right);
            }
        }

        /// <summary>
        /// Raises a number to a power. The function is made antisymmetrical around the imaginary axis. Also known as "pwrs".
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>The result.</returns>
        public static Complex Power2(Complex left, Complex right)
        {
            if (left.Imaginary.Equals(0.0) && right.Imaginary.Equals(0.0))
                return Power2(left.Real, right.Real);
            if (left.Real < 0)
                return -Complex.Pow(-left.Real, right);
            else
            {
                double r = left.Real;
                if (r.Equals(0.0) && right.Real < 0.0)
                    r += FudgeFactor;
                return Complex.Pow(r, right);
            }
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
        /// The step function.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static Complex Step(Complex arg) => Step(arg.Real);

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
        /// The step function with an initial ramp.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static Complex Step2(Complex arg) => Step2(arg.Real);

        /// <summary>
        /// The derivative of <see cref="Step2(double)"/>.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The derivative of the step function with initial ramp.</returns>
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
        /// The derivative of <see cref="Step2(double)"/>.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The derivative of the step function with initial ramp.</returns>
        public static Complex Step2Derivative(Complex arg) => Step2Derivative(arg.Real);

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
        /// The ramp function.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static Complex Ramp(Complex arg) => Ramp(arg.Real);

        /// <summary>
        /// The derivative of <see cref="Ramp(double)"/>
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The derivative of a ramp.</returns>
        public static double RampDerivative(double arg)
        {
            if (arg < 0)
                return 0.0;
            return 1.0;
        }

        /// <summary>
        /// The derivative of <see cref="Ramp(Complex)"/> over the real part.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The derivative of a ramp.</returns>
        public static Complex RampDerivative(Complex arg) => RampDerivative(arg.Real);

        /// <summary>
        /// Piece-wise linear interpolation.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="data">The interpolation data.</param>
        /// <returns>The interpolated value.</returns>
        public static double Pwl(double arg, Point[] data)
        {
            if (arg <= data[0].X)
            {
                return data[0].Y;
            }

            if (arg >= data[data.Length - 1].X)
            {
                return data[data.Length - 1].Y;
            }

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
            if (arg <= data[0].X || arg >= data[data.Length - 1].X)
            {
                return 0.0;
            }

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
        /// Squares a value.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static Complex Square(Complex arg) => arg * arg;

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
        /// Calculates the square root of a number.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The result.</returns>
        public static Complex Sqrt(Complex arg)
        {
            if (arg.Imaginary.Equals(0.0))
                return Sqrt(arg.Real);
            return Complex.Sqrt(arg);
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

        /// <summary>
        /// Returns the hypothenuse (sqrt(x^2+y^2)).
        /// </summary>
        /// <param name="y">The first argument.</param>
        /// <param name="x">The second argument.</param>
        /// <returns>The hypothenuse.</returns>
        public static double Hypot(double y, double x) => Math.Sqrt(x * x + y * y);

        /// <summary>
        /// Returns the intermediate value of x, y, z.
        /// </summary>
        /// <param name="x">The argument</param>
        /// <param name="y">Mininum value.</param>
        /// <param name="z">Max value.</param>
        /// <returns></returns>
        public static double Limit(double x, double y, double z)
        {
            return Math.Min(Math.Max(x, Math.Min(y, z)), Math.Max(y, z));
        }

        /// <summary>
        /// Returns the intermediate value of x, y, z.
        /// </summary>
        /// <param name="x">The argument</param>
        /// <param name="y">Mininum value.</param>
        /// <param name="z">Max value.</param>
        /// <returns></returns>
        public static Complex Limit(Complex x, Complex y, Complex z)
        {
            return Math.Min(Math.Max(x.Real, Math.Min(y.Real, z.Real)), Math.Max(y.Real, z.Real));
        }
        /// <summary>
        /// Returns the hypothenuse (sqrt(|x|^2+|y|^2)).
        /// </summary>
        /// <param name="y">The first argument.</param>
        /// <param name="x">The second argument.</param>
        /// <returns>The hypothenuse.</returns>
        public static Complex Hypot(Complex y, Complex x)
        {
            return Math.Sqrt(
                y.Real * y.Real + y.Imaginary * y.Imaginary +
                x.Real * x.Real + x.Imaginary * x.Imaginary);
        }

        /// <summary>
        /// Returns the rounded value of the complex number.
        /// </summary>
        /// <param name="a">The number.</param>
        /// <param name="n">The number of decimals.</param>
        /// <returns></returns>
        public static Complex Round(Complex a, int n)
        {
            return new Complex(Math.Round(a.Real, n), Math.Round(a.Imaginary, n));
        }

        /// <summary>
        /// Returns the floor.
        /// </summary>
        /// <param name="a">The argument.</param>
        /// <returns>The floor value.</returns>
        public static Complex Floor(Complex a)
        {
            return new Complex(
                Math.Floor(a.Real),
                Math.Floor(a.Imaginary));
        }

        /// <summary>
        /// Returns the ceiling.
        /// </summary>
        /// <param name="a">The argument.</param>
        /// <returns>The ceiling value.</returns>
        public static Complex Ceiling(Complex a)
        {
            return new Complex(
                Math.Ceiling(a.Real),
                Math.Ceiling(a.Imaginary));
        }

        /// <summary>
        /// Takes the absolute value of a complex number.
        /// </summary>
        /// <param name="a">The complex number.</param>
        /// <returns>The absolute value.</returns>
        public static Complex Abs(Complex a) => a.Magnitude;

        /// <summary>
        /// The minimum of two complex numbers (only uses the real parts).
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <returns>The smallest real part of either arguments.</returns>
        public static Complex Min(Complex a, Complex b) => Math.Min(a.Real, b.Real);

        /// <summary>
        /// The maximum of two complex numbers (only uses the real parts).
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <returns>The largest real part of either arguments.</returns>
        public static Complex Max(Complex a, Complex b) => Math.Max(a.Real, b.Real);

        /// <summary>
        /// The complex version for atan2 (only uses the real parts).
        /// </summary>
        /// <param name="y">The first argument.</param>
        /// <param name="x">The second argument.</param>
        /// <returns>The angle in a 2D plane.</returns>
        public static Complex Atan2(Complex y, Complex x) => Math.Atan2(y.Real, x.Real);
    }
}
