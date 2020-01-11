using System;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Parsers.ShuntingYard
{
    /// <summary>
    /// Defines the implementation of the operators used by the <see cref="ShuntingYard{T}"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public interface IShuntingYardOperators<T>
    {
        /// <summary>
        /// Occurs when a function was found.
        /// </summary>
        event EventHandler<FunctionFoundEventArgs<T>> FunctionFound;

        /// <summary>
        /// Occurs when a variable was found.
        /// </summary>
        event EventHandler<VariableFoundEventArgs<T>> VariableFound;

        /// <summary>
        /// Applies a unary plus operator.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>The result.</returns>
        T UnaryPlus(T argument);

        /// <summary>
        /// Applies a unary minus operator.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>The result.</returns>
        T UnaryMinus(T argument);

        /// <summary>
        /// Applies a unary not operator.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>The result.</returns>
        T Not(T argument);

        /// <summary>
        /// Addition.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result.</returns>
        T Add(T left, T right);

        /// <summary>
        /// Subtraction.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result.</returns>
        T Subtract(T left, T right);

        /// <summary>
        /// Multiplication.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result.</returns>
        T Multiply(T left, T right);

        /// <summary>
        /// Division.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result.</returns>
        T Divide(T left, T right);

        /// <summary>
        /// Raising to a power.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result.</returns>
        T Pow(T left, T right);

        /// <summary>
        /// Logarithm.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>The result.</returns>
        T Log(T argument);

        /// <summary>
        /// Sign of the argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>The result.</returns>
        T Sign(T argument);

        /// <summary>
        /// Remainder.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result.</returns>
        T Modulo(T left, T right);

        /// <summary>
        /// Greater than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result.</returns>
        T Greater(T left, T right);

        /// <summary>
        /// Less than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result.</returns>
        T Less(T left, T right);

        /// <summary>
        /// Greater or equal than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result.</returns>
        T GreaterOrEqual(T left, T right);

        /// <summary>
        /// Less or equal than.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result.</returns>
        T LessOrEqual(T left, T right);

        /// <summary>
        /// Equality.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result.</returns>
        T Equal(T left, T right);

        /// <summary>
        /// Inequality.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result.</returns>
        T NotEqual(T left, T right);

        /// <summary>
        /// A conditional.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="ifTrue">If true.</param>
        /// <param name="ifFalse">If false.</param>
        /// <returns>The result.</returns>
        T IfThenElse(T condition, T ifTrue, T ifFalse);

        /// <summary>
        /// Creates a value.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="unit">The unit.</param>
        /// <returns>The value.</returns>
        T CreateValue(double input, string unit = "");

        /// <summary>
        /// Creates a variable value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The value.</returns>
        T CreateVariable(string name);

        /// <summary>
        /// Creates a function value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The function value.</returns>
        T CreateFunction(string name, IReadOnlyList<T> arguments);

        /// <summary>
        /// And.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result.</returns>
        T And(T left, T right);

        /// <summary>
        /// Or.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result.</returns>
        T Or(T left, T right);
    }
}
