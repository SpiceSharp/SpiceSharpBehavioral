using System;
using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;

namespace SpiceSharpBehavioral.Components.BehavioralVoltageSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a behavioral voltage source.
    /// </summary>
    public class BiasingBehavior : BehavioralBiasingBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("i"), ParameterName("current"), ParameterInfo("Voltage source current")]
        public double GetCurrent(BaseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return state.Solution[BranchEq];
        }

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("p"), ParameterName("power"), ParameterInfo("Instantaneous power")]
        public double GetPower(BaseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return (state.Solution[PosNode] - state.Solution[NegNode]) * -state.Solution[BranchEq];
        }

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        /// <value>
        /// The voltage.
        /// </value>
        [ParameterName("v"), ParameterName("voltage"), ParameterInfo("Instantaneous voltage")]
        public double Voltage { get; private set; }

        public int BranchEq { get; private set; }
        protected int PosNode { get; private set; }
        protected int NegNode { get; private set; }

        protected MatrixElement<double> PosBranchPtr { get; private set; }
        protected MatrixElement<double> NegBranchPtr { get; private set; }
        protected MatrixElement<double> BranchPosPtr { get; private set; }
        protected MatrixElement<double> BranchNegPtr { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public BiasingBehavior(string name) : base(name) { }

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
        /// Gets the equation pointers.
        /// </summary>
        /// <param name="variables">The variables.</param>
        /// <param name="solver">The solver.</param>
        public override void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            variables.ThrowIfNull(nameof(variables));
            solver.ThrowIfNull(nameof(solver));

            // Create a new branch equation for the current through the voltage source
            BranchEq = variables.Create(Name.Combine("branch"), VariableType.Current).Index;

            // Get matrix pointers
            PosBranchPtr = solver.GetMatrixElement(PosNode, BranchEq);
            NegBranchPtr = solver.GetMatrixElement(NegNode, BranchEq);
            BranchPosPtr = solver.GetMatrixElement(BranchEq, PosNode);
            BranchNegPtr = solver.GetMatrixElement(BranchEq, NegNode);

            // Note: Because the voltage source equation looks like "v1 - v2 = f(...)" this translates to
            // "v1 - v2 - f(...) = 0", so that means we need the negative sign here - hence NegIndex.
            PosIndex = 0;
            NegIndex = BranchEq;
            base.GetEquationPointers(variables, solver);
        }

        /// <summary>
        /// Loads the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void Load(BaseSimulation simulation)
        {
            PosBranchPtr.Value += 1;
            BranchPosPtr.Value += 1;
            NegBranchPtr.Value -= 1;
            BranchNegPtr.Value -= 1;

            base.Load(simulation);
        }
    }
}
