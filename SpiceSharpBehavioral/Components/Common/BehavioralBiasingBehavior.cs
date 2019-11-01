using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    /// <summary>
    /// A template for behavioral sources.
    /// </summary>
    public abstract class BehavioralBiasingBehavior : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Gets the current evaluated value.
        /// </summary>
        protected double CurrentValue { get; private set; }

        /// <summary>
        /// Gets the parsed expressions.
        /// </summary>
        public BehavioralFunction Function { get; private set; }

        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        public BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets or sets the row of positive contributions.
        /// </summary>
        protected int PosIndex { get; set; }

        /// <summary>
        /// Gets or sets the row of negative contributions.
        /// </summary>
        protected int NegIndex { get; set; }

        /// <summary>
        /// Gets the simulation state.
        /// </summary>
        protected BaseSimulationState State { get; private set; }

        /// <summary>
        /// Private variables
        /// </summary>
        private Vector<double> _currentSolution;
        private double _cumulated;
        private Action _fillMatrix;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralBiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        public BehavioralBiasingBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The binding context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            // Get parameters
            BaseParameters = context.GetParameterSet<BaseParameters>();

            // We are now going to parse the expression
            var parser = BaseParameters.Parser.ThrowIfNull("Parser")(simulation);
            var variables = simulation.Variables;

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
                if (BaseParameters.SpicePropertyComparer.Equals(property.Identifier, "V"))
                {
                    // Only recognize V(a) or V(a, b)
                    if (property.ArgumentCount != 1 && property.ArgumentCount != 2)
                        return;

                    // Get the nodes
                    int index, refindex;
                    if (BaseParameters.Instance != null && BaseParameters.Instance is ComponentInstanceData cid)
                    {
                        index = variables.MapNode(cid.GenerateNodeName(property[0])).Index;
                        refindex = property.ArgumentCount > 1 ? variables.MapNode(cid.GenerateNodeName(property[1])).Index : 0;
                    }
                    else
                    {
                        index = variables.MapNode(property[0]).Index;
                        refindex = property.ArgumentCount > 1 ? variables.MapNode(property[1]).Index : 0;
                    }

                    if (index != refindex)
                    {
                        if (index > 0 && refindex > 0)
                        {
                            e.Apply(null, Derivative(index), 1.0);
                            e.Apply(() => _currentSolution[index] - _currentSolution[refindex], Derivative(refindex), -1.0);
                        }
                        else if (index > 0)
                            e.Apply(() => _currentSolution[index], Derivative(index), 1.0);
                        else if (refindex > 0)
                            e.Apply(() => -_currentSolution[refindex], Derivative(refindex), -1.0);
                    }
                }
                else if (BaseParameters.SpicePropertyComparer.Equals(property.Identifier, "I"))
                {
                    // Only recognized I(xxx)
                    if (property.ArgumentCount != 1)
                        return;

                    var component = property[0];
                    if (BaseParameters.Instance != null && BaseParameters.Instance is ComponentInstanceData cid)
                        component = cid.GenerateIdentifier(component);

                    // Get the voltage behavior to find the branch equation
                    if (simulation.EntityBehaviors.TryGetBehaviors(component, out var ebd))
                    {
                        // Check for voltage source behaviors
                        if (ebd.TryGet<SpiceSharp.Components.VoltageSourceBehaviors.BiasingBehavior>(out var vsrcb))
                        {
                            int index = vsrcb.BranchEq;
                            e.Apply(() => _currentSolution[index], Derivative(index), 1.0);
                        }
                        else if (ebd.TryGet<SpiceSharp.Components.CurrentSourceBehaviors.BiasingBehavior>(out var isrcb))
                            e.Apply(() => isrcb.Current);
                    }
                }
            };
            parser.SpicePropertyFound += SpicePropertyFound;
            var parsedResult = parser.Parse(BaseParameters.Expression);
            parser.SpicePropertyFound -= SpicePropertyFound;
            Function = new BehavioralFunction(map, parsedResult);

            // Build the total method
            State = ((BaseSimulation)simulation).RealState;
            var solver = State.Solver;
            BuildFunctionMethod(solver);
        }

        /// <summary>
        /// Unsetup the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            BaseParameters = null;
            Function = null;
            State = null;
            _currentSolution = null;
        }

        /// <summary>
        /// Builds the total method to load the Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="solver">The solver that provides the matrix and vector elements.</param>
        private void BuildFunctionMethod(Solver<double> solver)
        {
            solver.ThrowIfNull(nameof(solver));

            // If the contributions cancel each other out anyway, don't bother compiling
            if (PosIndex == NegIndex)
                return;

            // Initialize contributions
            List<Action> _contributions = new List<Action>();
            {
                var func = Function.Value;
                _contributions.Add(() =>
                {
                    _cumulated = func();
                    CurrentValue = _cumulated;
                });
            }

            // Fill Y-matrix
            foreach (var item in Function.Derivatives)
            {
                var index = item.Key;
                var func = item.Value;

                // Ignore 0-contributions
                if (func == null)
                    continue;

                MatrixElement<double> posElt = null, negElt = null;
                if (PosIndex > 0)
                    posElt = solver.GetMatrixElement(PosIndex, index);
                if (NegIndex > 0)
                    negElt = solver.GetMatrixElement(NegIndex, index);

                if (posElt != null && negElt != null)
                {
                    _contributions.Add(() =>
                    {
                        var derivative = func();
                        posElt.Value += derivative;
                        negElt.Value -= derivative;
                        _cumulated -= _currentSolution[index] * derivative;
                    });
                }
                else if (posElt != null)
                {
                    _contributions.Add(() =>
                    {
                        var derivative = func();
                        posElt.Value += derivative;
                        _cumulated -= _currentSolution[index] * derivative;
                    });
                }
                else if (negElt != null)
                {
                    _contributions.Add(() =>
                    {
                        var derivative = func();
                        negElt.Value -= derivative;
                        _cumulated -= _currentSolution[index] * derivative;
                    });
                }
            }

            // Fill Rhs-vectors
            VectorElement<double> posRhs = null, negRhs = null;
            if (PosIndex > 0)
                posRhs = solver.GetRhsElement(PosIndex);
            if (NegIndex > 0)
                negRhs = solver.GetRhsElement(NegIndex);
            if (posRhs != null && negRhs != null)
                _contributions.Add(() =>
                {
                    posRhs.Value -= _cumulated;
                    negRhs.Value += _cumulated;
                });
            else if (posRhs != null)
                _contributions.Add(() => posRhs.Value -= _cumulated);
            else if (negRhs != null)
                _contributions.Add(() => negRhs.Value += _cumulated);

            // Build the total behavioral method, ignore if nothing has been done
            if (_contributions.Count == 0)
                _fillMatrix = null;
            else
            {
                var actions = _contributions.ToArray();
                _fillMatrix = () =>
                {
                    for (var i = 0; i < actions.Length; i++)
                        actions[i]();
                };
            }
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        public virtual void Load()
        {
            // We compiled all instructions into one big function, so we can make this simple!
            _currentSolution = State.Solution;
            _fillMatrix?.Invoke();
        }

        /// <summary>
        /// Tests convergence.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsConvergent() => true;
    }
}
