using SpiceSharp;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.ShuntingYard
{
    /// <summary>
    /// Event arguments that are used when a function has been found.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <seealso cref="EventArgs" />
    public class FunctionFoundEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        /// <value>
        /// The function name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        public IReadOnlyList<T> Arguments { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the function was found.
        /// </summary>
        /// <value>
        ///   <c>true</c> if found; otherwise, <c>false</c>.
        /// </value>
        public bool Found { get; set; }

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
                Found = true;
                _result = value;
            }
        }
        private T _result;

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
