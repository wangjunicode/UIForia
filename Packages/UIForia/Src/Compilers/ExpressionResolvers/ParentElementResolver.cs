using System;
using UIForia.Parsing;

namespace UIForia.Compilers {

    public class ParentElementResolver : ExpressionAliasResolver {

        public ParentElementResolver(string aliasName) : base(aliasName) { }

        private static readonly UIParentElementExpression<UIElement> s_Expression = new UIParentElementExpression<UIElement>();

        public override Expression CompileAsValueExpression(ASTNode node, Func<ASTNode, Expression> visit) {
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