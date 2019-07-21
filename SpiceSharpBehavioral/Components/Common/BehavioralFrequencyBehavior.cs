using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Behaviors;
using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Algebra;
using System.Numerics;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharpBehavioral.Components.BehavioralBehaviors
{
    /// <summary>
    /// Frequency behavior for a behavioral component.
    /// </summary>
    public abstract class BehavioralFrequencyBehavior : ExportingBehavior, IFrequencyBehavior
    {
        private static readonly PropertyInfo MatrixValueProperty = typeof(MatrixElement<Complex>).GetTypeInfo().GetProperty("Value");

        /// <summary>
        /// Gets the biasing behavior.
        /// </summary>
        protected BehavioralBiasingBehavior BiasingBehavior { get; private set; }

        /// <summary>
        /// Gets or sets the row of positive contributions.
        /// </summary>
        protected int PosIndex { get; set; }

        /// <summary>
        /// Gets or sets the row of negative contributions.
        /// </summary>
        protected int NegIndex { get; set; }

        /// <summary>
        /// All contributions to the Y-matrix.
        /// </summary>
        private Complex[] _contributions;
        private Action _behavioralMethod;
        private Action<Vector<double>> _initializeMethod;

        /// <summary>
        /// Creates a new instance of the <see cref="BehavioralFrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name"></param>
        public BehavioralFrequencyBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The setup data provider.</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            provider.ThrowIfNull(nameof(provider));

            // Get behaviors
            BiasingBehavior = provider.GetBehavior<BehavioralBiasingBehavior>();
        }

        /// <summary>
        /// Unsetup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void Unsetup(Simulation simulation)
        {
            BiasingBehavior = null;
            _behavioralMethod = null;
            _contributions = null;
            _initializeMethod = null;
        }

        /// <summary>
        /// Get equation pointers.
        /// </summary>
        /// <param name="solver">Gets the equation pointers.</param>
        public virtual void GetEquationPointers(Solver<Complex> solver)
        {
            var block = new List<Expression>();
            var init_block = new List<Expression>();

            // If the contributions cancel each other, don't bother compiling
            if (PosIndex == NegIndex)
            {
                _initializeMethod = null;
                _behavioralMethod = null;
                return;
            }

            // The biasing behavior has a nice series of derivatives that need to be
            // calculated here. Since it is a small-signal analysis, we don't need
            // the actual value, only the derivatives.
            int index = 0;
            _contributions = new Complex[BiasingBehavior.Function.DerivativeCount];
            foreach (var item in BiasingBehavior.Function.Derivatives)
            {
                var contribution = Expression.ArrayAccess(Expression.Constant(_contributions), Expression.Constant(index, typeof(int)));
                if (PosIndex > 0)
                {
                    var melt = solver.GetMatrixElement(PosIndex, item.Key);
                    block.Add(Expression.AddAssign(Expression.Property(Expression.Constant(melt), MatrixValueProperty), contribution));
                }
                if (NegIndex > 0)
                {
                    var melt = solver.GetMatrixElement(NegIndex, item.Key);
                    block.Add(Expression.SubtractAssign(Expression.Property(Expression.Constant(melt), MatrixValueProperty), contribution));
                }

                // Create an initialization method
                init_block.Add(Expression.Assign(contribution, Expression.Convert(item.Value, typeof(Complex))));
                index++;
            }

            // Compile the methods
            _behavioralMethod = Expression.Lambda<Action>(Expression.Block(block)).Compile();
            _initializeMethod = Expression.Lambda<Action<Vector<double>>>(Expression.Block(init_block), BiasingBehavior.Function.Solution).Compile();
        }

        /// <summary>
        /// Initialize small-signal parameters.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public void InitializeParameters(FrequencySimulation simulation)
        {
            var solution = simulation.RealState.Solution;
            _initializeMethod?.Invoke(solution);
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public virtual void Load(FrequencySimulation simulation)
        {
            _behavioralMethod?.Invoke();
        }
    }
}
