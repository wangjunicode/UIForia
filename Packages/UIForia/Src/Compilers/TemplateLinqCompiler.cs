using System;
using System.Linq.Expressions;
using UIForia.Systems;

namespace UIForia.Compilers {

    public class TemplateLinqCompiler : LinqCompiler {

        [ThreadStatic] private static ParameterExpression elementParam;

        private Expression castExpression;
        private ParameterExpression elementExpression;

        private AttributeContext attributeContext;
        private readonly Parameter parameter;
        private int depth;
        private int depthLevels;
        private Type contextType;
        public Type elementType;
        
        public TemplateLinqCompiler() {
            this.parameter = new Parameter(typeof(LinqBindingNode), "bindingNode", ParameterFlags.NeverNull);
        }
        
        public void Setup(AttributeContext context, int depth) {
            this.depth = depth;
            this.depthLevels = context.size;

            contextType = depth == 0 
                ? context.rootType 
                : context.referencedTypes[depth - 1];

            Reset();
            SetSignature(parameter);
            SetNamespaces(context.GetNamespaces(depth));
        }

        public ParameterExpression GetRoot() {
            // attribute set is a grouping of attributes with their context data.
            // we loop through each set of attributes and call setup on the compilers
            // depending on the depth of the current setup, GetRoot()
            // will return one of: 
            // the highest template
            // the lowest template
            // slotContexts[depth]
            // and cast appropriately.
            
            if (depth == 0) {
                RawExpression(Expression.TypeAs(Expression.Field(parameter.expression, MemberData.BindingNode_Root), contextType));
            }

            return null;

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

    }

}