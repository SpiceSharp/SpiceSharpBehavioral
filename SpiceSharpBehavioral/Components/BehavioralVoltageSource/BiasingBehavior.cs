using System;
using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;

namespace SpiceSharpBehavioral.Components.BehavioralVoltageSourceBehaviors
{
    /// <summary>
    /// Biasing behavior for a behavioral voltage source.
    /// </summary>
    public class BiasingBehavior : BehavioralBiasingBehavior
    {
        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("i"), ParameterName("current"), ParameterInfo("Voltage source current")]
        public double GetCurrent() => State.ThrowIfNotBound(this).Solution[BranchEq];

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("p"), ParameterName("power"), ParameterInfo("Instantaneous power")]
        public double GetPower() => (State.ThrowIfNotBound(this).Solution[PosNode] - State.Solution[NegNode]) * -State.Solution[BranchEq];

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        /// <value>
        /// The voltage.
        /// </value>
        [ParameterName("v"), ParameterName("voltage"), ParameterInfo("Instantaneous voltage")]
        public double Voltage => CurrentValue;

        /// <summary>
        /// Gets the branch equation of the voltage source.
        /// </summary>
        public int BranchEq { get; private set; }

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
        protected MatrixElement<double> PosBranchPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, branch) element.
        /// </summary>
        protected MatrixElement<double> NegBranchPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, positive) element.
        /// </summary>
        protected MatrixElement<double> BranchPosPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, negative) element.
        /// </summary>
        protected MatrixElement<double> BranchNegPtr { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public BiasingBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            // Get the nodes
            if (context is ComponentBindingContext cc)
            {
                PosNode = cc.Pins[0];
                NegNode = cc.Pins[1];
            }

            // Create a new branch equation for the current through the voltage source
            var variables = simulation.Variables;
            BranchEq = variables.Create(Name.Combine("branch"), VariableType.Current).Index;
            
            // Note: Because the voltage source equation looks like "v1 - v2 = f(...)" this translates to
            // "v1 - v2 - f(...) = 0", so that means we need the negative sign here - hence NegIndex.
            PosIndex = 0;
            NegIndex = BranchEq;

            // Do other behavioral source stuff
            base.Bind(simulation, context);

            // Get matrix pointers
            var solver = State.Solver;
            PosBranchPtr = solver.GetMatrixElement(PosNode, BranchEq);
            NegBranchPtr = solver.GetMatrixElement(NegNode, BranchEq);
            BranchPosPtr = solver.GetMatrixElement(BranchEq, PosNode);
            BranchNegPtr = solver.GetMatrixElement(BranchEq, NegNode);
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
