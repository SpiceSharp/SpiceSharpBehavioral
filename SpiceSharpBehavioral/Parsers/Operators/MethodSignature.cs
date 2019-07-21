using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpiceSharpBehavioral.Parsers.Operators
{
    /// <summary>
    /// Represents a parameter signature for methods.
    /// </summary>
    public class MethodSignature
    {
        // The underlying types
        private readonly Type[] _types;

        /// <summary>
        /// Gets the <see cref="Type"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="Type"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Index out of bounds</exception>
        public Type this[int index]
        {
            get
            {
                if (index < 0 || index >= _types.Length)
                    throw new ArgumentException("Index out of bounds");
                return _types[index];
            }
        }

        /// <summary>
        /// Gets the number of types in the signature.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Count => _types.Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodSignature"/> class.
        /// </summary>
        /// <param name="types">The types.</param>
        public MethodSignature(params Type[] types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            _types = (Type[]) types.Clone();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodSignature"/> class.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <exception cref="ArgumentNullException">types</exception>
        public MethodSignature(IEnumerable<Type> types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            _types = types.ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodSignature"/> class.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <exception cref="ArgumentNullException">method</exception>
        public MethodSignature(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            var parameters = method.GetParameters();
            _types = new Type[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
                _types[i] = parameters[i].ParameterType;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return _types.Aggregate(0, (current, t) => (current * 13) ^ t.GetHashCode());
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            if (obj is MethodSignature sig)
            {
                if (_types.Length != sig._types.Length)
                    return false;
                if (_types.Where((t, i) => t != sig._types[i]).Any())
                    return false;
                return true;
            }

            return false;
        }
    }
}
