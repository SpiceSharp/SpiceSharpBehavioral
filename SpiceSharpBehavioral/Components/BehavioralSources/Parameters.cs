using SpiceSharp.Attributes;
using SpiceSharp.Components;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.ParameterSets;
using SpiceSharpBehavioral.Builders.Functions;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Numerics;

namespace SpiceSharpBehavioral.Components
{
    /// <summary>
    /// Base parameters for any behavioral component.
    /// </summary>
    /// <typeparam name="P">The parameter type.</typeparam>
    public abstract class Parameters<P> : ParameterSet<P>
    {
        private string _expression;
        private Node _function;
        private bool _isDirty;
        private Func<string, Node> _parseAction = e => Parser.Parse(Lexer.FromString(e));

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>
        /// The expression.
        /// </value>
        [ParameterName("expression"), ParameterName("e"), ParameterInfo("The expression describing the component")]
        public string Expression
        {
            get => _expression;
            set
            {
                if (!ReferenceEquals(_expression, value))
                    _isDirty = true;
                _expression = value;
            }
        }

        /// <summary>
        /// Gets the function.
        /// </summary>
        /// <value>
        /// The function.
        /// </value>
        [ParameterName("node"), ParameterInfo("The node that represents the expression")]
        public Node Function
        {
            get
            {
                if (_isDirty)
                {
                    if (_parseAction == null || _expression == null)
                        _function = null;
                    else
                        _function = _parseAction(_expression);
                }
                return _function;
            }
        }

        /// <summary>
        /// Gets or sets the parse action.
        /// </summary>
        /// <value>
        /// The parse action.
        /// </value>
        public Func<string, Node> ParseAction
        {
            get => _parseAction;
            set
            {
                if (_parseAction != value)
                    _isDirty = true;
                _parseAction = value;
            }
        }

        /// <summary>
        /// Occurs when a builder has been created that uses real values.
        /// </summary>
        public event EventHandler<BuilderCreatedEventArgs<double>> RealBuilderCreated;

        /// <summary>
        /// Occurs when a builder has been created that uses complex values.
        /// </summary>
        public event EventHandler<BuilderCreatedEventArgs<Complex>> ComplexBuilderCreated;

        /// <summary>
        /// Registers a new function builder.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="builder">The builder.</param>
        public virtual void RegisterBuilder(IComponentBindingContext context, IFunctionBuilder<double> builder)
            => RealBuilderCreated?.Invoke(this, new BuilderCreatedEventArgs<double>(context, builder));

        /// <summary>
        /// Register a new function builder.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="builder">The builder.</param>
        public virtual void RegisterBuilder(IComponentBindingContext context, IFunctionBuilder<Complex> builder)
            => ComplexBuilderCreated?.Invoke(this, new BuilderCreatedEventArgs<Complex>(context, builder));
    }
}
