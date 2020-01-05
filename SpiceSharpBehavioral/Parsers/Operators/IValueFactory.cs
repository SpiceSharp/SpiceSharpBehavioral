using SpiceSharp;
using System;
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
    [Flags]
    public enum PropertyType
    {
        /// <summary>
        /// A voltage property.
        /// </summary>
        Voltage = 0x01,

        /// <summary>
        /// A current property.
        /// </summary>
        Current = 0x02,

        /// <summary>
        /// The real part of a complex property.
        /// </summary>
        Real = 0x010,

        /// <summary>
        /// The imaginary part of a complex property.
        /// </summary>
        Imaginary = 0x020,

        /// <summary>
        /// The amplitude of a complex property.
        /// </summary>
        Amplitude = 0x030,

        /// <summary>
        /// The decibels of a complex property.
        /// </summary>
        Decibels = 0x040,

        /// <summary>
        /// The phase of a complex property.
        /// </summary>
        Phase = 0x050,

        /// <summary>
        /// The property of a component.
        /// </summary>
        Property = 0x0100,

        /// <summary>
        /// The real part of a complex voltage VR(...).
        /// </summary>
        VoltageReal = Real | Voltage,

        /// <summary>
        /// The imaginary part of a complex voltage VI(...).
        /// </summary>
        VoltageImaginary = Imaginary | Voltage,

        /// <summary>
        /// The amplitude of a complex voltage VA(...).
        /// </summary>
        VoltageAmplitude = Amplitude | Voltage,

        /// <summary>
        /// The voltage decibels VDB(...).
        /// </summary>
        VoltageDecibels = Decibels | Voltage,

        /// <summary>
        /// The voltage phase VPH(...).
        /// </summary>
        VoltagePhase = Phase | Voltage,

        /// <summary>
        /// The real part of a complex current IR(...).
        /// </summary>
        CurrentReal = Real | Current,

        /// <summary>
        /// The imaginary part of a complex current II(...).
        /// </summary>
        CurrentImaginary = Imaginary | Current,

        /// <summary>
        /// The amplitude of a complex current IA(...).
        /// </summary>
        CurrentAmplitude = Amplitude | Current,

        /// <summary>
        /// The current decibels IDB(...).
        /// </summary>
        CurrentDecibels = Decibels | Current,

        /// <summary>
        /// The current phase IPH(...).
        /// </summary>
        CurrentPhase = Phase | Current,

        /// <summary>
        /// A mask that contains all flags that require a complex input.
        /// </summary>
        ComplexMask = Real | Imaginary | Amplitude | Decibels | Phase
    }
}
