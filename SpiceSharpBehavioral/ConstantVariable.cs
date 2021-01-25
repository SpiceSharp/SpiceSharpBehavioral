using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;

namespace SpiceSharpBehavioral
{
    /// <summary>
    /// A constant variable.
    /// </summary>
    public class ConstantVariable<T> : IVariable<T>
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the units.
        /// </summary>
        public IUnit Unit { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantVariable{T}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="unit">The unit.</param>
        public ConstantVariable(string name, T value, IUnit unit)
        {
            Name = name;
            Value = value;
            Unit = unit;
        }
    }
}
