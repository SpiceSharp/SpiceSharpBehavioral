using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Builders.Functions;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SpiceSharp.Components.BehavioralSources
{
    /// <summary>
    /// Base parameters for a behavioral component.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public partial class Parameters : ParameterSet<Parameters>
    {
        private readonly NodeFinder _nodeFinder = new NodeFinder();
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
        /// Gets the voltage nodes.
        /// </summary>
        /// <value>
        /// The voltage nodes.
        /// </value>
        public IEnumerable<string> VoltageNodes => _nodeFinder.VoltageNodes(Function).Select(n => n.Name);

        /// <summary>
        /// Gets the current nodes.
        /// </summary>
        /// <value>
        /// The current nodes.
        /// </value>
        public IEnumerable<string> CurrentNodes => _nodeFinder.CurrentNodes(Function).Select(n => n.Name);

        /// <summary>
        /// Gets all variable nodes.
        /// </summary>
        /// <value>
        /// The variable nodes.
        /// </value>
        public IEnumerable<VariableNode> VariableNodes => _nodeFinder.Build(Function);

        /// <summary>
        /// Gets or sets the variable comparer.
        /// </summary>
        /// <value>
        /// The variable comparer.
        /// </value>
        public IEqualityComparer<string> VariableComparer { get; set; } = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parameters"/> class.
        /// </summary>
        public Parameters()
        {
            RealBuilderCreated += BuilderHelper.RegisterDefaultBuilder;
            ComplexBuilderCreated += BuilderHelper.RegisterDefaultBuilder;
        }

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
