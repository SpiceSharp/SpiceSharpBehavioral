﻿using SpiceSharp.Attributes;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Validation;
using System.Linq;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A behavioral voltage source.
    /// </summary>
    /// <seealso cref="BehavioralComponent" />
    [Pin(0, "V+"), Pin(1, "V-"), VoltageDriver(0, 1), IndependentSource]
    public class BehavioralVoltageSource : BehavioralComponent, IRuleSubject
    {
        /// <summary>
        /// The behavioral voltage source base pin count
        /// </summary>
        public const int BehavioralVoltageSourcePinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        public BehavioralVoltageSource(string name)
            : base(name, BehavioralVoltageSourcePinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehavioralVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="expression">The expression.</param>
        public BehavioralVoltageSource(string name, string pos, string neg, string expression)
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
                rule.AddPath(this, nodes[0], nodes[1]);
            foreach (var rule in rules.GetRules<IAppliedVoltageRule>())
                rule.Fix(this, nodes[0], nodes[1]);
        }
    }
}
