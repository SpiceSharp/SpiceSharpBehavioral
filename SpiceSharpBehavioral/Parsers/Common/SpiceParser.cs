using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// A class capable of parsing Spice-like arithmetic
    /// </summary>
    public abstract class SpiceParser : StandardArithmeticParser
    {
        /// <summary>
        /// Parse unary operators.
        /// </summary>
        /// <returns></returns>
        protected override bool ParseUnaryOperator()
        {
            // We want to prevent methods from being detected for the following cases:
            // - V(...)
            // - I(...)
            // - @...[...]
            switch (Input[Index])
            {
                case 'v':
                case 'V':
                case 'i':
                case 'I':
                    if (NextChar() == '(')
                        return false;
                    break;
                case '@':
                    return false;
            }

            // In other cases, just continue...
            return base.ParseUnaryOperator();
        }

        /// <summary>
        /// Parse a value.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if a value was parsed; otherwise, <c>false</c>.
        /// </returns>
        protected override bool ParseValue()
        {
            // Try to find a number
            if (ParseDouble(out var value))
            {
                PushValue(value);
                return true;
            }

            SpiceProperty args;
            string arg;
            switch (Input[Index])
            {
                case 'v':
                case 'V':
                    if (NextChar() == '(')
                    {
                        string id = Input[Index].ToString();
                        Index += 2;
                        arg = NextUntil(')');
                        args = new SpiceProperty(id, arg.Split(','));
                        Index += arg.Length + 1;
                        PushSpiceProperty(args);
                        return true;
                    }
                    break;

                case 'i':
                case 'I':
                    if (NextChar() == '(')
                    {
                        string id = Input[Index].ToString();
                        Index += 2;
                        arg = NextUntil(')');
                        Index += arg.Length + 1;
                        args = new SpiceProperty(id, arg.Split(','));
                        PushSpiceProperty(args);
                        return true;
                    }
                    break;

                case '@':
                    Index++;
                    var name = NextUntil('[');
                    Index += name.Length + 1;
                    arg = NextUntil(']');
                    Index += arg.Length + 1;
                    args = new SpiceProperty("@", name, arg);
                    PushSpiceProperty(args);
                    return true;
            }

            // Try parsing variables or Spice properties
            if (base.ParseValue())
                return true;

            return false;
        }

        /// <summary>
        /// Push a value on the stack.
        /// </summary>
        /// <param name="value">The parsed value.</param>
        protected abstract void PushValue(double value);

        /// <summary>
        /// Push a spice property on the value stack.
        /// </summary>
        /// <param name="property">The Spice property.</param>
        protected abstract void PushSpiceProperty(SpiceProperty property);

        /// <summary>
        /// Parse a double value.
        /// </summary>
        /// <returns>Parse result.</returns>
        protected bool ParseDouble(out double value)
        {
            value = 0.0;
            if ((Input[Index] < '0' || Input[Index] > '9') && Input[Index] != '.')
                return false;

            // Read integer part
            while (Index < Count && (Input[Index] >= '0' && Input[Index] <= '9'))
                value = (value * 10.0) + (Input[Index++] - '0');

            // Read decimal part
            if (Index < Count && (Input[Index] == '.'))
            {
                Index++;
                double mult = 1.0;
                while (Index < Count && (Input[Index] >= '0' && Input[Index] <= '9'))
                {
                    value = (value * 10.0) + (Input[Index++] - '0');
                    mult *= 10.0;
                }

                value /= mult;
            }

            if (Index < Count)
            {
                // Scientific notation
                if (Input[Index] == 'e' || Input[Index] == 'E')
                {
                    Index++;
                    var exponent = 0;
                    var neg = false;
                    if (Index < Count && (Input[Index] == '+' || Input[Index] == '-'))
                    {
                        if (Input[Index] == '-')
                            neg = true;

                        Index++;
                    }

                    // Get the exponent
                    while (Index < Count && (Input[Index] >= '0' && Input[Index] <= '9'))
                        exponent = (exponent * 10) + (Input[Index++] - '0');

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
                    switch (Input[Index])
                    {
                        case 't':
                        case 'T': value *= 1.0e12; Index++; break;
                        case 'g':
                        case 'G': value *= 1.0e9; Index++; break;
                        case 'x':
                        case 'X': value *= 1.0e6; Index++; break;
                        case 'k':
                        case 'K': value *= 1.0e3; Index++; break;
                        case 'u':
                        case 'µ':
                        case 'U': value /= 1.0e6; Index++; break;
                        case 'n':
                        case 'N': value /= 1.0e9; Index++; break;
                        case 'p':
                        case 'P': value /= 1.0e12; Index++; break;
                        case 'f':
                        case 'F': value /= 1.0e15; Index++; break;
                        case 'm':
                        case 'M':
                            if (Index + 2 < Count &&
                                (Input[Index + 1] == 'e' || Input[Index + 1] == 'E') &&
                                (Input[Index + 2] == 'g' || Input[Index + 2] == 'G'))
                            {
                                value *= 1.0e6;
                                Index += 3;
                            }
                            else if (Index + 2 < Count &&
                                (Input[Index + 1] == 'i' || Input[Index + 1] == 'I') &&
                                (Input[Index + 2] == 'l' || Input[Index + 2] == 'L'))
                            {
                                value *= 25.4e-6;
                                Index += 3;
                            }
                            else
                            {
                                value /= 1.0e3;
                                Index++;
                            }

                            break;
                    }
                }

                // Any trailing letters are ignored
                while (Index < Count && ((Input[Index] >= 'a' && Input[Index] <= 'z') || (Input[Index] >= 'A' && Input[Index] <= 'Z')))
                    Index++;
            }
            return true;
        }
    }
}
