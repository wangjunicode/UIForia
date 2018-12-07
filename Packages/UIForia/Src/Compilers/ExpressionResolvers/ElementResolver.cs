using System;

namespace UIForia.Compilers {

    public class ElementResolver : ExpressionAliasResolver {

        public ElementResolver(string aliasName) : base(aliasName) { }

        private static readonly UIElementExpression s_Expression = new UIElementExpression();

        public override Expression CompileAsValueExpression(CompilerContext context) {
            return s_Expression;
        }

        public class UIElementExpression : Expression<UIElement> {

            public override Type YieldedType => typeof(UIElement);
       
            public override UIElement Evaluate(ExpressionContext context) {
                return (UIElement) context.currentObject;
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

}