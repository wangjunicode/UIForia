using System;

namespace UIForia.Compilers {

    public class ParentElementResolver : ExpressionAliasResolver {

        public ParentElementResolver(string aliasName) : base(aliasName) { }

        private static readonly UIParentElementExpression<UIElement> s_Expression = new UIParentElementExpression<UIElement>();

        public override Expression CompileAsValueExpression(ContextDefinition context, ExpressionNodeOld nodeOld, Func<ExpressionNodeOld, Expression> visit) {
            if (nodeOld.expressionType == ExpressionNodeType.Accessor) {
                return s_Expression;
            }

            if (nodeOld.expressionType == ExpressionNodeType.AliasAccessor) { }

            return null;
        }

        public class UIParentElementExpression<T> : Expression<T> where T : UIElement {

            public override Type YieldedType => typeof(T);

            public override T Evaluate(ExpressionContext context) {
                return (T) ((UIElement) context.currentObject).parent;
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

}