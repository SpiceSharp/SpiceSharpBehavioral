using System;
using System.Collections;
using System.Collections.Generic;
using SpiceSharp;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Event arguments for 
    /// </summary>
    public class SpiceProperty : IEnumerable<string>
    {
        /// <summary>
        /// The identifier of the type of export.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Gets the argument count.
        /// </summary>
        public int ArgumentCount => _arguments.Length;

        /// <summary>
        /// Gets an argument.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public string this[int index]
        {
            get
            {
                if (index < 0 || index >= _arguments.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _arguments[index];
            }
        }
        private string[] _arguments;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="identifier">The export identifier.</param>
        /// <param name="arguments">The arguments.</param>
        public SpiceProperty(string identifier, params string[] arguments)
        {
            Identifier = identifier;
            _arguments = arguments.ThrowIfNull(nameof(arguments));
        }

        /// <summary>
        /// Enumerate the arguments.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<string> GetEnumerator() => (IEnumerator<string>)_arguments.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _arguments.GetEnumerator();
    }
}
