using System;
using System.Linq.Expressions;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Systems;
using UIForia.Util;

namespace UIForia.Compilers {

    internal class TemplateLinqCompiler : LinqCompiler {

        [ThreadStatic] private static ParameterExpression elementParam;

        private Expression castExpression;
        private ParameterExpression elementExpression;
        private ParameterExpression parentExpression;
        private LightList<ParameterExpression> contextExpressionList;

        private readonly Parameter parameter;
        private AttributeCompilerContext context;
        private AttrInfo currentAttribute;

        public TemplateLinqCompiler(AttributeCompilerContext context) {
            this.context = context;
            this.resolveAlias = ResolveAlias;
            this.contextExpressionList = new LightList<ParameterExpression>();
            this.parameter = new Parameter(typeof(LinqBindingNode), "bindingNode", ParameterFlags.NeverNull);
        }

        public void Setup() {
            SetNamespaces(context.namespaces);
            contextExpressionList.EnsureCapacity(context.references.size);
        }

        public void Init() {
            Reset();
            SetSignature(parameter);

            this.elementExpression = null;
            this.parentExpression = null;
            this.contextExpressionList.Clear();
        }

        public ParameterExpression GetRoot() {

            if (context.currentAttribute.isInjected) {
                if (context.parentType.rawType == typeof(UISlotDefinition)) {
                    // bindingNode.GetParentContext<T>(index);
                    // ContextData parentContextData = context.parentBindingResult.GetContextData(context.currentAttribute.depth);
                    // contextExpression = AddVariable(parentContextData.type, parentContextData.index);
                }

                throw new NotImplementedException();
            }

            ref TemplateContextReference ctx = ref context.references.array[context.depth];
            switch (context.templateNodeType) {

                case TemplateNodeType.SlotOverride: {
                    if (contextExpressionList.array[context.depth] == null) {
                        // reference array is inverted from attr depth, ie depth = level slot was defined on, level contexts.size -1 == level slot was overridden on. forwards are between 0 and 1
                        int diff = context.references.size - 1 - context.depth;
                        contextExpressionList.array[context.depth] = AddVariable(context.references.array[diff].processedType.rawType, "refContext_" + diff, ParameterFlags.NeverNull);
                        IndexExpression array = Expression.ArrayAccess(Expression.Field(parameter.expression, MemberData.BindingNode_ReferencedContexts), ExpressionUtil.GetIntConstant(diff));
                        Assign(contextExpressionList.array[context.depth], Expression.TypeAs(array, context.references.array[diff].processedType.rawType));
                    }

                    break;
                }

                case TemplateNodeType.SlotDefine:
                // case TemplateNodeType.Container:
                    if (contextExpressionList.array[context.depth] == null) {
                        contextExpressionList.array[context.depth] = AddVariable(ctx.processedType.rawType, "context_" + context.depth, ParameterFlags.NeverNull);
                        Assign(contextExpressionList.array[context.depth], Expression.TypeAs(Expression.Field(parameter.expression, MemberData.BindingNode_Root), ctx.processedType.rawType));
                    }

                    break;

                // case TemplateNodeType.Expanded:
                //     if (contextExpressionList.array[context.depth] == null) {
                //         contextExpressionList.array[context.depth] = AddVariable(ctx.processedType.rawType, "context_" + context.depth, ParameterFlags.NeverNull);
                //         Assign(contextExpressionList.array[context.depth], Expression.TypeAs(Expression.Field(parameter.expression, MemberData.BindingNode_Root), ctx.processedType.rawType));
                //     }
                //
                //     break;

                case TemplateNodeType.Root:
                    return GetElement();

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return contextExpressionList.array[context.depth];
        }

        public ParameterExpression GetElement() {

            if (elementExpression == null) {
                elementExpression = AddVariable(context.elementType.rawType, "element", ParameterFlags.NeverNull);
                Assign(elementExpression, Expression.TypeAs(Expression.Field(parameter.expression, MemberData.BindingNode_Element), context.elementType.rawType));
            }

            return elementExpression;
        }

        public Expression GetBindingNode() {
            return parameter.expression;
        }

        public ProcessedType GetContextProcessedType() {
            return context.references.array[context.depth].processedType;
        }

        public Expression GetParent() {
            // should return the parent and skip any implicit elements
            // add a variable for parent type
            throw new NotImplementedException();
        }

        private Expression ResolveAlias(string aliasName, LinqCompiler _) {
            return default;
            // if (aliasName == "oldValue") {
            //     if (context.changeHandlerPreviousValue == null) {
            //         throw new TemplateCompileException("Invalid use of $oldValue, this alias is only available when used inside of an onChange handler");
            //     }
            //
            //     return context.changeHandlerPreviousValue;
            // }
            //
            // if (aliasName == "newValue") {
            //     if (context.changeHandlerCurrentValue == null) {
            //         throw new TemplateCompileException("Invalid use of $newValue, this alias is only available when used inside of an onChange handler");
            //     }
            //
            //     return context.changeHandlerCurrentValue;
            // }
            //
            // if (aliasName == "element") {
            //     return GetElement();
            // }
            //
            // if (aliasName == "parent") {
            //     // todo -- should return the parent but ignore intrinsic elements like RepeatMulitChildContainer
            //     return GetParent();
            // }
            //
            // if (aliasName == "evt") {
            //     if (context.currentEvent == null) {
            //         throw new TemplateCompileException("Invalid use of $evt, this alias is only available when used inside of an event handler");
            //     }
            //
            //     return context.currentEvent;
            // }
            //
            // if (aliasName == "event") {
            //     if (context.currentEvent == null) {
            //         throw new TemplateCompileException("Invalid use of $event, this alias is only available when used inside of an event handler");
            //     }
            //
            //     return context.currentEvent;
            // }
            //
            // if (aliasName == "root" || aliasName == "this") {
            //     return GetRoot();
            // }
            //
            // if (context.TryGetBindingVariable(aliasName, out BindingVariableDesc variable)) {
            //     return ExpressionFactory.CallInstance(
            //         GetBindingNode(),
            //         AttributeCompiler.GetBindingVariableGetter(variable.variableType),
            //         ExpressionUtil.GetIntConstant(variable.index)
            //     );
            // }
            //
            // throw TemplateCompileException.UnknownAlias(aliasName);
        }

    }

}