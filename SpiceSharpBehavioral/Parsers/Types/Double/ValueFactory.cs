using SpiceSharp;
using System;
using System.Collections.Generic;
using SpiceSharpBehavioral.Parsers.Operators;

namespace SpiceSharpBehavioral.Parsers.Double
{
    /// <summary>
    /// A value factory for doubles.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IValueFactory{Double}" />
    public class ValueFactory : ValueFactory<double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFactory"/> class.
        /// </summary>
        public ValueFactory()
        {
            FunctionFound += Defaults.FunctionFound;
        }

        /// <summary>
        /// Creates the value based on its textual representation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="units">The units of the value.</param>
        /// <returns>
        /// The value of the variable.
        /// </returns>
        public override double CreateValue(double value, string units) => value;

        /// <summary>
        /// Creates the property.
        /// </summary>
        /// <param name="type">The type of property.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public override double CreateProperty(PropertyType type, IReadOnlyList<string> arguments)
        {
            throw new NotSupportedException();
        }
    }
}
