﻿using SpiceSharp.Attributes;
using SpiceSharp.Components.BehavioralSources;
using SpiceSharp.Validation;
using System.Linq;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A behavioral capacitor.
    /// </summary>
    /// <seealso cref="BehavioralComponent"/>
    [Pin(0, "P"), Pin(1, "N"), Connected, AutoGeneratedBehaviors]
    public partial class BehavioralCapacitor : BehavioralComponent, IRuleSubject
    {
        /// <summary>
        /// The behavioral capacitor base pin count.
        /// </summary>
        public const int BehavioralCapacitorPinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralCapacitor"/> class.
        /// </summary>
        /// <param name="name">The name of the entity</param>
        public BehavioralCapacitor(string name)
            : base(name, BehavioralCapacitorPinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralCapacitor"/> class.
        /// </summary>
        /// <param name="name">The name of the resistor.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="expression">The expression that describes the resistance.</param>
        public BehavioralCapacitor(string name, string pos, string neg, string expression)
            : this(name)
        {
            Connect(pos, neg);
            Parameters.Expression = expression;
        }

        /// <inheritdoc/>
        void IRuleSubject.Apply(IRules rules)
        {
            var p = rules.GetParameterSet<ComponentRuleParameters>();
            var nodes = Nodes.Select(name => p.Factory.GetSharedVariable(name)).ToArray();
            foreach (var rule in rules.GetRules<IConductiveRule>())
                rule.AddPath(this, ConductionTypes.Ac, nodes[0], nodes[1]);
        }
    }
}
