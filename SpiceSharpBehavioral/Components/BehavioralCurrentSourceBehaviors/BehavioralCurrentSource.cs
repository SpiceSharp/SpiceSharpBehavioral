using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;

namespace SpiceSharp.Components
{
    public class BehavioralCurrentSource : Component,
        IParameterized<BaseParameters>,
        IParameterized<IParserDescription>
    {
        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public BaseParameters Parameters { get; } = new BaseParameters();

        /// <summary>
        /// Gets the parser description.
        /// </summary>
        /// <value>
        /// The parser description.
        /// </value>
        public IParserDescription ParserDescription { get; }
        IParserDescription IParameterized<IParserDescription>.Parameters => ParserDescription;

        /// <summary>
        /// The behavioral current source pin count.
        /// </summary>
        public const int BIPinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        public BehavioralCurrentSource(string name)
            : base(name, BIPinCount)
        {
            ParserDescription = new ParserDescription();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The parser description.</param>
        public BehavioralCurrentSource(string name, IParserDescription description)
            : base(name, BIPinCount)
        {
            ParserDescription = description.ThrowIfNull(nameof(description));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="expression">The expression that describes the source.</param>
        public BehavioralCurrentSource(string name, string pos, string neg, string expression)
            : this(name)
        {
            Connect(pos, neg);
            Parameters.Expression = expression;
        }

        /// <summary>
        /// Creates the behaviors.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var container = new BehaviorContainer(Name);

            // We first need to make sure the referenced behaviors are created before
            // creating our own behaviors that reference them


            // var context = new BehavioralBindingContext(this, simulation);
        }
    }
}
