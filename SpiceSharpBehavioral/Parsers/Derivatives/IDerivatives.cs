using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Interface for storing and managing derivatives.
    /// </summary>
    /// <typeparam name="T">The return types of the variables.</typeparam>
    public interface IDerivatives<T>
    {
        /// <summary>
        /// Create a function for calculating the derivative.
        /// </summary>
        /// <param name="index">The derivative index.</param>
        /// <returns></returns>
        Func<T> GetDerivative(int index);
    }
}
