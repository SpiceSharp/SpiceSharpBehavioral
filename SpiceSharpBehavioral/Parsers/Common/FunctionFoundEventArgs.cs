using SpiceSharp;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Event arguments used when a function is found.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    public class FunctionFoundEventArgs<T> : EventArgs, IEnumerable<T>
    {
        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the argument count.
        /// </summary>
        public int ArgumentCount => _arguments.Length;

        /// <summary>
        /// Gets an argument.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _arguments.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _arguments[index];
            }
        }
        private T[] _arguments;

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
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
        /// Gets or sets whether the result has been assigned or not.
        /// </summary>
        public bool Found { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="arguments">The arguments of the function.</param>
        public FunctionFoundEventArgs(string name, T defaultValue, params T[] arguments)
        {
            Name = name.ThrowIfNull(nameof(name));
            _arguments = arguments.ThrowIfNull(nameof(arguments));
            _result = defaultValue;
            Found = false;
        }

        /// <summary>
        /// Enumerate all arguments.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator() => (IEnumerator<T>) _arguments.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _arguments.GetEnumerator();
    }
}