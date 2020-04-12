using SpiceSharp;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Spice-specific helper methods.
    /// </summary>
    public static class SpiceHelper
    {
        /// <summary>
        /// Parses a number literal.
        /// </summary>
        /// <param name="literal">The literal.</param>
        /// <returns>The parsed number.</returns>
        /// <exception cref="Exception">Thrown if the number can't be parsed.</exception>
        public static double ParseNumber(string literal)
        {
            double value = 0.0;
            int index = 0;
            if ((literal[index] < '0' || literal[index] > '9') && literal[index] != '.')
                throw new Exception("Cannot read the number '{0}'".FormatString(literal));

            // Read integer part
            while (index < literal.Length && (literal[index] >= '0' && literal[index] <= '9'))
                value = (value * 10.0) + (literal[index++] - '0');

            // Read decimal part
            if (index < literal.Length && (literal[index] == '.'))
            {
                index++;
                double mult = 1.0;
                while (index < literal.Length && (literal[index] >= '0' && literal[index] <= '9'))
                {
                    value = (value * 10.0) + (literal[index++] - '0');
                    mult *= 10.0;
                }

                value /= mult;
            }

            if (index < literal.Length)
            {
                // Scientific notation
                if (literal[index] == 'e' || literal[index] == 'E')
                {
                    index++;
                    var exponent = 0;
                    var neg = false;
                    if (index < literal.Length && (literal[index] == '+' || literal[index] == '-'))
                    {
                        if (literal[index] == '-')
                            neg = true;
                        index++;
                    }

                    // Get the exponent
                    while (index < literal.Length && (literal[index] >= '0' && literal[index] <= '9'))
                        exponent = (exponent * 10) + (literal[index++] - '0');

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
                    switch (literal[index])
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
                            if (index + 2 < literal.Length &&
                                (literal[index + 1] == 'e' || literal[index + 1] == 'E') &&
                                (literal[index + 2] == 'g' || literal[index + 2] == 'G'))
                            {
                                value *= 1.0e6;
                                index += 3;
                            }
                            else if (index + 2 < literal.Length &&
                                (literal[index + 1] == 'i' || literal[index + 1] == 'I') &&
                                (literal[index + 2] == 'l' || literal[index + 2] == 'L'))
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
            }
            return value;
        }

        /// <summary>
        /// Femto-...
        /// </summary>
        /// <param name="value">The value in femto-...</param>
        /// <returns>The value.</returns>
        [SuppressMessage("Microsoft.Design", "IDE1006", Justification = "This is to Spice notation.")]
        public static double f(this double value) => value * 1e-15;

        /// <summary>
        /// Pico-...
        /// </summary>
        /// <param name="value">The value in pico-...</param>
        /// <returns>The value.</returns>
        [SuppressMessage("Microsoft.Design", "IDE1006", Justification = "This is to Spice notation.")]
        public static double p(this double value) => value * 1e-12;

        /// <summary>
        /// Nano-...
        /// </summary>
        /// <param name="value">The value in nano-...</param>
        /// <returns>The value.</returns>
        [SuppressMessage("Microsoft.Design", "IDE1006", Justification = "This is to Spice notation.")]
        public static double n(this double value) => value * 1e-9;

        /// <summary>
        /// Micro-...
        /// </summary>
        /// <param name="value">The value in micro-....</param>
        /// <returns>The value.</returns>
        [SuppressMessage("Microsoft.Design", "IDE1006", Justification = "This is to Spice notation.")]
        public static double u(this double value) => value * 1e-6;

        /// <summary>
        /// Milli-...
        /// </summary>
        /// <param name="value">The value in milli-....</param>
        /// <returns>The value.</returns>
        [SuppressMessage("Microsoft.Design", "IDE1006", Justification = "This is to Spice notation.")]
        public static double m(this double value) => value * 1e-6;

        /// <summary>
        /// Kilo-...
        /// </summary>
        /// <param name="value">The value in kilo-...</param>
        /// <returns>The value.</returns>
        [SuppressMessage("Microsoft.Design", "IDE1006", Justification = "This is to Spice notation.")]
        public static double k(this double value) => value * 1e3;

        /// <summary>
        /// Mega-...
        /// </summary>
        /// <param name="value">The value in mega-...</param>
        /// <returns>The value.</returns>
        public static double Meg(this double value) => value * 1e6;

        /// <summary>
        /// Giga-...
        /// </summary>
        /// <param name="value">The value in giga-...</param>
        /// <returns>The value.</returns>
        public static double G(this double value) => value * 1e9;

        /// <summary>
        /// Tera-...
        /// </summary>
        /// <param name="value">The value in tera-...</param>
        /// <returns>The value.</returns>
        public static double T(this double value) => value * 1e9;
    }
}
