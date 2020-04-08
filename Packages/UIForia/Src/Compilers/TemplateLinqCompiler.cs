using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UIForia.Parsing;
using UIForia.Systems;
using UIForia.Util;

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

        private struct ContextReference {

            public ProcessedType type;
            public ParameterExpression expression;
            public IList<string> namespaces;

        }

        [ThreadStatic] private static ParameterExpression elementParam;

        private Expression castExpression;
        private ParameterExpression elementExpression;

        private readonly Parameter parameter;
        private int depth;
        public Type elementType;
        private LightList<ContextReference> rootVariables;
        private bool slotSetup;

        public TemplateLinqCompiler() {
            this.parameter = new Parameter(typeof(LinqBindingNode), "bindingNode", ParameterFlags.NeverNull);
        }

        public void Init(Type elementType, ReadOnlySizedArray<TemplateContextReference> contexts) {
            Reset();
            SetSignature(parameter);
            rootVariables = rootVariables ?? new LightList<ContextReference>();
            rootVariables.Clear();
            for (int i = 0; i < contexts.size; i++) {
                rootVariables.Add(new ContextReference() {
                    type = contexts.array[i].processedType,
                    namespaces = contexts.array[i].templateNode.root.templateShell.referencedNamespaces,
                });
            }

            this.elementType = elementType;
            this.elementExpression = null;
        }

        public void Setup(in AttrInfo attrInfo) {
            depth = attrInfo.depth;
            SetNamespaces(rootVariables.array[depth].namespaces);
        }

        public ParameterExpression GetRoot() {
            ref ContextReference ctx = ref rootVariables.array[depth];
            if (slotSetup) {
                if (ctx.expression == null) {
                    // ctx.expression = Expression.Parameter()
                    throw new NotImplementedException();
                }
            }
            else {
                if (ctx.expression == null) {
                    ctx.expression = AddVariable(ctx.type.rawType, "context_" + depth, ParameterFlags.NeverNull);
                    Assign(ctx.expression, Expression.TypeAs(Expression.Field(parameter.expression, MemberData.BindingNode_Root), ctx.type.rawType));
                }
            }

            return ctx.expression;
        }

        public ParameterExpression GetElement() {

            if (elementExpression == null) {
                elementExpression = AddVariable(elementType, "element", ParameterFlags.NeverNull);
                Assign(elementExpression, Expression.TypeAs(Expression.Field(parameter.expression, MemberData.BindingNode_Element), elementType));
            }

            return elementExpression;
        }
        
        public Expression GetBindingNode() {
            return parameter.expression;
        }

        public ProcessedType GetContextProcessedType() {
            return rootVariables[depth].type;
        }

    }

}