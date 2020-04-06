using System;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public class AttributeCompiler {

        private TemplateLinqCompiler updateCompiler;
        private TemplateLinqCompiler lateCompiler;

        internal static readonly Expression s_StringBuilderExpr = Expression.Field(null, typeof(StringUtil), nameof(StringUtil.s_CharStringBuilder));
        internal static readonly Expression s_StringBuilderClear = ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod("Clear"));
        internal static readonly Expression s_StringBuilderToString = ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod("ToString", Type.EmptyTypes));
        internal static readonly RepeatKeyFnTypeWrapper s_RepeatKeyFnTypeWrapper = new RepeatKeyFnTypeWrapper();

        public AttributeCompiler() {
            this.updateCompiler = new TemplateLinqCompiler();
            this.lateCompiler = new TemplateLinqCompiler();
        }

        public void CompileAttributes(ProcessedType processedType, TemplateNode node, AttributeSet attributeSet) {

            processedType.EnsureReflectionData();

            StructList<ContextAliasActions> contextModifications = null;

            for (int i = 0; i < attributeSet.size; i++) {

                AttributeContext attrContext = new AttributeContext();

                SetupCompilers();

                CompileConditionalBindings(node, attrContext.attributes);

                CompilePropertiesAndContextVariables(processedType, attrContext.attributes, ref contextModifications);

            }

            if (processedType.requiresUpdateFn) {
                // always uses root context
                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(updateCompiler.GetElement(), processedType.updateMethod));
            }

            // always uses root context
            CompileTextBinding(node as TextNode);

        }

        private void CompileConditionalBindings(TemplateNode templateNode, ReadOnlySizedArray<AttributeDefinition> attributes) {

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

                if (attr.type != AttributeType.Conditional) {
                    continue;
                }

                if ((attr.flags & AttributeFlags.Const) != 0) {
                    // CompileConditionalBinding(createdCompiler, attr);
                }
                else if ((attr.flags & AttributeFlags.EnableOnly) != 0) {
                    //  CompileConditionalBinding(enabledCompiler, attr);
                }
                else {
                    CompileConditionalBinding(updateCompiler, attr);
                }

            }
        }

        private static void CompileConditionalBinding(TemplateLinqCompiler compiler, in AttributeDefinition attr) {

            compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(
                compiler.GetElement(),
                MemberData.Element_SetEnabledInternal,
                compiler.Value(attr.value))
            );

        }

        private void CompilePropertiesAndContextVariables(ProcessedType processedType, ReadOnlySizedArray<AttributeDefinition> attributes, ref StructList<ContextAliasActions> contextModifications) {

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

                switch (attr.type) {
                    case AttributeType.Context:
                        // CompileContextVariable(attr, ref contextModifications);
                        break;

                    case AttributeType.Property: {
                        if (ReflectionUtil.IsEvent(processedType.rawType, attr.key, out EventInfo eventInfo)) {
                            // CompileEventBinding(createdCompiler, attr, eventInfo);
                            continue;
                        }

                        if ((attr.flags & AttributeFlags.Sync) != 0) {
                            // ContextVariableDefinition ctxVar = CompilePropertyBinding(updateCompiler, processedType, attr, changeHandlers);
                            // CompilePropertyBindingSync(lateCompiler, attr, ctxVar);
                        }
                        else if ((attr.flags & AttributeFlags.Const) != 0) {
                            //CompilePropertyBinding(createdCompiler, processedType, attr, changeHandlers);
                        }
                        else if ((attr.flags & AttributeFlags.EnableOnly) != 0) {
                            //CompilePropertyBinding(enabledCompiler, processedType, attr, changeHandlers);
                        }
                        else {
                            CompilePropertyBinding(updateCompiler, processedType, attr);
                        }

                        break;
                    }
                }
            }

        }

        private ContextVariableDefinition CompilePropertyBinding(TemplateLinqCompiler compiler, ProcessedType processedType, in AttributeDefinition attr) {

            LHSStatementChain left;
            Expression right = null;

            ParameterExpression castElement = compiler.GetElement();
            ParameterExpression rootElement = compiler.GetRoot();

            try {
                compiler.SetImplicitContext(castElement);
                left = compiler.AssignableStatement(attr.key);
            }
            catch (Exception e) {
                compiler.EndIsolatedSection();
                Debug.LogError(e); // todo -- diagnostic
                return null;
            }

            compiler.SetImplicitContext(rootElement);

            if (ReflectionUtil.IsFunc(left.targetExpression.Type)) {
                Type[] generics = left.targetExpression.Type.GetGenericArguments();
                Type target = generics[generics.Length - 1];
                if (HasTypeWrapper(target, out ITypeWrapper wrapper)) {
                    right = compiler.TypeWrapStatement(wrapper, left.targetExpression.Type, attr.value);
                }
            }

            if (right == null) {
                Expression accessor = compiler.AccessorStatement(left.targetExpression.Type, attr.value);

                if (accessor is ConstantExpression) {
                    right = accessor;
                }
                else {
                    right = compiler.AddVariable(left.targetExpression.Type, "__right");
                    compiler.Assign(right, accessor);
                }
            }

            // todo -- I can figure out if a value is constant using IsConstant(expr), use this information to push the expression onto the const compiler
            if (ExpressionUtil.IsConstant(right)) {
                Debug.Log("expression is always constant");
            }

            // todo -- clean up this mess
            if ((attr.flags & AttributeFlags.Const) != 0) {
                compiler.Assign(left, right);
            }
            else {
                StructList<PropertyChangeHandlerDesc> changeHandlers = StructList<PropertyChangeHandlerDesc>.Get();
                processedType.GetChangeHandlers(attr.key, changeHandlers);

                bool isProperty = ReflectionUtil.IsProperty(castElement.Type, attr.key);

                // if there is a change handler or the member is a property we need to check for changes
                // otherwise field values can be assigned w/o checking
                if (changeHandlers.size > 0 || isProperty) {
                    ParameterExpression old = compiler.AddVariable(left.targetExpression.Type, "__oldVal");
                    compiler.RawExpression(Expression.Assign(old, left.targetExpression));
                    // todo -- try to remove closure here
                    compiler.IfNotEqual(left, right, () => {
                        compiler.Assign(left, right);
                        for (int j = 0; j < changeHandlers.size; j++) {
                            MethodInfo methodInfo = changeHandlers.array[j].methodInfo;
                            ParameterInfo[] parameters = methodInfo.GetParameters();

                            if (!methodInfo.IsPublic) {
                                // todo -- diagnostic
                                throw TemplateCompileException.NonPublicPropertyChangeHandler(methodInfo.Name, right.Type);
                            }

                            if (parameters.Length == 0) {
                                compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(castElement, methodInfo));
                                continue;
                            }

                            if (parameters.Length == 1 && parameters[0].ParameterType == right.Type) {
                                compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(castElement, methodInfo, old));
                                continue;
                            }

                            // todo -- diagnostic
                            throw TemplateCompileException.UnresolvedPropertyChangeHandler(methodInfo.Name, right.Type); // todo -- better error message
                        }
                    });
                }
                else {
                    compiler.Assign(left, right);
                }

                changeHandlers.Release();
            }

            ContextVariableDefinition ctxVar = null;
            // todo -- context var

            // if ((attr.flags & AttributeFlags.Sync) != 0) {
            //     ctxVar = new ContextVariableDefinition();
            //     ctxVar.id = NextContextId;
            //     ctxVar.name = "sync_" + attr.key;
            //     ctxVar.type = left.targetExpression.Type;
            //     ctxVar.variableType = AliasResolverType.ContextVariable;
            //
            //     MethodCallExpression createVariable = CreateLocalContextVariableExpression(ctxVar, out Type contextVarType);
            //
            //     createdCompiler.RawExpression(createVariable);
            //
            //     ctxVar.contextVarType = contextVarType;
            //
            //     CompileAssignContextVariable(compiler, attr, ctxVar.contextVarType, ctxVar.id, "sync_", right);
            // }

            return ctxVar;

        }

        private void SetupCompilers() {
            updateCompiler.Reset();
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

        public enum ModType {

            Alias,
            Context

        }

        public struct ContextAliasActions {

            public ModType modType;
            public string name;

        }

    }

}