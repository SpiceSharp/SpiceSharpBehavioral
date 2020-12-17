namespace SpiceSharpBehavioral.Parsers.Nodes
{
    /// <summary>
    /// Node types.
    /// </summary>
    /// <remarks>
    /// Node types.
    /// </remarks>
    public enum NodeTypes
    {
        /// <summary>
        /// Addition
        /// </summary>
        Add,

        /// <summary>
        /// Subtraction.
        /// </summary>
        Subtract,

        /// <summary>
        /// Multiplication.
        /// </summary>
        Multiply,

        /// <summary>
        /// Division.
        /// </summary>
        Divide,

        /// <summary>
        /// Remainder.
        /// </summary>
        Modulo,

        /// <summary>
        /// Unary plus.
        /// </summary>
        Plus,

        /// <summary>
        /// Unary minus.
        /// </summary>
        Minus,

        /// <summary>
        /// Logical not.
        /// </summary>
        Not,

        /// <summary>
        /// Conditional operator.
        /// </summary>
        Conditional,

        /// <summary>
        /// Conditional or.
        /// </summary>
        Or,

        /// <summary>
        /// Conditional and.
        /// </summary>
        And,

        /// <summary>
        /// Exclusive or.
        /// </summary>
        Xor,

        /// <summary>
        /// Equality operator.
        /// </summary>
        Equals,

        /// <summary>
        /// Inequality operator.
        /// </summary>
        NotEquals,

        /// <summary>
        /// Less than operator.
        /// </summary>
        LessThan,

        /// <summary>
        /// Greater than operator.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Less than or equal operator.
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// Greater than or equal operator.
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// Power operator.
        /// </summary>
        Pow,

        /// <summary>
        /// Constant value.
        /// </summary>
        Constant,

        /// <summary>
        /// A function that returns a value.
        /// </summary>
        Function,

        /// <summary>
        /// A variable.
        /// </summary>
        Variable,

        /// <summary>
        /// A voltage value.
        /// </summary>
        Voltage,

        /// <summary>
        /// A current value.
        /// </summary>
        Current,

        /// <summary>
        /// A property value.
        /// </summary>
        Property
    }
}
