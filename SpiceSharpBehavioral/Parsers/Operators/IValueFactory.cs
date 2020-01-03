using SpiceSharp;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Describes the generation of values based on their textual representation.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public interface IValueFactory<T> : IParameterSet
    {
        /// <summary>
        /// Creates the variable based on its textual representation.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>The value of the variable.</returns>
        T CreateVariable(string variable);

        /// <summary>
        /// Creates the value based on its textual representation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="units">The units of the value.</param>
        /// <returns>
        /// The value of the variable.
        /// </returns>
        T CreateValue(double value, string units = "");

        /// <summary>
        /// Creates a function of the specified name.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="arguments">The arguments of the function.</param>
        /// <returns>
        /// The value of the function.
        /// </returns>
        T CreateFunction(string name, params T[] arguments);

        /// <summary>
        /// Creates the property.
        /// </summary>
        /// <param name="type">The type of property.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        T CreateProperty(PropertyType type, IReadOnlyList<string> arguments);
    }

    /// <summary>
    /// A list of supported property types.
    /// </summary>
    public enum PropertyType
    {
        /// <summary>
        /// A voltage property V(...).
        /// </summary>
        Voltage,

        /// <summary>
        /// A current property I(...).
        /// </summary>
        Current,

        /// <summary>
        /// An entity property @...[...].
        /// </summary>
        Property
    }
}
