using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Interface for storing and managing derivatives.
    /// </summary>
    /// <typeparam name="T">The return types of the variables.</typeparam>
    public interface IDerivatives
    {
        int Count { get; }
    }
}
