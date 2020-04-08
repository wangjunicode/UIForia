using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Parsing.Expressions.AstNodes;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public class AttributeCompiler {

        private TemplateLinqCompiler constCompiler;
        private TemplateLinqCompiler updateCompiler;
        private TemplateLinqCompiler lateCompiler;

        internal static readonly Expression s_StringBuilderExpr = Expression.Field(null, typeof(StringUtil), nameof(StringUtil.s_CharStringBuilder));
        internal static readonly Expression s_StringBuilderClear = ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod("Clear"));
        internal static readonly Expression s_StringBuilderToString = ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod("ToString", Type.EmptyTypes));
        internal static readonly RepeatKeyFnTypeWrapper s_RepeatKeyFnTypeWrapper = new RepeatKeyFnTypeWrapper();
        private static readonly Dictionary<Type, MethodInfo> s_SyncGetterCache = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, MethodInfo> s_SyncSetterCache = new Dictionary<Type, MethodInfo>();

        private readonly StructList<PropertyChangeHandlerDesc> changeHandlers;
        private readonly StructList<SyncPropertyData> syncData;

        private int contextId = 1;
        private int NextContextId => contextId++;
        private int syncCount = 0;

        public AttributeCompiler() {
            this.updateCompiler = new TemplateLinqCompiler();
            this.lateCompiler = new TemplateLinqCompiler();
            this.constCompiler = new TemplateLinqCompiler();
            this.changeHandlers = new StructList<PropertyChangeHandlerDesc>(16);
            this.syncData = new StructList<SyncPropertyData>();
        }

        private static bool IsAttrBeforeUpdate(in AttrInfo attrInfo) {
            return (attrInfo.type == AttributeType.Conditional || attrInfo.type == AttributeType.Property || attrInfo.type == AttributeType.Alias);
        }

        private TemplateLinqCompiler GetCompiler(bool isConst, in AttrInfo attrInfo) {
            // if (isConst || (attrInfo.flags & AttributeFlags.Const) != 0) {
            //     constCompiler.Setup(attrInfo);
            //     return constCompiler;
            // }

            // todo if const + or once or isConst + sync -> probably throw an error?
            updateCompiler.Setup(attrInfo);
            return updateCompiler;
        }

        private void InitializeCompilers(Type elementType, AttributeSet attributeSet) {
            // todo -- namespaces
            updateCompiler.Init(elementType, attributeSet.contextTypes);
            lateCompiler.Init(elementType, attributeSet.contextTypes);
        }

        public void CompileAttributes(ProcessedType processedType, TemplateNode node, AttributeSet attributeSet, ref BindingResult bindingResult) {
            syncCount = 0;
            processedType.EnsureReflectionData();

            StructList<ContextAliasActions> contextModifications = null;

            // [InvokeWhenDisabled]
            // Update() { } 
            // const + once bindings and unmarked bindings that are constant

            InitializeCompilers(processedType.rawType, attributeSet);

            for (int i = 0; i < attributeSet.attributes.size; i++) {

                ref AttrInfo attr = ref attributeSet.attributes.array[i];

                if (!IsAttrBeforeUpdate(attr)) {
                    continue;
                }

                ASTNode ast = ExpressionParser.Parse(attr.value);

                TemplateLinqCompiler compiler = GetCompiler(IsConstantExpression(ast), attr);

                switch (attr.type) {

                    case AttributeType.Conditional: {
                        compiler.SetImplicitContext(compiler.GetRoot(), ParameterFlags.NeverNull);
                        compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(
                                compiler.GetElement(),
                                MemberData.Element_SetEnabledInternal,
                                compiler.Value(ast)
                            )
                        );
                        break;
                    }

                    case AttributeType.Property: {

                        CompilePropertyBinding(compiler, ast, processedType, attr);

                        break;
                    }

                }

            }

            // if (processedType.requiresUpdateFn) {
            //     // always uses root context
            //     updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(updateCompiler.GetElement(), processedType.updateMethod));
            // }
            //
            // // always uses root context
            // CompileTextBinding(node as TextNode);

            BuildLambda(updateCompiler, ref bindingResult.updateLambda);
            BuildLambda(lateCompiler, ref bindingResult.lateLambda);
            // BuildLambda(updateCompiler, ref retn.constEnableLambda);

        }

        private static void BuildLambda(TemplateLinqCompiler compiler, ref LambdaExpression lambdaExpression) {
            if (!compiler.HasStatements) {
                return;
            }

            try {
                lambdaExpression = compiler.BuildLambda();
            }
            catch (Exception e) {
                // todo -- diagnostic
                Debug.Log(e);
            }
        }

        private void CompilePropertyBinding(TemplateLinqCompiler compiler, ASTNode expr, ProcessedType processedType, in AttrInfo attr) {

            LHSStatementChain left;
            Expression right = null;

            ParameterExpression element = compiler.GetElement();
            ParameterExpression root = compiler.GetRoot();
            compiler.SetImplicitContext(element);

            try {
                left = compiler.AssignableStatement(attr.key);
            }
            catch (Exception e) {
                Debug.LogError(e); // todo -- diagnostic, unroll compiler changes 
                return;
            }

            compiler.SetImplicitContext(root);

            // todo -- action / delegate support
            if (ReflectionUtil.IsFunc(left.targetExpression.Type)) {
                Type[] generics = left.targetExpression.Type.GetGenericArguments();
                Type target = generics[generics.Length - 1];
                if (HasTypeWrapper(target, out ITypeWrapper wrapper)) {
                    right = compiler.TypeWrapStatement(wrapper, left.targetExpression.Type, expr);
                }
            }

            if (right == null) {
                Expression accessor = compiler.AccessorStatement(left.targetExpression.Type, expr);

                if (accessor is ConstantExpression) {
                    right = accessor;
                }
                else {
                    right = compiler.AddVariable(left.targetExpression.Type, "__value");
                    compiler.Assign(right, accessor);
                }
            }

            // if there is a change handler we need to check for changes
            // otherwise field values can be assigned w/o checking

            // todo -- handled dotted accessors like <Element property:someArray[i].value="4"/>, likely needs parser support
            if (processedType.TryGetChangeHandlers(attr.key, PropertyChangedType.BindingRead, changeHandlers)) {

                ParameterExpression old = compiler.AddVariable(left.targetExpression.Type, "__oldVal");

                compiler.RawExpression(Expression.Assign(old, left.targetExpression));

                compiler.IfNotEqual(left, right, () => {

                    compiler.Assign(left, right);

                    CompilePropertyChangeHandlers(compiler, PropertyChangeSource.BindingRead, element, right, old);

                });
            }
            else {
                compiler.Assign(left, right);
            }

            if ((attr.flags & AttributeFlags.Sync) != 0) {
                int syncIdx = syncCount++;

                MethodInfo setSyncVar = GetSyncSetter(left.targetExpression.Type);

                compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(compiler.GetBindingNode(), setSyncVar, ExpressionUtil.GetIntConstant(syncIdx), right));

                // todo -- assert is update

                CompilePropertyBindingSync(syncIdx, attr, right.Type);

            }

        }

        private void CompilePropertyChangeHandlers(TemplateLinqCompiler compiler, PropertyChangeSource changeSource, ParameterExpression element, Expression right, Expression prevValue) {
            for (int j = 0; j < changeHandlers.size; j++) {

                MethodInfo methodInfo = changeHandlers.array[j].methodInfo;
                ParameterInfo[] parameters = changeHandlers.array[j].parameterInfos;

                // todo -- support PropertyChangeSource (binding read vs sync)

                if (parameters.Length == 0) {
                    compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(element, methodInfo));
                    continue;
                }

                if (parameters.Length == 1 && parameters[0].ParameterType == right.Type) {
                    compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(element, methodInfo, prevValue));
                    continue;
                }

                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(PropertyChangeSource)) {
                    compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(element, methodInfo, Expression.Constant(changeSource)));
                    continue;
                }

                if (parameters.Length == 2 && parameters[0].ParameterType == typeof(PropertyChangeSource) && parameters[1].ParameterType == right.Type) {
                    compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(element, methodInfo, Expression.Constant(changeSource), prevValue));
                    continue;
                }

                if (parameters.Length == 2 && parameters[0].ParameterType == right.Type && parameters[1].ParameterType == typeof(PropertyChangeSource)) {
                    compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(element, methodInfo, prevValue, Expression.Constant(changeSource)));
                    continue;
                }

                // todo -- diagnostic
                throw TemplateCompileException.UnresolvedPropertyChangeHandler(methodInfo.Name, right.Type); // todo -- better error message
            }
        }

        private void CompilePropertyBindingSync(int syncIdx, in AttrInfo attr, Type syncVarType) {

            lateCompiler.Setup(attr);
            Expression right;

            ParameterExpression element = lateCompiler.GetElement();
            ParameterExpression root = lateCompiler.GetRoot();

            lateCompiler.SetImplicitContext(root);

            LHSStatementChain assignableStatement = lateCompiler.AssignableStatement(attr.value);

            Expression accessor = lateCompiler.AccessorStatement(assignableStatement.targetExpression.Type, attr.value);

            if (accessor is ConstantExpression) {
                right = accessor;
            }
            else {
                right = lateCompiler.AddVariable(assignableStatement.targetExpression.Type, "__right");
                lateCompiler.Assign(right, accessor);
            }

            lateCompiler.SetImplicitContext(root);

            Expression expr = ExpressionFactory.CallInstanceUnchecked(lateCompiler.GetBindingNode(), GetSyncGetter(syncVarType), ExpressionUtil.GetIntConstant(syncIdx));

            lateCompiler.SetImplicitContext(element);

            string key = attr.key;
            string val = attr.value;

            lateCompiler.IfEqual(expr, right, () => {
                // todo -- currently only supports fields, properties should also work
                lateCompiler.Assign(assignableStatement, Expression.MakeMemberAccess(element, element.Type.GetField(key)));

                if (lateCompiler.GetContextProcessedType().TryGetChangeHandlers(val, PropertyChangedType.Synchronized, changeHandlers)) {
                    CompilePropertyChangeHandlers(lateCompiler, PropertyChangeSource.Synchronized, root, right, right);
                }

            });
        }

        private void CompileTextBinding(TextNode textNode) {

            if (textNode?.textExpressionList == null || textNode.textExpressionList.size <= 0 || textNode.IsTextConstant()) {
                return;
            }

            // todo -- remove namespaces? do we even need them?
            updateCompiler.AddNamespace("UIForia.Util");
            updateCompiler.AddNamespace("UIForia.Text");

            StructList<TextExpression> expressionParts = textNode.textExpressionList;

            MemberExpression textValueExpr = Expression.Field(updateCompiler.GetElement(), MemberData.TextElement_Text);

            updateCompiler.RawExpression(s_StringBuilderClear);

            for (int i = 0; i < expressionParts.size; i++) {
                if (expressionParts[i].isExpression) {

                    Expression val = updateCompiler.Value(expressionParts[i].text);
                    if (val.Type.IsEnum) {
                        MethodCallExpression toString = ExpressionFactory.CallInstanceUnchecked(val, val.Type.GetMethod("ToString", Type.EmptyTypes));
                        updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendString, toString));
                        continue;
                    }

                    switch (Type.GetTypeCode(val.Type)) {
                        case TypeCode.Boolean:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendBool, val));
                            break;

                        case TypeCode.Byte:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendByte, val));
                            break;

                        case TypeCode.Char:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendChar, val));
                            break;

                        case TypeCode.Decimal:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendDecimal, val));
                            break;

                        case TypeCode.Double:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendDouble, val));
                            break;

                        case TypeCode.Int16:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendInt16, val));
                            break;

                        case TypeCode.Int32:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendInt32, val));
                            break;

                        case TypeCode.Int64:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendInt64, val));
                            break;

                        case TypeCode.SByte:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendSByte, val));
                            break;

                        case TypeCode.Single:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendFloat, val));
                            break;

                        case TypeCode.String:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendString, val));
                            break;

                        case TypeCode.UInt16:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendUInt16, val));
                            break;

                        case TypeCode.UInt32:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendUInt32, val));
                            break;

                        case TypeCode.UInt64:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendUInt64, val));
                            break;

                        default:
                            // todo -- search for a ToString(CharStringBuilder) implementation and use that if possible
                            // maybe implement special cases for common unity types
                            MethodCallExpression toString = ExpressionFactory.CallInstanceUnchecked(val, val.Type.GetMethod("ToString", Type.EmptyTypes));
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendString, toString));
                            break;
                    }
                }
                else {
                    updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, MemberData.StringBuilder_AppendString, Expression.Constant(expressionParts[i].text)));
                }
            }

            // todo -- this needs to check the TextInfo for equality or whitespace mutations will be ignored and we will return false from equal!!!
            Expression e = updateCompiler.GetElement();
            Expression condition = ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.EqualsString), new[] {typeof(string)}), textValueExpr);
            condition = Expression.Equal(condition, Expression.Constant(false));
            ConditionalExpression ifCheck = Expression.IfThen(condition, Expression.Block(ExpressionFactory.CallInstanceUnchecked(e, MemberData.TextElement_SetText, s_StringBuilderToString)));

            updateCompiler.RawExpression(ifCheck);
            updateCompiler.RawExpression(s_StringBuilderClear);
        }

        private static bool HasTypeWrapper(Type type, out ITypeWrapper typeWrapper) {
            if (type == typeof(RepeatItemKey)) {
                typeWrapper = s_RepeatKeyFnTypeWrapper;
                return true;
            }

            typeWrapper = null;
            return false;
        }

        private static MethodInfo GetSyncSetter(Type type) {
            if (s_SyncSetterCache.TryGetValue(type, out MethodInfo setter)) {
                return setter;
            }

            ReflectionUtil.TypeArray1[0] = type;
            setter = MemberData.BindingNode_SetSyncVar.MakeGenericMethod(ReflectionUtil.TypeArray1);
            s_SyncSetterCache.Add(type, setter);
            return setter;
        }

        private static MethodInfo GetSyncGetter(Type type) {
            if (s_SyncGetterCache.TryGetValue(type, out MethodInfo getter)) {
                return getter;
            }

            ReflectionUtil.TypeArray1[0] = type;
            getter = MemberData.BindingNode_GetSyncVar.MakeGenericMethod(ReflectionUtil.TypeArray1);
            s_SyncGetterCache.Add(type, getter);
            return getter;
        }

        private static bool IsConstantExpression(ASTNode n) {
            while (true) {
                switch (n) {
                    case LiteralNode _:
                        return true;

                    //not sure about this one 
                    case ParenNode parenNode:
                        return parenNode.accessExpression == null && IsConstantExpression(parenNode.expression);

                    //not sure about this one 
                    case UnaryExpressionNode unary:
                        n = unary.expression;
                        continue;

                    case OperatorNode binaryExpression:
                        return IsConstantExpression(binaryExpression.left) && IsConstantExpression(binaryExpression.right);
                }

                return false;

            }
        }

        public enum ModType {

            Alias,
            Context

        }

        public struct ContextAliasActions {

            public ModType modType;
            public string name;

        }

    }

    public struct BindingResult {

        public LambdaExpression lateLambda;
        public LambdaExpression updateLambda;
        public LambdaExpression constEnableLambda;
        public SizedArray<SyncPropertyData> syncData;

        public bool HasValue {
            get => lateLambda != null || updateLambda != null || constEnableLambda != null;
        }

    }

    public struct SyncPropertyData {

        public int index;
        public Type type;
        public Expression syncArrayAccessExpr;
        public LHSStatementChain leftSideAssign;

    }

}