using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    /// <summary>
    /// A template for behavioral sources.
    /// </summary>
    public abstract class BehavioralBiasingBehavior : ExportingBehavior, IBiasingBehavior
    {
        protected static readonly PropertyInfo ItemProperty = typeof(Vector<double>).GetTypeInfo().GetProperty("Item");
        protected static readonly PropertyInfo MatrixValueProperty = typeof(MatrixElement<double>).GetTypeInfo().GetProperty("Value");
        protected static readonly PropertyInfo VectorValueProperty = typeof(VectorElement<double>).GetTypeInfo().GetProperty("Value");

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
        /// Private variables
        /// </summary>
        private Dictionary<string, SpiceSharp.Components.VoltageSourceBehaviors.BiasingBehavior> _voltageSourceBehaviors = 
            new Dictionary<string, SpiceSharp.Components.VoltageSourceBehaviors.BiasingBehavior>();
        private Vector<double> _currentSolution;
        private double _cumulated;
        private Action _fillMatrix;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralBiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        public BehavioralBiasingBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The setup data provider.</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            simulation.ThrowIfNull(nameof(simulation));
            provider.ThrowIfNull(nameof(provider));

            // Get parameters
            BaseParameters = provider.GetParameterSet<BaseParameters>();

            // This is our chance to find the voltage source behaviors
            var parser = new SimpleParser();
            parser.VariableFound += (sender, e) => e.Result = 0.0;
            parser.FunctionFound += (sender, e) => e.Result = 0.0;
            parser.SpicePropertyFound += (sender, e) =>
            {
                if (BaseParameters.SpicePropertyComparer.Equals(e.Property.Identifier, "I"))
                {
                    var source = e.Property[0];
                    _voltageSourceBehaviors.Add(source, simulation.EntityBehaviors[source].Get<SpiceSharp.Components.VoltageSourceBehaviors.BiasingBehavior>());
                }
                e.Apply(() => 0.0, 0, 0.0);
            };
            parser.Parse(BaseParameters.Expression);
        }

        /// <summary>
        /// Unsetup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void Unsetup(Simulation simulation)
        {
            Function = null;
        }

        /// <summary>
        /// Get all equation pointers.
        /// </summary>
        /// <param name="variables">The variables.</param>
        /// <param name="solver">The solver.</param>
        public virtual void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            var parser = BaseParameters.Parser ?? throw new CircuitException("No parser specified for {0}".FormatString(Name));
            var map = new Dictionary<int, int>();

            // We want to keep track of derivatives, and which column they map to
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
                    if (property.ArgumentCount != 1 && property.ArgumentCount != 2)
                        throw new CircuitException("Cannot parse V({0})".FormatString(string.Join(",", property)));
                    var index = variables.MapNode(property[0]).Index;
                    var refindex = property.ArgumentCount > 1 ? variables.MapNode(property[1]).Index : 0;
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
                    if (property.ArgumentCount != 1)
                        throw new CircuitException("Cannot parse I({0})".FormatString(string.Join(",", property)));

                    // Get the voltage behavior to find the branch equation
                    var index = _voltageSourceBehaviors[property[0]].BranchEq;
                    e.Apply(() => _currentSolution[index], Derivative(index), 1.0);
                }
            };
            parser.SpicePropertyFound += SpicePropertyFound;
            var parsedResult = parser.Parse(BaseParameters.Expression);
            parser.SpicePropertyFound -= SpicePropertyFound;
            Function = new BehavioralFunction(map, parsedResult);

            // Build the total method
            BuildFunctionMethod(solver);
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
                _contributions.Add(() => _cumulated = func());
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
        /// Creates an expression that returns the vector element at the specified index.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private Expression Solution(Expression vector, int index) => Expression.MakeIndex(vector, ItemProperty, new[] { Expression.Constant(index) });

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public virtual void Load(BaseSimulation simulation)
        {
            // We compiled all instructions into one big function, so we can make this simple!
            _currentSolution = simulation.RealState.Solution;
            _fillMatrix?.Invoke();
        }

        /// <summary>
        /// Tests convergence.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <returns></returns>
        public virtual bool IsConvergent(BaseSimulation simulation) => true;
    }
}
