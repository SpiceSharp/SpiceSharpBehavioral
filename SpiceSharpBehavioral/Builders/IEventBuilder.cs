using System;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEventBuilder<T> : IBuilder<T>
    {
        /// <summary>
        /// Occurs when a variable was found.
        /// </summary>
        public event EventHandler<VariableFoundEventArgs<T>> VariableFound;

        /// <summary>
        /// Occurs when an unknown function was found.
        /// </summary>
        public event EventHandler<FunctionFoundEventArgs<T>> FunctionFound;

        /// <summary>
        /// Occurs when a voltage was found.
        /// </summary>
        public event EventHandler<VoltageFoundEventArgs<T>> VoltageFound;

        /// <summary>
        /// Occurs when a current was found.
        /// </summary>
        public event EventHandler<CurrentFoundEventArgs<T>> CurrentFound;
    }
}
