using System;
using UIForia.Elements;
using UIForia.Expressions;

namespace UIForia.Compilers.ExpressionResolvers {

    public class ParentElementResolver : ExpressionAliasResolver {

        public ParentElementResolver(string aliasName) : base(aliasName) { }

        private static readonly UIParentElementExpression<UIElement> s_Expression = new UIParentElementExpression<UIElement>();

        public override Expression CompileAsValueExpression(CompilerContext context) {
            return s_Expression;
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