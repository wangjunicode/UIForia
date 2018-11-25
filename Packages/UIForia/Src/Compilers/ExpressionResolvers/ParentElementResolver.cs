using System;

namespace UIForia.Compilers {

    public class ParentElementResolver : ExpressionAliasResolver {

        public ParentElementResolver(string aliasName) : base(aliasName) { }

        private static readonly UIParentElementExpression<UIElement> s_Expression = new UIParentElementExpression<UIElement>();

        public override Expression CompileAsValueExpression(ContextDefinition context, ExpressionNode node, Func<ExpressionNode, Expression> visit) {
            if (node.expressionType == ExpressionNodeType.Accessor) {
                return s_Expression;
            }

            if (node.expressionType == ExpressionNodeType.AliasAccessor) { }

            return null;
        }

        public class UIParentElementExpression<T> : Expression<T> where T : UIElement {

            public override Type YieldedType => typeof(T);

            public override object Evaluate(ExpressionContext context) {
                return ((UIElement) context.currentObject).parent;
            }

            public override T EvaluateTyped(ExpressionContext context) {
                return (T) ((UIElement) context.currentObject).parent;
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

}