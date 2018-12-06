using System;
using UIForia.Parsing;

namespace UIForia.Compilers {

    public class ElementResolver : ExpressionAliasResolver {

        public ElementResolver(string aliasName) : base(aliasName) { }

        private static readonly UIElementExpression<UIElement> s_Expression = new UIElementExpression<UIElement>();

        public override Expression CompileAsValueExpression(ASTNode node, Func<Type, ASTNode, Expression> visit) {
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