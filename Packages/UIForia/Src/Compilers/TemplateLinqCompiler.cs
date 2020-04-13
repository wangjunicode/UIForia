using System;
using System.Linq.Expressions;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Systems;

namespace UIForia.Compilers {

    public struct TemplateContextReference {

        public readonly ProcessedType processedType;
        public readonly TemplateNode templateNode;

        public TemplateContextReference(ProcessedType processedType, TemplateNode templateNode) {
            this.processedType = processedType;
            this.templateNode = templateNode;
        }

    }

    public class TemplateLinqCompiler : LinqCompiler {

        [ThreadStatic] private static ParameterExpression elementParam;

        private Expression castExpression;
        private ParameterExpression elementExpression;
        private ParameterExpression parentExpression;
        private ParameterExpression contextExpression;

        private readonly Parameter parameter;
        private TemplateLinqCompilerContext context;

        public TemplateLinqCompiler(TemplateLinqCompilerContext context) {
            this.context = context;
            this.resolveAlias = ResolveAlias;
            this.parameter = new Parameter(typeof(LinqBindingNode), "bindingNode", ParameterFlags.NeverNull);
        }

        public void Setup() {
            SetNamespaces(context.namespaces);
        }

        public void Init() {
            Reset();
            SetSignature(parameter);

            this.elementExpression = null;
            this.parentExpression = null;
            this.contextExpression = null;
        }

        public ParameterExpression GetRoot() {
            ref TemplateContextReference ctx = ref context.rootVariables.array[context.depth];
            switch (context.elementBindingType) {

                case ElementBindingType.Slot:
                    throw new NotImplementedException();
                    break;

                case ElementBindingType.Standard:
                    if (contextExpression == null) {
                        contextExpression = AddVariable(ctx.processedType.rawType, "context_" + context.depth, ParameterFlags.NeverNull);
                        Assign(contextExpression, Expression.TypeAs(Expression.Field(parameter.expression, MemberData.BindingNode_Root), ctx.processedType.rawType));
                    }

                    break;

                case ElementBindingType.Expanded:
                    if (contextExpression == null) {
                        contextExpression = AddVariable(ctx.processedType.rawType, "context_" + context.depth, ParameterFlags.NeverNull);
                        Assign(contextExpression, Expression.TypeAs(Expression.Field(parameter.expression, MemberData.BindingNode_Root), ctx.processedType.rawType));
                    }

                    break;

                case ElementBindingType.EntryPoint:
                    return GetElement();

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return contextExpression;
        }

        public ParameterExpression GetElement() {

            if (elementExpression == null) {
                elementExpression = AddVariable(context.elementType, "element", ParameterFlags.NeverNull);
                Assign(elementExpression, Expression.TypeAs(Expression.Field(parameter.expression, MemberData.BindingNode_Element), context.elementType));
            }

            return elementExpression;
        }

        public Expression GetBindingNode() {
            return parameter.expression;
        }

        public ProcessedType GetContextProcessedType() {
            return context.rootVariables.array[context.depth].processedType;
        }

        public Expression GetParent() {
            // should return the parent and skip any implicit elements
            // add a variable for parent type
            throw new NotImplementedException();
        }

        private Expression ResolveAlias(string aliasName, LinqCompiler _) {

            if (aliasName == "oldValue") {
                if (context.changeHandlerPreviousValue == null) {
                    throw new TemplateCompileException("Invalid use of $oldValue, this alias is only available when used inside of an onChange handler");
                }

                return context.changeHandlerPreviousValue;
            }

            if (aliasName == "newValue") {
                if (context.changeHandlerCurrentValue == null) {
                    throw new TemplateCompileException("Invalid use of $newValue, this alias is only available when used inside of an onChange handler");
                }

                return context.changeHandlerCurrentValue;
            }

            if (aliasName == "element") {
                return GetElement();
            }

            if (aliasName == "parent") {
                // todo -- should return the parent but ignore intrinsic elements like RepeatMulitChildContainer
                return GetParent();
            }

            if (aliasName == "evt") {
                if (context.currentEvent == null) {
                    throw new TemplateCompileException("Invalid use of $evt, this alias is only available when used inside of an event handler");
                }

                return context.currentEvent;
            }

            if (aliasName == "event") {
                if (context.currentEvent == null) {
                    throw new TemplateCompileException("Invalid use of $event, this alias is only available when used inside of an event handler");
                }

                return context.currentEvent;
            }

            if (aliasName == "root" || aliasName == "this") {
                return GetRoot();
            }

            if (context.TryGetBindingVariable(aliasName, out BindingVariableDesc variable)) {
                return ExpressionFactory.CallInstanceUnchecked(
                    GetBindingNode(),
                    AttributeCompiler.GetBindingVariableGetter(variable.variableType),
                    ExpressionUtil.GetIntConstant(variable.index)
                );
            }

            // ContextVariableDefinition contextVar = FindContextByName(aliasName);
            //
            // if (contextVar != null) {
            //     if (resolvingTypeOnly) {
            //         return contextVar.ResolveType(compiler);
            //     }
            //
            //     return contextVar.Resolve(compiler);
            // }

            throw TemplateCompileException.UnknownAlias(aliasName);
        }

    }

}