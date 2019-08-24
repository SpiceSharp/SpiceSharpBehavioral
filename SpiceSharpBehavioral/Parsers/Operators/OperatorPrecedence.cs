namespace SpiceSharpBehavioral.Parsers.Operators
{
    /// <summary>
    /// The default operator precedences
    /// </summary>
    public enum OperatorPrecedence
    {
        /// <summary>
        /// The conditional operator precedence.
        /// </summary>
        Conditional,

        /// <summary>
        /// The conditional or operator precedence.
        /// </summary>
        ConditionalOr,

        /// <summary>
        /// The conditional and operator precedence.
        /// </summary>
        ConditionalAnd,

        /// <summary>
        /// The logical or operator precedence.
        /// </summary>
        LogicalOr,

        /// <summary>
        /// The logical xor operator precedence.
        /// </summary>
        LogicalXor,

        /// <summary>
        /// The logical and operator precedence.
        /// </summary>
        LogicalAnd,

        /// <summary>
        /// The equality operator precedence.
        /// </summary>
        Equality,

        /// <summary>
        /// The relational operator precedence.
        /// </summary>
        Relational,

        /// <summary>
        /// The shift operator precedence.
        /// </summary>
        Shift,

        /// <summary>
        /// The additive operator precedence.
        /// </summary>
        Additive,

        /// <summary>
        /// The multiplicative operator precedence.
        /// </summary>
        Multiplicative,

        /// <summary>
        /// The unary operator precedence.
        /// </summary>
        Unary,

        /// <summary>
        /// The primary operator precedence.
        /// </summary>
        Primary,

        /// <summary>
        /// The power operator precedence.
        /// </summary>
        Power
    }
}
