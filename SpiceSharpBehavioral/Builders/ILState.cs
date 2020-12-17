using SpiceSharp;
using SpiceSharpBehavioral.Diagnostics;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SpiceSharpBehavioral.Builders
{
    /// <summary>
    /// An instance for building functions.
    /// </summary>
    public class ILState
    {
        private static readonly MethodInfo _safeDiv = ((Func<double, double, double, double>)Functions.SafeDivide).GetMethodInfo();
        private static readonly MethodInfo _power = ((Func<double, double, double>)Functions.Power).GetMethodInfo();
        private static readonly MethodInfo _equals = ((Func<double, double, double, double, bool>)Functions.Equals).GetMethodInfo();
        private static readonly MethodInfo _invoke0 = typeof(Func<double>).GetTypeInfo().GetMethod("Invoke");
        private static readonly MethodInfo _invoke1 = typeof(Func<double, double>).GetTypeInfo().GetMethod("Invoke");
        private static readonly MethodInfo _invoke2 = typeof(Func<double, double, double>).GetTypeInfo().GetMethod("Invoke");
        private readonly DynamicMethod _method;
        private readonly Dictionary<object, int> _referenceMap = new Dictionary<object, int>();

        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>
        /// The builder.
        /// </value>
        public FunctionBuilder Builder { get; }

        /// <summary>
        /// Gets the IL generator.
        /// </summary>
        /// <value>
        /// The IL generator.
        /// </value>
        public ILGenerator Generator { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ILState"/> class.
        /// </summary>
        public ILState(FunctionBuilder parent)
        {
            Builder = parent.ThrowIfNull(nameof(parent));
            _method = new DynamicMethod("function", typeof(double), new[] { typeof(object[]) });
            Generator = _method.GetILGenerator();
        }

        /// <summary>
        /// Creates the function.
        /// </summary>
        /// <returns>The function.</returns>
        public Func<double> CreateFunction()
        {
            Generator.Emit(OpCodes.Ret);
            if (_referenceMap.Count > 0)
            {
                // Create a context - which is just an array of objects
                var context = new object[_referenceMap.Count];
                foreach (var pair in _referenceMap)
                    context[pair.Value] = pair.Key;
                return (Func<double>)_method.CreateDelegate(typeof(Func<double>), context);
            }
            else
                return (Func<double>)_method.CreateDelegate(typeof(Func<double>));
        }

        /// <summary>
        /// Pushes an expression on the stack.
        /// </summary>
        /// <param name="node">The node.</param>
        public void Push(Node node)
        {
            switch (node)
            {
                case BinaryOperatorNode bn:

                    // Execution
                    switch (bn.NodeType)
                    {
                        case NodeTypes.Add:
                            Push(bn.Left);
                            Push(bn.Right);
                            Generator.Emit(OpCodes.Add);
                            return;

                        case NodeTypes.Subtract:
                            Push(bn.Left);
                            Push(bn.Right);
                            Generator.Emit(OpCodes.Sub);
                            return;

                        case NodeTypes.Multiply:
                            Push(bn.Left);
                            Push(bn.Right); 
                            Generator.Emit(OpCodes.Mul);
                            return;

                        case NodeTypes.Divide:
                            Push(bn.Left);
                            Push(bn.Right); 
                            Generator.Emit(OpCodes.Ldc_R8, Builder.FudgeFactor);
                            Generator.Emit(OpCodes.Call, _safeDiv);
                            return;

                        case NodeTypes.Modulo:
                            Push(bn.Left);
                            Push(bn.Right);
                            Generator.Emit(OpCodes.Rem);
                            return;

                        case NodeTypes.GreaterThan:
                            Push(bn.Left);
                            Push(bn.Right); 
                            PushCheck(OpCodes.Bgt_S);
                            return;

                        case NodeTypes.LessThan:
                            Push(bn.Left);
                            Push(bn.Right);
                            PushCheck(OpCodes.Blt_S);
                            return;

                        case NodeTypes.GreaterThanOrEqual:
                            Push(bn.Left);
                            Push(bn.Right); 
                            PushCheck(OpCodes.Bge_S);
                            return;

                        case NodeTypes.LessThanOrEqual:
                            Push(bn.Left);
                            Push(bn.Right);
                            PushCheck(OpCodes.Ble_S);
                            return;

                        case NodeTypes.Equals:
                            Push(bn.Left);
                            Push(bn.Right);
                            Generator.Emit(OpCodes.Ldc_R8, Builder.RelativeTolerance);
                            Generator.Emit(OpCodes.Ldc_R8, Builder.AbsoluteTolerance);
                            Generator.Emit(OpCodes.Call, _equals);
                            PushCheck(OpCodes.Brtrue_S);
                            return;

                        case NodeTypes.NotEquals:
                            Push(bn.Left);
                            Push(bn.Right);
                            Generator.Emit(OpCodes.Ldc_R8, Builder.RelativeTolerance);
                            Generator.Emit(OpCodes.Ldc_R8, Builder.AbsoluteTolerance);
                            Generator.Emit(OpCodes.Call, _equals);
                            PushCheck(OpCodes.Brfalse_S);
                            return;

                        case NodeTypes.And:
                            var lblBypass = Generator.DefineLabel();
                            var lblEnd = Generator.DefineLabel();

                            Push(bn.Left);
                            Generator.Emit(OpCodes.Ldc_R8, 0.5);
                            Generator.Emit(OpCodes.Ble_S, lblBypass);
                            Push(bn.Right);
                            Generator.Emit(OpCodes.Ldc_R8, 0.5);
                            Generator.Emit(OpCodes.Ble_S, lblBypass);
                            Generator.Emit(OpCodes.Ldc_R8, 1.0);
                            Generator.Emit(OpCodes.Br_S, lblEnd);
                            Generator.MarkLabel(lblBypass);
                            Generator.Emit(OpCodes.Ldc_R8, 0.0);
                            Generator.MarkLabel(lblEnd);
                            return;

                        case NodeTypes.Or:
                            lblBypass = Generator.DefineLabel();
                            lblEnd = Generator.DefineLabel();

                            Push(bn.Left);
                            Generator.Emit(OpCodes.Ldc_R8, 0.5);
                            Generator.Emit(OpCodes.Bgt_S, lblBypass);
                            Push(bn.Right);
                            Generator.Emit(OpCodes.Ldc_R8, 0.5);
                            Generator.Emit(OpCodes.Bgt_S, lblBypass);
                            Generator.Emit(OpCodes.Ldc_R8, 0.0);
                            Generator.Emit(OpCodes.Br_S, lblEnd);
                            Generator.MarkLabel(lblBypass);
                            Generator.Emit(OpCodes.Ldc_R8, 1.0);
                            Generator.MarkLabel(lblEnd);
                            return;

                        case NodeTypes.Xor:
                            Push(bn.Left);
                            Generator.Emit(OpCodes.Ldc_R8, 0.5);
                            Generator.Emit(OpCodes.Cgt);
                            Push(bn.Right);
                            Generator.Emit(OpCodes.Ldc_R8, 0.5);
                            Generator.Emit(OpCodes.Cgt);
                            Generator.Emit(OpCodes.Xor);
                            PushCheck(OpCodes.Brtrue);
                            return;

                        case NodeTypes.Pow:
                            Push(bn.Left);
                            Push(bn.Right);
                            Generator.Emit(OpCodes.Call, _power);
                            return;
                    }
                    break;

                case ConstantNode cn:
                    Generator.Emit(OpCodes.Ldc_R8, cn.Literal);
                    return;
                
                case UnaryOperatorNode un:
                    Push(un.Argument);
                    switch (un.NodeType)
                    {
                        case NodeTypes.Plus: return;
                        case NodeTypes.Minus: Generator.Emit(OpCodes.Neg); return;
                        case NodeTypes.Not:
                            Generator.Emit(OpCodes.Ldc_R8, 0.5);
                            PushCheck(OpCodes.Ble_S);
                            return;
                    }
                    break;

                case FunctionNode fn:
                    if (Builder.FunctionDefinitions != null && Builder.FunctionDefinitions.TryGetValue(fn.Name, out var definition))
                    {
                        definition.ThrowIfNull(nameof(definition));
                        definition.Invoke(this, fn.Arguments);
                        return;
                    }
                    break;

                case VariableNode vn:
                    if (Builder.Variables != null && Builder.Variables.TryGetValue(vn, out var variable))
                    {
                        Call(() => variable.Value);
                        return;
                    }
                    break;
            }

            throw new Exception("Unrecognized node {0}".FormatString(node));
        }

        /// <summary>
        /// Pushes the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void Push(int index)
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
        /// Pushes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Push(double value) => Generator.Emit(OpCodes.Ldc_R8, value);

        /// <summary>
        /// Pushes a check.
        /// </summary>
        /// <param name="opCode">The operator code.</param>
        /// <exception cref="Exception">Thrown if the operator code is invalid.</exception>
        public void PushCheck(OpCode opCode)
        {
            // Check the op-code to make sure it can be used here
            if (opCode.OperandType != OperandType.InlineBrTarget && opCode.OperandType != OperandType.ShortInlineBrTarget)
                throw new Exception("Operation code is invalid");
            var trueLbl = Generator.DefineLabel();
            var exitLbl = Generator.DefineLabel();
            Generator.Emit(opCode, trueLbl);
            Generator.Emit(OpCodes.Ldc_R8, 0.0);
            Generator.Emit(OpCodes.Br_S, exitLbl);
            Generator.MarkLabel(trueLbl);
            Generator.Emit(OpCodes.Ldc_R8, 1.0);
            Generator.MarkLabel(exitLbl);
        }

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
                Push(index);
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
        public void Call(Func<double> function)
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
        public void Call(Func<double, double> function, IReadOnlyList<Node> args)
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
        public void Call(Func<double, double, double> function, IReadOnlyList<Node> args)
        {
            if (args == null || args.Count != 2)
                throw new ArgumentMismatchException(2, args?.Count ?? 0);
            if (function.Target == null)
                Call(null, function.GetMethodInfo(), args);
            else
                Call(function, _invoke2, args);
        }
    }
}
