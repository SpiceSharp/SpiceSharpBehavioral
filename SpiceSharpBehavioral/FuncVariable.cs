using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using System;

namespace SpiceSharpBehavioral
{
    /// <summary>
    /// A variable that returns the value of a function.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    public class FuncVariable<T> : IVariable<T>
    {
        private readonly Func<T> _func;

        /// <summary>
        /// Gets the value.
        /// </summary>
        public T Value => _func();

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the unit.
        /// </summary>
        public IUnit Unit { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FuncVariable{T}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="func">The function.</param>
        /// <param name="unit">The unit.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="func"/> is <c>null</c>.</exception>
        public FuncVariable(string name, Func<T> func, IUnit unit = null)
        {
            Name = name.ThrowIfNull(nameof(name));
            _func = func.ThrowIfNull(nameof(func));
            Unit = unit;
        }
    }
}
