using SpiceSharp.Components.BehavioralComponents;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A behavioral voltage source.
    /// </summary>
    /// <seealso cref="BehavioralComponent" />
    public class BehavioralVoltageSource : BehavioralComponent
    {
        /// <summary>
        /// The behavioral voltage source base pin count
        /// </summary>
        public const int BehavioralVoltageSourcePinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        public BehavioralVoltageSource(string name)
            : base(name, BehavioralVoltageSourcePinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="expression">The expression.</param>
        public BehavioralVoltageSource(string name, string pos, string neg, string expression)
            : this(name)
        {
            Connect(pos, neg);
            Parameters.Expression = expression;
        }
    }
}
