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
        /// Initializes a new instance of the <see cref="Derivatives{T}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public Derivatives(int capacity)
        {
            _derivatives = new T[capacity];
            Count = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Derivatives{T}"/> class.
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

        /// <summary>
        /// Negate the derivatives.
        /// </summary>
        /// <returns>The derivatives.</returns>
        public abstract Derivatives<T> Negate();

        /// <summary>
        /// Not (binary) the derivatives.
        /// </summary>
        /// <returns>The derivatives.</returns>
        public abstract Derivatives<T> Not();

        /// <summary>
        /// Raises the derivatives to a power.
        /// </summary>
        /// <param name="exponent">The exponent.</param>
        /// <returns>The raised power.</returns>
        public abstract Derivatives<T> Pow(Derivatives<T> exponent);

        /// <summary>
        /// Or the derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>The derivatives.</returns>
        public abstract Derivatives<T> Or(Derivatives<T> b);

        /// <summary>
        /// And the derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>The derivatives.</returns>
        public abstract Derivatives<T> And(Derivatives<T> b);

        /// <summary>
        /// Conditional derivatives.
        /// </summary>
        /// <param name="iftrue">Argument if true.</param>
        /// <param name="iffalse">Argument if false.</param>
        /// <returns>The derivatives.</returns>
        public abstract Derivatives<T> IfThenElse(Derivatives<T> iftrue, Derivatives<T> iffalse);

        /// <summary>
        /// Check for equality.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>A value representing true if equal.</returns>
        public abstract Derivatives<T> Equal(Derivatives<T> b);

        /// <summary>
        /// Check for inequality.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>A value representing true if not equal.</returns>
        public abstract Derivatives<T> NotEqual(Derivatives<T> b);

        /// <summary>
        /// Add derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>The sum.</returns>
        public abstract Derivatives<T> Add(Derivatives<T> b);

        /// <summary>
        /// Subtract derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>The difference.</returns>
        public abstract Derivatives<T> Subtract(Derivatives<T> b);

        /// <summary>
        /// Multiply derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>The multiplied result.</returns>
        public abstract Derivatives<T> Multiply(Derivatives<T> b);

        /// <summary>
        /// Divide derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>The divided result.</returns>
        public abstract Derivatives<T> Divide(Derivatives<T> b);

        /// <summary>
        /// Modulo operation on derivatives.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>The remainder of the division.</returns>
        public abstract Derivatives<T> Modulo(Derivatives<T> b);

        /// <summary>
        /// Check less than.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>A value representing true if this is less.</returns>
        public abstract Derivatives<T> LessThan(Derivatives<T> b);

        /// <summary>
        /// Check greater than.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>A value representing true if this is greater.</returns>
        public abstract Derivatives<T> GreaterThan(Derivatives<T> b);

        /// <summary>
        /// Check less or equal.
        /// </summary>
        /// <param name="b">The other operand.</param>
        /// <returns>A value representing true if this is less or equal.</returns>
        public abstract Derivatives<T> LessOrEqual(Derivatives<T> b);

        /// <summary>
        /// Check greater or equal.
        /// </summary>
        /// <param name="b">The operand.</param>
        /// <returns>A value representing true if this is greater or equal.</returns>
        public abstract Derivatives<T> GreaterOrEqual(Derivatives<T> b);
    }
}
