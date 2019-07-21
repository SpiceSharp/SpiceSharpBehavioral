namespace SpiceSharpBehavioral.Parsers.Operators
{
    /// <summary>
    /// The default operator precedences
    /// </summary>
    public enum OperatorPrecedence
    {
        Conditional,
        ConditionalOr,
        ConditionalAnd,
        LogicalOr,
        LogicalXor,
        LogicalAnd,
        Equality,
        Relational,
        Shift,
        Additive,
        Multiplicative,
        Unary,
        Primary,
        Power
    }
}
