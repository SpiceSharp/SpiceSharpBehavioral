using System;
using System.Linq.Expressions;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Keeps track of derivatives
    /// </summary>
    public abstract class Derivatives<T> : IEquatable<Derivatives<T>>
    {
        // The derivatives
        private T[] _derivatives;

        /// <summary>
        /// Gets the number of derivatives.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get; private set; }

        /// <summary>
        /// Gets or sets the derivative at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="Expression"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// Invalid index
        /// or
        /// Invalid index
        /// </exception>
        public T this[int index]
        {
            get
            {
                if (index < 0)
                    throw new ArgumentException("Invalid index");
                if (index >= Count)
                    return default(T);
                return _derivatives[index];
            }
            set
            {
                if (index < 0)
                    throw new ArgumentException("Invalid index");
                if (index >= Count)
                    Expand(index + 1);
                Count = Math.Max(index + 1, Count);
                _derivatives[index] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Derivatives"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public Derivatives(int capacity)
        {
            _derivatives = new T[capacity];
            Count = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Derivatives"/> class.
        /// </summary>
        public Derivatives()
            : this(4)
        {
            // Number 4 can be whatever...
        }

        /// <summary>
        /// Expands the specified needed.
        /// </summary>
        /// <param name="needed">The needed.</param>
        private void Expand(int needed)
        {
            if (needed < _derivatives.Length)
                return;

            int allocSize = Math.Max((int) (_derivatives.Length * 1.4f), needed);
            Array.Resize(ref _derivatives, allocSize);
        }

        /// <summary>
        /// Check equality.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns></returns>
        public bool Equals(Derivatives<T> obj)
        {
            if (obj.Count != Count)
                return false;
            for (var i = 0; i < Count; i++)
            {
                if (!this[i].Equals(obj[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check equality.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is DoubleDerivatives dd)
                return Equals(dd);
            return false;
        }

        /// <summary>
        /// Gets a hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var hash = 1;
            for (var i = 0; i < Count; i++)
                hash = (hash * 13) ^ this[i].GetHashCode();
            return hash;
        }

        /// <summary>
        /// Convert to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = "";
            for (var i = 0; i < Count; i++)
                result += _derivatives[i].ToString() + "; ";
            return result;
        }

        public abstract Derivatives<T> Negate();

        public abstract Derivatives<T> Not();

        public abstract Derivatives<T> Pow(Derivatives<T> b);

        public abstract Derivatives<T> Or(Derivatives<T> b);

        public abstract Derivatives<T> And(Derivatives<T> b);

        public abstract Derivatives<T> IfThenElse(Derivatives<T> iftrue, Derivatives<T> iffalse);

        public abstract Derivatives<T> Equal(Derivatives<T> b);

        public abstract Derivatives<T> NotEqual(Derivatives<T> b);

        public abstract Derivatives<T> Add(Derivatives<T> b);

        public abstract Derivatives<T> Subtract(Derivatives<T> b);

        public abstract Derivatives<T> Multiply(Derivatives<T> b);

        public abstract Derivatives<T> Divide(Derivatives<T> b);

        public abstract Derivatives<T> Modulo(Derivatives<T> b);

        public abstract Derivatives<T> LessThan(Derivatives<T> b);

        public abstract Derivatives<T> GreaterThan(Derivatives<T> b);

        public abstract Derivatives<T> LessOrEqual(Derivatives<T> b);

        public abstract Derivatives<T> GreaterOrEqual(Derivatives<T> b);
    }
}
