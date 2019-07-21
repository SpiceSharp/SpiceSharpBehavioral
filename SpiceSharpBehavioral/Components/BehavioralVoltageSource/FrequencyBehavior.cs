using SpiceSharpBehavioral.Components.BehavioralBehaviors;
using SpiceSharp.Components;
using SpiceSharp.Algebra;
using System.Numerics;
using SpiceSharp;
using SpiceSharp.Simulations;

namespace SpiceSharpBehavioral.Components.BehavioralVoltageSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="BehavioralVoltageSource"/>.
    /// </summary>
    public class FrequencyBehavior : BehavioralFrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Nodes
        /// </summary>
        protected int PosNode { get; private set; }
        protected int NegNode { get; private set; }
        protected MatrixElement<Complex> PosBranchPtr { get; private set; }
        protected MatrixElement<Complex> NegBranchPtr { get; private set; }
        protected MatrixElement<Complex> BranchPosPtr { get; private set; }
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
        /// Connects the specified pins.
        /// </summary>
        /// <param name="pins">The pins.</param>
        public void Connect(params int[] pins)
        {
            pins.ThrowIfNot(nameof(pins), 2);
            PosNode = pins[0];
            NegNode = pins[1];
        }

        /// <summary>
        /// Gets the matrix pointers.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
            // First we copy the branch equation from the biasing behavior
            int branchEq = ((BiasingBehavior)BiasingBehavior).BranchEq;
            PosIndex = 0;
            NegIndex = branchEq;

            // Get matrix pointers
            PosBranchPtr = solver.GetMatrixElement(PosNode, branchEq);
            NegBranchPtr = solver.GetMatrixElement(NegNode, branchEq);
            BranchPosPtr = solver.GetMatrixElement(branchEq, PosNode);
            BranchNegPtr = solver.GetMatrixElement(branchEq, NegNode);

            base.GetEquationPointers(solver);
        }

        /// <summary>
        /// Loads the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void Load(FrequencySimulation simulation)
        {
            PosBranchPtr.Value += 1;
            BranchPosPtr.Value += 1;
            NegBranchPtr.Value -= 1;
            BranchNegPtr.Value -= 1;

            base.Load(simulation);
        }
    }
}
