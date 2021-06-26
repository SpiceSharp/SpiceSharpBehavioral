using SpiceSharp;
using SpiceSharpBehavioral.Diagnostics;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SpiceSharpBehavioral.Builders.Functions
{
    /// <summary>
    /// An instance for building functions.
    /// </summary>
    public abstract class ILState<T> : IILState<T>
    {
        private static readonly MethodInfo _invoke0 = typeof(Func<T>).GetTypeInfo().GetMethod("Invoke");
        private static readonly MethodInfo _invoke1 = typeof(Func<T, T>).GetTypeInfo().GetMethod("Invoke");
        private static readonly MethodInfo _invoke2 = typeof(Func<T, T, T>).GetTypeInfo().GetMethod("Invoke");

        private readonly DynamicMethod _method;
        private readonly Dictionary<object, int> _referenceMap = new Dictionary<object, int>();

        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>
        /// The builder.
        /// </value>
        public IFunctionBuilder<T> Builder { get; }

        /// <summary>
        /// Gets the IL generator.
        /// </summary>
        /// <value>
        /// The IL generator.
        /// </value>
        public ILGenerator Generator { get; private set; }

        /// <summary>
        /// Occurs when a function was encountered.
        /// </summary>
        public event EventHandler<FunctionFoundEventArgs<T>> FunctionFound;

        /// <summary>
        /// Occurs when a variable was encountered.
        /// </summary>
        public event EventHandler<VariableFoundEventArgs<T>> VariableFound;

        /// <summary>
        /// Initializes a new instance of the <see cref="ILState{T}"/> class.
        /// </summary>
        /// <param name="parent">The parent function builder.</param>
        protected ILState(IFunctionBuilder<T> parent)
        {
            Builder = parent.ThrowIfNull(nameof(parent));
            _method = new DynamicMethod("function", typeof(T), new[] { typeof(object[]) });
            Generator = _method.GetILGenerator();
        }

        /// <summary>
        /// Call when a function has been encountered.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected void OnFunctionFound(FunctionFoundEventArgs<T> args) => FunctionFound?.Invoke(this, args);

        /// <summary>
        /// Call when a variable has been encountered.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected void OnVariableFound(VariableFoundEventArgs<T> args) => VariableFound?.Invoke(this, args);

        /// <summary>
        /// Creates the function.
        /// </summary>
        /// <returns>The function.</returns>
        public Func<T> CreateFunction()
        {
            Generator.Emit(OpCodes.Ret);
            if (_referenceMap.Count > 0)
            {
                // Create a context - which is just an array of objects
                var context = new object[_referenceMap.Count];
                foreach (var pair in _referenceMap)
                    context[pair.Value] = pair.Key;
                return (Func<T>)_method.CreateDelegate(typeof(Func<T>), context);
            }
            else
                return (Func<T>)_method.CreateDelegate(typeof(Func<T>));
        }

        /// <summary>
        /// Pushes the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void PushInt(int index)
        {
            // Set the index in the array
            switch (index)
            {
                case 0: Generator.Emit(OpCodes.Ldc_I4_0); break;
                case 1: Generator.Emit(OpCodes.Ldc_I4_1); break;
                case 2: Generator.Emit(OpCodes.Ldc_I4_2); break;
                case 3: Generator.Emit(OpCodes.Ldc_I4_3); break;
                case 4: Generator.Emit(OpCodes.Ldc_I4_4); break;
                case 5: Generator.Emit(OpCodes.Ldc_I4_5); break;
                case 6: Generator.Emit(OpCodes.Ldc_I4_6); break;
                case 7: Generator.Emit(OpCodes.Ldc_I4_7); break;
                case 8: Generator.Emit(OpCodes.Ldc_I4_8); break;
                case int s when s > 8 && s <= 127: Generator.Emit(OpCodes.Ldc_I4_S, (byte)s); break;
                default: Generator.Emit(OpCodes.Ldc_I4, index); break;
            }
        }

        /// <summary>
        /// Pushes the specified double value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void PushDouble(double value) => Generator.Emit(OpCodes.Ldc_R8, value);

        /// <summary>
        /// Pushes a value on the stack.
        /// </summary>
        /// <param name="value">The value.</param>
        public abstract void Push(T value);

        /// <inheritdoc/>
        public void PushCheck(OpCode opCode, Node ifTrue, Node ifFalse)
        {
            // Check the op-code to make sure it can be used here
            if (opCode.OperandType != OperandType.InlineBrTarget && opCode.OperandType != OperandType.ShortInlineBrTarget)
                throw new SpiceSharpException("Operation code is invalid");
            var trueLbl = Generator.DefineLabel();
            var exitLbl = Generator.DefineLabel();
            Generator.Emit(opCode, trueLbl);
            Push(ifFalse);
            Generator.Emit(OpCodes.Br_S, exitLbl);
            Generator.MarkLabel(trueLbl);
            Push(ifTrue);
            Generator.MarkLabel(exitLbl);
        }

        /// <summary>
        /// Pushes an expression on the stack.
        /// </summary>
        /// <param name="node">The node.</param>
        public abstract void Push(Node node);

        /// <summary>
        /// Calls the specified method where all arguments are doubles.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="method">The method.</param>
        /// <param name="args">The arguments.</param>
        /// <exception cref="ArgumentMismatchException"></exception>
        public void Call(object target, MethodInfo method, IReadOnlyList<Node> args)
        {
            if (target != null)
            {
                if (!_referenceMap.TryGetValue(target, out int index))
                {
                    index = _referenceMap.Count;
                    _referenceMap.Add(target, index);
                }
                
                // Add the target
                Generator.Emit(OpCodes.Ldarg_0);
                PushInt(index);
                Generator.Emit(OpCodes.Ldelem_Ref);
                Generator.Emit(OpCodes.Castclass, target.GetType());
            }

            // Push
            if (args != null)
            {
                foreach (var arg in args)
                    Push(arg);
            }

            // If the method has a target, then we may want to call the late-bound version.
            if (target != null)
                Generator.Emit(OpCodes.Callvirt, method);
            else
                Generator.Emit(OpCodes.Call, method);
        }

        /// <summary>
        /// Calls the specified function.
        /// </summary>
        /// <param name="function">The function.</param>
        public void Call(Func<T> function)
        {
            if (function.Target == null)
                Call(null, function.GetMethodInfo(), null);
            else
                Call(function, _invoke0, null);
        }

        /// <summary>
        /// Calls the specified function.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="args">The arguments.</param>
        /// <exception cref="ArgumentMismatchException">Thrown if there isn't one argument.</exception>
        public void Call(Func<T, T> function, IReadOnlyList<Node> args)
        {
            if (args == null || args.Count != 1)
                throw new ArgumentMismatchException(1, args?.Count ?? 0);
            if (function.Target == null)
                Call(null, function.GetMethodInfo(), args);
            else
                Call(function, _invoke1, args);
        }

        /// <summary>
        /// Calls the specified function.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="args">The arguments.</param>
        /// <exception cref="ArgumentMismatchException">Thrown if there aren't two arguments.</exception>
        public void Call(Func<T, T, T> function, IReadOnlyList<Node> args)
        {
            if (args == null || args.Count != 2)
                throw new ArgumentMismatchException(2, args?.Count ?? 0);
            if (function.Target == null)
                Call(null, function.GetMethodInfo(), args);
            else
                Call(function, _invoke2, args);
        }

        /// <summary>
        /// Calls the specified function.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="args">The arguments.</param>
        /// <exception cref="ArgumentMismatchException">Thrown if there aren't two arguments.</exception>
        public void Call(Func<T, T, T, T> function, IReadOnlyList<Node> args)
        {
            if (args == null || args.Count != 3)
                throw new ArgumentMismatchException(3, args?.Count ?? 0);
            if (function.Target == null)
                Call(null, function.GetMethodInfo(), args);
            else
                Call(function, _invoke2, args);
        }
    }
}
