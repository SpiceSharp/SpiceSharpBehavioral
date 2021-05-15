using SpiceSharp;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SpiceSharpBehavioral.Builders.Functions
{
    /// <summary>
    /// Describes a state for writing IL code.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public interface IILState<T>
    {
        /// <summary>
        /// Gets the builder creating the method.
        /// </summary>
        /// <value>The builder.</value>
        IFunctionBuilder<T> Builder { get; }

        /// <summary>
        /// Gets the IL generator.
        /// </summary>
        /// <value>The IL generator.</value>
        ILGenerator Generator { get; }

        /// <summary>
        /// Pushes a node on the stack.
        /// </summary>
        /// <param name="node">The node.</param>
        void Push(Node node);

        /// <summary>
        /// Pushes a value on the stack.
        /// </summary>
        /// <param name="value">The value.</param>
        void Push(T value);

        /// <summary>
        /// Pushes an integer on the stack.
        /// </summary>
        /// <param name="value">The value.</param>
        void PushInt(int value);

        /// <summary>
        /// Pushes a double value on the stack.
        /// </summary>
        /// <param name="value">The value.</param>
        void PushDouble(double value);

        /// <summary>
        /// Pushes a check on the stack.
        /// </summary>
        /// <param name="opCode">The opcode that checks and jumps.</param>
        /// <param name="ifTrue">The result if true.</param>
        /// <param name="ifFalse">The result if false.</param>
        /// <exception cref="SpiceSharpException">Thrown if the operator code is invalid.</exception>
        void PushCheck(OpCode opCode, Node ifTrue, Node ifFalse);

        /// <summary>
        /// Calls a function.
        /// </summary>
        /// <param name="target">The target method.</param>
        /// <param name="method">The method info.</param>
        /// <param name="args">The arguments.</param>
        void Call(object target, MethodInfo method, IReadOnlyList<Node> args);

        /// <summary>
        /// Shorthand for calling a function.
        /// </summary>
        /// <param name="function">The function.</param>
        void Call(Func<T> function);

        /// <summary>
        /// Shorthand for calling a function with an argument.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="args">The arguments.</param>
        void Call(Func<T, T> function, IReadOnlyList<Node> args);

        /// <summary>
        /// Shorthand for calling a function with two arguments.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="args">The arguments.</param>
        void Call(Func<T, T, T> function, IReadOnlyList<Node> args);

        /// <summary>
        /// Shorthand for calling a function with three arguments.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="args">The arguments.</param>
        void Call(Func<T, T, T, T> function, IReadOnlyList<Node> args);
    }
}
