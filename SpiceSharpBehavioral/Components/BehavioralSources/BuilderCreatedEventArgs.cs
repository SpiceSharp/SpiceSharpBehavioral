using SpiceSharpBehavioral.Builders.Functions;
using System;

namespace SpiceSharp.Components.BehavioralSources
{
    /// <summary>
    /// Event arguments for when a builder has been created.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public class BuilderCreatedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the context for which the builder is created.
        /// </summary>
        public IComponentBindingContext Context { get; }

        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>
        /// The builder.
        /// </value>
        public IFunctionBuilder<T> Builder { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderCreatedEventArgs{T}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="builder">The builder.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> or <paramref name="builder"/> is <c>null</c>.</exception>
        public BuilderCreatedEventArgs(IComponentBindingContext context, IFunctionBuilder<T> builder)
        {
            Context = context.ThrowIfNull(nameof(builder));
            Builder = builder.ThrowIfNull(nameof(builder));
        }
    }
}
