using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BehavioralComponents
{
    /// <summary>
    /// A branched behavior.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IBehavior" />
    /// <seealso cref="IBranchedBehavior" />
    public class BranchedBehavior : Behavior, IBehavior, IBranchedBehavior
    {
        /// <summary>
        /// Gets the branch equation variable.
        /// </summary>
        /// <value>
        /// The branch equation variable.
        /// </value>
        public Variable Branch { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchedBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="variables">The variables.</param>
        public BranchedBehavior(string name, IVariableSet variables)
            : base(name)
        {
            Branch = variables.Create(name.Combine("branch"), VariableType.Current);
        }
    }
}
