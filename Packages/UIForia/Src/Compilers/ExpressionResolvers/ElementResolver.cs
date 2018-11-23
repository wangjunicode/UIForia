using System;

namespace UIForia.Compilers {

    public class ElementResolver : ExpressionAliasResolver {

        public ElementResolver(string aliasName) : base(aliasName) { }

        private static readonly UIElementExpression<UIElement> s_Expression = new UIElementExpression<UIElement>();

        public override Expression Compile(ContextDefinition context, ExpressionNode node, Func<ExpressionNode, Expression> visit) {
            
            if (node.expressionType == ExpressionNodeType.Accessor) {
                return s_Expression;
            }

            return null;
        }

        public class UIElementExpression<T> : Expression<T> {

            public override Type YieldedType => typeof(T);

            public override object Evaluate(ExpressionContext context) {
                return context.currentObject;
            }

            public override T EvaluateTyped(ExpressionContext context) {
                return (T) context.currentObject;
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

}