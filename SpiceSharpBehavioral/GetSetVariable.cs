using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using System;

namespace SpiceSharpBehavioral
{
    /// <summary>
    /// An <see cref="IVariable{T}"/> that just has a value that has read-write access.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IVariable{T}" />
    public class GetSetVariable<T> : IVariable<T> where T : IFormattable
    {
        /// <summary>
        /// Gets or sets the value of the variable.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value { get; set; }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>
        /// The name of the variable.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the units of the quantity.
        /// </summary>
        /// <value>
        /// The units.
        /// </value>
        public IUnit Unit { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSetVariable{T}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="unit">The unit.</param>
        public GetSetVariable(string name, IUnit unit)
        {
            Name = name;
            Unit = unit;
        }
    }
}
