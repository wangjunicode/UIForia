using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UIForia.Systems;
using UIForia.Util;

namespace UIForia.Compilers {

    public class TemplateLinqCompiler : LinqCompiler {

        private struct RootContext {

            public Type type;
            public ParameterExpression expression;
            public IList<string> namespaces;

        }

        [ThreadStatic] private static ParameterExpression elementParam;

        private Expression castExpression;
        private ParameterExpression elementExpression;

        private readonly Parameter parameter;
        private int depth;
        public Type elementType;
        private LightList<RootContext> rootVariables;
        private bool slotSetup;

        public TemplateLinqCompiler() {
            this.parameter = new Parameter(typeof(LinqBindingNode), "bindingNode", ParameterFlags.NeverNull);
        }

        public void Init(Type elementType, ReadOnlySizedArray<Type> contexts) {
            Reset();
            SetSignature(parameter);
            rootVariables = rootVariables ?? new LightList<RootContext>();
            rootVariables.Clear();
            for (int i = 0; i < contexts.size; i++) {
                rootVariables.Add(new RootContext() {
                    type = contexts.array[i],
                    namespaces = default, // todo
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
            ref RootContext ctx = ref rootVariables.array[depth];
            if (slotSetup) {
                if (ctx.expression == null) {
                    // ctx.expression = Expression.Parameter()
                    throw new NotImplementedException();
                }
            }
            else {
                if (ctx.expression == null) {
                    ctx.expression = AddVariable(ctx.type, "context_" + depth, ParameterFlags.NeverNull);
                    Assign(ctx.expression, Expression.TypeAs(Expression.Field(parameter.expression, MemberData.BindingNode_Root), ctx.type));
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

        public void RestoreImplicitContext() {
            SetImplicitContext(GetRoot());
        }

        public Expression GetBindingNode() {
            return parameter.expression;
        }

    }

}