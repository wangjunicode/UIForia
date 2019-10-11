using System;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.UIInput;

namespace UIForia.Compilers.ExpressionResolvers {

    public class KeyboardEventResolver : ExpressionAliasResolver {

        public KeyboardEventResolver(string aliasName) : base(aliasName) { }

        private static readonly KeyboardEventAliasExpression s_Expression = new KeyboardEventAliasExpression();

        public override Expression CompileAsValueExpression(CompilerContext context) {
            return s_Expression;
        }

        public class KeyboardEventAliasExpression : Expression<KeyboardInputEvent> {

            public override Type YieldedType => typeof(KeyboardInputEvent);


            public override KeyboardInputEvent Evaluate(ExpressionContext context) {
                UIElement element = (UIElement) context.currentObject;
                return element.View.application.InputSystem.CurrentKeyboardEvent;
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

}