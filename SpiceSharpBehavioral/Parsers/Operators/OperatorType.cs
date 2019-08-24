namespace SpiceSharpBehavioral.Parsers.Operators
{
    /// <summary>
    /// Possible operator types
    /// </summary>
    public enum OperatorType
    {
        /// <summary>
        /// The positive operator.
        /// </summary>
        Positive,

        /// <summary>
        /// The negative operator.
        /// </summary>
        Negative,

        /// <summary>
        /// The not operator.
        /// </summary>
        Not,

        /// <summary>
        /// The addition operator.
        /// </summary>
        Add,

        /// <summary>
        /// The subtraction operator.
        /// </summary>
        Subtract,

        /// <summary>
        /// The multiplication operator.
        /// </summary>
        Multiply,

        /// <summary>
        /// The division operator.
        /// </summary>
        Divide,

        /// <summary>
        /// The modulo operator.
        /// </summary>
        Modulo,

        /// <summary>
        /// The power operator.
        /// </summary>
        Power,

        /// <summary>
        /// The equality operator.
        /// </summary>
        IsEqual,

        /// <summary>
        /// The inequality operator.
        /// </summary>
        IsNotEqual,

        /// <summary>
        /// The open ternary operator.
        /// </summary>
        OpenTernary,

        /// <summary>
        /// The close ternary operator.
        /// </summary>
        CloseTernary,

        /// <summary>
        /// The conditional or operator.
        /// </summary>
        ConditionalOr,

        /// <summary>
        /// The conditional and operator.
        /// </summary>
        ConditionalAnd,

        /// <summary>
        /// The less than operator.
        /// </summary>
        LessThan,

        /// <summary>
        /// The less than or equal operator.
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// The greater than operator.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// The greater than or equal operator.
        /// </summary>
        GreaterThanOrEqual
    }
}
