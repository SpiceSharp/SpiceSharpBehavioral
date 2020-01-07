using SpiceSharp;
using SpiceSharp.Attributes;
using SpiceSharpBehavioral.Parsers.Operators;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.DoubleFunc
{
    /// <summary>
    /// A value factory for double functions.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IValueFactory{T}" />
    public class ValueFactory : ValueFactory<Func<double>>
    {
        /// <summary>
        /// Creates the value based on its textual representation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="units">The units of the value.</param>
        /// <returns>
        /// The value of the variable.
        /// </returns>
        public override Func<double> CreateValue(double value, string units) => () => value;

        /// <summary>
        /// Creates the property.
        /// </summary>
        /// <param name="type">The type of property.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public override Func<double> CreateProperty(PropertyType type, IReadOnlyList<string> arguments)
        {
            throw new NotSupportedException();
        }
    }
}
