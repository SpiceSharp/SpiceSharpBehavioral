using System;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// Interface for parsers that can also derive to variables.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISpiceDerivativeParser<T> : IParser<Derivatives<Func<T>>>
    {
        /// <summary>
        /// Event called when a Spice property has been found.
        /// </summary>
        event EventHandler<SpicePropertyFoundEventArgs<T>> SpicePropertyFound;
    }
}
