using SpiceSharp;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// Event arguments used when a function was found.
    /// </summary>
    /// <typeparam name="T">Base value type.</typeparam>
    /// <seealso cref="EventArgs" />
    public class FunctionFoundEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        /// <value>
        /// The name of the function.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the arguments of the function call.
        /// </summary>
        /// <value>
        /// The arguments of the function call.
        /// </value>
        public IReadOnlyList<T> Arguments { get; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public T Result
        {
            get => _result;
            set
            {
                _result = value;
                Found = true;
            }
        }
        private T _result;

        /// <summary>
        /// Gets a value indicating whether the function was found.
        /// </summary>
        /// <value>
        ///   <c>true</c> if found; otherwise, <c>false</c>.
        /// </value>
        public bool Found { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionFoundEventArgs{T}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="arguments">The arguments.</param>
        public FunctionFoundEventArgs(string name, IReadOnlyList<T> arguments)
        {
            Name = name.ThrowIfNull(nameof(name));
            Arguments = arguments.ThrowIfNull(nameof(arguments));
        }
    }
}
