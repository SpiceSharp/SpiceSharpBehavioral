using SpiceSharpBehavioral.Parsers;
using System.Collections.Generic;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// A builder for parsing expressions.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public interface IBuilder<T>
    {
        /// <summary>
        /// Adds two operands together.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The sum of the two operands.</returns>
        T Add(T left, T right);

        /// <summary>
        /// Subtracts the right operand from the left operand..
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The difference between the two operands.</returns>
        T Subtract(T left, T right);

        /// <summary>
        /// Multiplies two operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The two operands.</returns>
        T Multiply(T left, T right);

        /// <summary>
        /// Divides the left operand by the right operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the division.</returns>
        T Divide(T left, T right);

        /// <summary>
        /// Calculates the remainder of the division of the two operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The remainder of the division.</returns>
        T Mod(T left, T right);

        /// <summary>
        /// Takes the unary plus operation of the argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>The result of the unary plus operator.</returns>
        T Plus(T argument);

        /// <summary>
        /// Takes the unary minus operation of the argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>The result of the unary minus operator.</returns>
        T Minus(T argument);

        /// <summary>
        /// Conditional evaluation.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="ifTrue">If true.</param>
        /// <param name="ifFalse">If false.</param>
        /// <returns>The result.</returns>
        T Conditional(T condition, T ifTrue, T ifFalse);

        /// <summary>
        /// Combines two operands using or logic.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the or operation.</returns>
        T Or(T left, T right);

        /// <summary>
        /// Combines two operands using and logic.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the and operation.</returns>
        T And(T left, T right);

        /// <summary>
        /// Returns whether or not the operands are equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the equality operator.</returns>
        T Equals(T left, T right);

        /// <summary>
        /// Returns whether or not the operands are not equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the inequality operator.</returns>
        T NotEquals(T left, T right);

        /// <summary>
        /// Returns whether or not the left operand is less than the second operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the less than operator.</returns>
        T LessThan(T left, T right);

        /// <summary>
        /// Returns whether or not the left operand is greater than the second operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the greater than operator.</returns>
        T GreaterThan(T left, T right);

        /// <summary>
        /// Returns whether or not the left operand is less than or equal to the second operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the less than or equal operator.</returns>
        T LessThanOrEqual(T left, T right);

        /// <summary>
        /// Returns whether or not the left operand is greater than or equal to the second operand.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the greater than or equal operator.</returns>
        T GreaterThanOrEqual(T left, T right);

        /// <summary>
        /// Raises a number to a power.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The raised power.</returns>
        T Pow(T left, T right);

        /// <summary>
        /// Creates a number.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The number.</returns>
        T CreateNumber(string value);

        /// <summary>
        /// Creates the function.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The function value.
        /// </returns>
        T CreateFunction(string name, IReadOnlyList<T> arguments);

        /// <summary>
        /// Creates the value of a variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="type">The type of the variable.</param>
        /// <returns>
        /// The variable value.
        /// </returns>
        T CreateVariable(string name, QuantityTypes type = QuantityTypes.Raw);

        /// <summary>
        /// Creates the value of a voltage.
        /// </summary>
        /// <param name="node">The name of the node.</param>
        /// <param name="reference">The name of the reference node.</param>
        /// <param name="type">The type of voltage.</param>
        /// <returns>
        /// The value of the voltage.
        /// </returns>
        T CreateVoltage(string node, string reference = null, QuantityTypes type = QuantityTypes.Raw);

        /// <summary>
        /// Creates the value of a current.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="type">The type of current.</param>
        /// <returns>
        /// The value of the current.
        /// </returns>
        T CreateCurrent(string name, QuantityTypes type = QuantityTypes.Raw);

        /// <summary>
        /// Creates the value of a property
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="property">The name of the property.</param>
        /// <param name="type">The type of property.</param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        T CreateProperty(string name, string property, QuantityTypes type = QuantityTypes.Raw);
    }
}
