using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers;
using System;

namespace SpiceSharp.Components.BehavioralComponents
{
    /// <summary>
    /// Describes a parser that can be used for parsing behavioral component descriptions.
    /// </summary>
    /// <seealso cref="IParser{T}" />
    public interface IBehavioralParser : IParser<Derivatives<Func<double>>>
    {
        /// <summary>
        /// Gets or sets the simulation.
        /// </summary>
        /// <value>
        /// The simulation.
        /// </value>
        ISimulation Simulation { set; }
    }
}
