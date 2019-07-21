using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers
{
    /// <summary>
    /// An expression parser that provides methods for parsing expressions
    /// using the Dijkstra's Shunting Yard algorithm. It can convert an
    /// expression in infix expression into Reverse Polish Notation (RPN).
    /// </summary>
    public abstract class ShuntingYard
    {
        /// <summary>
        /// Gets the current input expression.
        /// </summary>
        /// <value>
        /// The input.
        /// </value>
        protected string Input { get; private set; }

        /// <summary>
        /// Gets the character count for the current input expression.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        protected int Count { get; private set; }

        /// <summary>
        /// Gets or sets the index where the parser is currently parsing.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        protected int Index { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an infix or postfix operator can be expected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if an infix or postfix operator is expected; otherwise, <c>false</c>.
        /// </value>
        protected bool ExpectInfixPostfix { get; set; }

        /// <summary>
        /// Peeks the top-most operator.
        /// </summary>
        /// <value>
        /// The top-most operator.
        /// </value>
        protected Operator Top => _operators.Count > 0 ? _operators.Peek() : null;
        
        /// <summary>
        /// Private variables
        /// </summary>
        private readonly Stack<Operator> _operators = new Stack<Operator>();
        
        /// <summary>
        /// Parses the expression using the Shunting Yard algorithm.
        /// </summary>
        /// <param name="expression">The expression to be parsed.</param>
        /// <exception cref="ParserException">
        /// Infix or postfix operator expected
        /// or
        /// Cannot continue parsing
        /// </exception>
        protected virtual void ParseExpression(string expression)
        {
            // Initialize for parsing the expression
            Index = 0;
            Input = expression;
            Count = Input.Length;
            ExpectInfixPostfix = false;
            _operators.Clear();
            
            // Parse the expression
            while (Index < Count)
            {
                // Skip spaces
                while (Index < Count && Input[Index] == ' ')
                    Index++;

                // Parse a binary operator
                if (ExpectInfixPostfix)
                {
                    ExpectInfixPostfix = false;

                    // Test for infix and postfix operators
                    if (!ParseBinaryOperator())
                        throw new ParserException("Infix or postfix operator expected", Input, Index);
                }

                // Parse a unary operator or value
                else
                {
                    if (ParseUnaryOperator())
                        continue;
                    
                    if (ParseValue())
                    {
                        ExpectInfixPostfix = true;
                        continue;
                    }
                    
                    throw new ParserException("Could not parse new token", Input, Index);
                }
            }

            // Evaluate all that is left on the stack
            while (_operators.Count > 0)
                ExecuteOperator(_operators.Pop());
        }

        /// <summary>
        /// Adds the operator on the operator stack for later execution.
        /// </summary>
        /// <param name="op">The operator.</param>
        protected void PushOperator(Operator op)
        {
            while (_operators.Count > 0)
            {
                var o = _operators.Peek();

                // Let's see if the operator wants the other operator to execute first.
                if (!op.HasPrecedenceOver(o))
                {
                    // Let's see if this other operator wants to be executed first.
                    if (o.AllowPrecedence(op))
                    {
                        o = _operators.Pop();
                        ExecuteOperator(o);
                    }
                    else
                        break;
                }
                else
                    break;
            }

            // Prepare the operator
            PrepareOperator(op);
            _operators.Push(op);
        }

        /// <summary>
        /// Executes the last encountered operator.
        /// </summary>
        protected void PopAndExecuteOperator()
        {
            if (_operators.Count == 0)
                throw new ArgumentException("No operators to pop");
            ExecuteOperator(_operators.Pop());
        }

        /// <summary>
        /// Executes an operator in Reverse Polish Notation.
        /// </summary>
        /// <param name="op">The operator.</param>
        protected abstract void ExecuteOperator(Operator op);

        /// <summary>
        /// Prepare an operator as soon as it has been encountered in Infix Notation.
        /// </summary>
        /// <param name="op">The operator.</param>
        protected abstract void PrepareOperator(Operator op);

        /// <summary>
        /// Parses a unary operator at the current index. Moves that index if an operator has been found.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if a value could be parsed; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool ParseUnaryOperator();

        /// <summary>
        /// Parses a value at the current index. Moves that index if a value has been found.
        /// </summary>
        /// <remarks>
        /// A value is only detected if a unary operator could not be found. So be careful when
        /// parsing for example "+1.5". If the leading "+" is not recognized as a unary operator,
        /// you may want to treat it as part of a value instead.
        /// </remarks>
        /// <returns>
        ///     <c>true</c> if a value could be parsed; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool ParseValue();

        /// <summary>
        /// Parses a binary operator at the current index. Moves that index if an operator has been found.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if a value could be parsed; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool ParseBinaryOperator();
    }
}
