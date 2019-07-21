namespace SpiceSharpBehavioral.Parsers.Operators
{
    /// <summary>
    /// An arithmetic operator that listens to a default arithmetic precedence and associativity.
    /// </summary>
    /// <seealso cref="SpiceSharpBehavioral.Operator" />
    public class ArithmeticOperator : Operator
    {
        /// <summary>
        /// Gets the operator type.
        /// </summary>
        /// <value>
        /// The operator type.
        /// </value>
        public OperatorType Type { get; }

        // Private variables
        private readonly int _precedence;
        private readonly bool _leftAssociative;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArithmeticOperator"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="precedence">The precedence.</param>
        /// <param name="leftAssociative">if set to <c>true</c> [left associative].</param>
        public ArithmeticOperator(OperatorType type, int precedence, bool leftAssociative)
        {
            Type = type;
            _precedence = precedence;
            _leftAssociative = leftAssociative;
        }

        /// <summary>
        /// Determines whether this operator should be executed first
        /// assuming this operator comes after it.
        /// </summary>
        /// <param name="other">The other (previous) operator.</param>
        /// <returns></returns>
        public override bool HasPrecedenceOver(Operator other)
        {
            var result = true;

            // Cases where the other operator should be executed first
            if (other is ArithmeticOperator op)
            {
                // If the other operator has a higher precedence
                if (_precedence < op._precedence)
                    result = false;

                // Or if they have the same precedence and the current operator is left associative
                else if (_precedence == op._precedence && _leftAssociative)
                    result = false;
            }
            return result;
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
            // Should just say the same thing
            return !other.HasPrecedenceOver(this);
        }
    }
}
