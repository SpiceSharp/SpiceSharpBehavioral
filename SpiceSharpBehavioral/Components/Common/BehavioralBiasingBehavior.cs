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
        private Action<Vector<double>> _behavioralMethod;

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
                e.Result = 0.0;
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
            _behavioralMethod = null;
        }

        /// <summary>
        /// Get all equation pointers.
        /// </summary>
        /// <param name="variables">The variables.</param>
        /// <param name="solver">The solver.</param>
        public virtual void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            var parser = BaseParameters.Parser ?? new ExpressionTreeDerivativeParser();
            var map = new Dictionary<int, int>();

            // We want to keep track of derivatives, and which column they map to
            var solArg = Expression.Parameter(typeof(Vector<double>));
            int Derivative(int index)
            {
                if (index <= 0)
                    return -1;
                if (!map.TryGetValue(index, out var d))
                    map.Add(index, d = map.Count + 1);
                return d;
            };

            // Catch the derivatives for Y-matrix loading
            void SpicePropertyFound(object sender, SpicePropertyFoundEventArgs<ExpressionTreeDerivatives> e)
            {
                var result = new ExpressionTreeDerivatives();

                var property = e.Property;
                if (BaseParameters.SpicePropertyComparer.Equals(property.Identifier, "V"))
                {
                    if (property.ArgumentCount != 1 && property.ArgumentCount != 2)
                        throw new CircuitException("Cannot parse V({0})".FormatString(string.Join(",", property)));
                    var index = variables.MapNode(property[0]).Index;
                    var refindex = property.ArgumentCount > 1 ? variables.MapNode(property[1]).Index : 0;
                    if (index != refindex)
                    { 
                        if (index > 0)
                        {
                            result[0] = Solution(solArg, index);
                            result[Derivative(index)] = Expression.Constant(1.0);
                        }
                        if (refindex > 0)
                        {
                            if (result[0] == null)
                                result[0] = Expression.Negate(Solution(solArg, index));
                            else
                                result[0] = Expression.Subtract(result[0], Solution(solArg, index));
                            result[Derivative(index)] = Expression.Constant(-1.0);
                        }
                    }
                    e.Result = result;
                }
                else if (BaseParameters.SpicePropertyComparer.Equals(property.Identifier, "I"))
                {
                    if (property.ArgumentCount != 1)
                        throw new CircuitException("Cannot parse I({0})".FormatString(string.Join(",", property)));

                    // Get the voltage behavior to find the branch equation
                    var index = _voltageSourceBehaviors[property[0]].BranchEq;
                    result[0] = Solution(solArg, index);
                    result[Derivative(index)] = Expression.Constant(1.0);
                    e.Result = result;
                }
            };
            parser.SpicePropertyFound += SpicePropertyFound;
            var parsedResult = parser.Parse(BaseParameters.Expression);
            parser.SpicePropertyFound -= SpicePropertyFound;
            Function = new BehavioralFunction(map, parsedResult, solArg);

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
            {
                _behavioralMethod = null;
                return;
            }

            // Let us now build a method that will fill matrix elements
            var cumulated = Expression.Variable(typeof(double), "cumulated");
            var derivative = Expression.Variable(typeof(double), "derivative");
            List<Expression> block = new List<Expression>
            {
                // cumulated = f(v1, v2, ..., vn)
                Expression.Assign(cumulated, Function.Value)
            };

            // Fill Y-matrix
            foreach (var item in Function.Derivatives)
            {
                // We want to first calculate the derivative
                // derivative = df/dv(i)
                block.Add(Expression.Assign(derivative, item.Value));

                // Get the positive contribution matrix element
                if (PosIndex > 0)
                {
                    var melt = solver.GetMatrixElement(PosIndex, item.Key);
                    block.Add(Expression.AddAssign(Expression.Property(Expression.Constant(melt), MatrixValueProperty), derivative));
                }
                if (NegIndex > 0)
                {
                    var melt = solver.GetMatrixElement(NegIndex, item.Key);
                    block.Add(Expression.SubtractAssign(Expression.Property(Expression.Constant(melt), MatrixValueProperty), derivative));
                }

                // Track the cumulated value
                if (PosIndex > 0 || NegIndex > 0)
                    block.Add(Expression.SubtractAssign(cumulated, Expression.Multiply(derivative, Solution(Function.Solution, item.Key))));
            }

            // Fill Rhs-vectors
            if (PosIndex > 0)
            {
                var rhselt = solver.GetRhsElement(PosIndex);
                block.Add(Expression.SubtractAssign(Expression.Property(Expression.Constant(rhselt), VectorValueProperty), cumulated));
            }
            if (NegIndex > 0)
            {
                var rhselt = solver.GetRhsElement(NegIndex);
                block.Add(Expression.AddAssign(Expression.Property(Expression.Constant(rhselt), VectorValueProperty), cumulated));
            }

            // Build the total behavioral method, ignore if nothing has been done
            if (block.Count == 0)
                _behavioralMethod = null;
            else
                _behavioralMethod = Expression.Lambda<Action<Vector<double>>>(Expression.Block(new[] { cumulated, derivative }, block), Function.Solution).Compile();
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
            var solution = simulation.RealState.Solution;
            _behavioralMethod?.Invoke(solution);
        }

        /// <summary>
        /// Tests convergence.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <returns></returns>
        public virtual bool IsConvergent(BaseSimulation simulation) => true;
    }
}
