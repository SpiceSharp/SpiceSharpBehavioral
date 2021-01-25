using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A behavioral current source.
    /// </summary>
    /// <seealso cref="BehavioralComponent" />
    [Pin(0, "I+"), Pin(1, "I-"), Connected]
    public class BehavioralCurrentSource : BehavioralComponent
    {
        /// <summary>
        /// The behavioral current source base pin count
        /// </summary>
        public const int BehavioralCurrentSourcePinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        public BehavioralCurrentSource(string name)
            : base(name, BehavioralCurrentSourcePinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the source.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="expression">The expression that describes the current.</param>
        public BehavioralCurrentSource(string name, string pos, string neg, string expression)
            : this(name)
        {
            Connect(pos, neg);
            Parameters.Expression = expression;
        }
    }
}
