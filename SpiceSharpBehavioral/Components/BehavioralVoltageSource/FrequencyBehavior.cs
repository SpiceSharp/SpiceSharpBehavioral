using SpiceSharpBehavioral.Components.BehavioralBehaviors;
using SpiceSharp.Components;
using System.Numerics;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharpBehavioral.Components.BehavioralVoltageSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="BehavioralVoltageSource"/>.
    /// </summary>
    public class FrequencyBehavior : BehavioralFrequencyBehavior
    {
        private readonly int _pos, _neg;
        private readonly ElementSet<Complex> _elements;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior</param>
        public FrequencyBehavior(string name, BehavioralBindingContext context)
            : base(name, context)
        {
            // Create a parser
            var bp = context.GetParameterSet<BaseParameters>();
            BiasingState = context.GetState<IBiasingSimulationState>();
            var parser = context.GetParameterSet<BaseParameters>().Parser.Create();
            var variables = context.Variables;

            // We want to keep track of derivatives, and which column they map to
            var map = new Dictionary<int, int>();
            int Derivative(int index)
            {
                if (index <= 0)
                    return -1;
                if (!map.TryGetValue(index, out var d))
                    map.Add(index, d = map.Count + 1);
                return d;
            };

            // Catch the derivatives for Y-matrix loading
            void SpicePropertyFound(object sender, SpicePropertyFoundEventArgs<double> e)
            {
                var property = e.Property;
                if (BaseParameters.PropertyComparer.Equals(property.Identifier, "V"))
                {
                    // Only recognize V(a) or V(a, b)
                    if (property.ArgumentCount != 1 && property.ArgumentCount != 2)
                        return;

                    // Get the nodes
                    var index = BiasingState.Map[variables.MapNode(property[0], VariableType.Voltage)];
                    var refIndex = property.ArgumentCount > 1 ? BiasingState.Map[variables.MapNode(property[1], VariableType.Voltage)] : 0;
                    if (index != refIndex)
                    {
                        if (index > 0 && refIndex > 0)
                        {
                            e.Apply(null, Derivative(index), 1.0);
                            e.Apply(() => BiasingState.Solution[index] - BiasingState.Solution[refIndex], Derivative(refIndex), -1.0);
                        }
                        else if (index > 0)
                            e.Apply(() => BiasingState.Solution[index], Derivative(index), 1.0);
                        else if (refIndex > 0)
                            e.Apply(() => -BiasingState.Solution[refIndex], Derivative(refIndex), -1.0);
                    }
                }
                else if (BaseParameters.PropertyComparer.Equals(property.Identifier, "I"))
                {
                    // Only recognized I(xxx)
                    if (property.ArgumentCount != 1)
                        return;

                    var component = property[0];
                    if (context.References.TryGetValue(property[0], out var container))
                    {
                        if (container.TryGetValue<IBranchedBehavior>(out var behavior))
                        {
                            int index = BiasingState.Map[behavior.Branch];
                            e.Apply(() => BiasingState.Solution[index], Derivative(index), 1.0);
                        }
                        else
                        {
                            var getter = container.CreatePropertyGetter<double>("i");
                            if (getter != null)
                                e.Apply(getter);
                        }
                    }
                }
            };
            parser.SpicePropertyFound += SpicePropertyFound;
            var parsedResult = parser.Parse(BaseParameters.Expression);
            parser.SpicePropertyFound -= SpicePropertyFound;
            Function = new BehavioralFunction(map, parsedResult);
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
            int branchEq = context.GetBehavior<BiasingBehavior>().BranchEq;
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
