﻿using SpiceSharp.Attributes;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.BehavioralComponents
{
    /// <summary>
    /// Base parameters for a behavioral component.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class BaseParameters : ParameterSet
    {
        private bool _isDirty;

        /// <summary>
        /// Gets the default builder for building expressions.
        /// </summary>
        /// <value>
        /// The default builder.
        /// </value>
        public static IBuilder<Func<double>> DefaultBuilderFactory(Dictionary<VariableNode, IVariable<double>> variables)
        {
            return new FunctionBuilder()
            {
                FunctionDefinitions = FunctionBuilderHelper.Defaults,
                Variables = variables
            };
        }

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
        private string _expression;

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
        private Node _function;

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
        private Func<string, Node> _parseAction = e => new Parser().Parse(e);

        /// <summary>
        /// Gets or sets the builder factory.
        /// </summary>
        /// <value>
        /// The builder factory.
        /// </value>
        public BuilderFactoryMethod BuilderFactory { get; set; } = DefaultBuilderFactory;

        /// <summary>
        /// A delegate for creating an <see cref="IBuilder{T}"/>.
        /// </summary>
        /// <param name="variables">The variables.</param>
        /// <returns>The value.</returns>
        public delegate IBuilder<Func<double>> BuilderFactoryMethod(Dictionary<VariableNode, IVariable<double>> variables);
    }
}