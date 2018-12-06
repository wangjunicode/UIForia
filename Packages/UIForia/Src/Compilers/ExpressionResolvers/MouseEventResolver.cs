using System;
using UIForia.Input;
using UIForia.Parsing;

namespace UIForia.Compilers {

    public class MouseEventResolver : ExpressionAliasResolver {

        public MouseEventResolver(string aliasName) : base(aliasName) { }

        private static readonly MouseEventAliasExpression s_Expression = new MouseEventAliasExpression();

//        public override Expression CompileAsValueExpression(ASTNode nodeOld, Func<Type, ASTNode, Expression> visit) {
//            return s_Expression;
//        }

        public class MouseEventAliasExpression : Expression<MouseInputEvent> {

            public override Type YieldedType => typeof(MouseInputEvent);

            public override MouseInputEvent Evaluate(ExpressionContext context) {
                UIElement element = (UIElement) context.currentObject;
                return element.view.Application.InputSystem.CurrentMouseEvent;
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

}