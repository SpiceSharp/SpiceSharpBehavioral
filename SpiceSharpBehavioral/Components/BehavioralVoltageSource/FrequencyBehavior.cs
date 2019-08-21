using SpiceSharpBehavioral.Components.BehavioralBehaviors;
using SpiceSharp.Components;
using SpiceSharp.Algebra;
using System.Numerics;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharpBehavioral.Components.BehavioralVoltageSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="BehavioralVoltageSource"/>.
    /// </summary>
    public class FrequencyBehavior : BehavioralFrequencyBehavior
    {
        /// <summary>
        /// Gets the positive node index.
        /// </summary>
        protected int PosNode { get; private set; }

        /// <summary>
        /// Gets the negative node index.
        /// </summary>
        protected int NegNode { get; private set; }

        /// <summary>
        /// Gets the (positive, branch) element.
        /// </summary>
        protected MatrixElement<Complex> PosBranchPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, branch) element.
        /// </summary>
        protected MatrixElement<Complex> NegBranchPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, positive) element.
        /// </summary>
        protected MatrixElement<Complex> BranchPosPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, negative) element.
        /// </summary>
        protected MatrixElement<Complex> BranchNegPtr { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior</param>
        public FrequencyBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Binds the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            // Get node connections
            if (context is ComponentBindingContext cc)
            {
                PosNode = cc.Pins[0];
                NegNode = cc.Pins[1];
            }

            // First we copy the branch equation from the biasing behavior
            int branchEq = ((BiasingBehavior)BiasingBehavior).BranchEq;
            PosIndex = 0;
            NegIndex = branchEq;

            // Do other behavioral stuff
            base.Bind(simulation, context);

            // Get matrix pointers
            var solver = State.Solver;
            PosBranchPtr = solver.GetMatrixElement(PosNode, branchEq);
            NegBranchPtr = solver.GetMatrixElement(NegNode, branchEq);
            BranchPosPtr = solver.GetMatrixElement(branchEq, PosNode);
            BranchNegPtr = solver.GetMatrixElement(branchEq, NegNode);
        }

        /// <summary>
        /// Loads the specified simulation.
        /// </summary>
        public override void Load()
        {
            PosBranchPtr.Value += 1;
            BranchPosPtr.Value += 1;
            NegBranchPtr.Value -= 1;
            BranchNegPtr.Value -= 1;
            base.Load();
        }
    }
}
