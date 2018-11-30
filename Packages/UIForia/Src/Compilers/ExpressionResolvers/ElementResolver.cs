using System;

namespace UIForia.Compilers {

    public class ElementResolver : ExpressionAliasResolver {

        public ElementResolver(string aliasName) : base(aliasName) { }

        private static readonly UIElementExpression<UIElement> s_Expression = new UIElementExpression<UIElement>();

        public override Expression CompileAsValueExpression(ContextDefinition context, ExpressionNodeOld nodeOld, Func<ExpressionNodeOld, Expression> visit) {
            
            if (nodeOld.expressionType == ExpressionNodeType.Accessor) {
                return s_Expression;
            }

            return null;
        }

        public class UIElementExpression<T> : Expression<T> {

            public override Type YieldedType => typeof(T);
       
            public override T Evaluate(ExpressionContext context) {
                return (T) context.currentObject;
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

}