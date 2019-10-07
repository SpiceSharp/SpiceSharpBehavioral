using System;

namespace SpiceSharpBehavioral.Parsers.Operators
{
    /// <summary>
    /// Ternary operator.
    /// </summary>
    /// <seealso cref="SpiceSharpBehavioral.Parsers.Operators.ArithmeticOperator" />
    public class TernaryOperator : ArithmeticOperator
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="TernaryOperator"/> is closed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if closed; otherwise, <c>false</c>.
        /// </value>
        public bool Closed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TernaryOperator"/> class.
        /// </summary>
        public TernaryOperator()
            : base(OperatorType.OpenTernary, (int) OperatorPrecedence.Conditional, false)
        {
            Closed = false;
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            if (Closed)
                throw new Exception("Ternary operator is already closed.");
            Closed = true;
        }

        /// <summary>
        /// Allows another operator to cause this
        /// one to execute first even though the other one comes
        /// after it.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public override bool AllowPrecedence(Operator other)
        {
            // If the ternary operator is closed properly, allow passing through
            return Closed;
        }
    }
}