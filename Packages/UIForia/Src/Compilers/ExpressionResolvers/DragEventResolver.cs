using System;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.UIInput;

namespace UIForia.Compilers.ExpressionResolvers {

    public class DragEventResolver : ExpressionAliasResolver {

        public DragEventResolver(string aliasName) : base(aliasName) { }

        private static readonly DragEventAliasExpression s_Expression = new DragEventAliasExpression();

        public override Expression CompileAsValueExpression(CompilerContext context) {
            return s_Expression;
        }

        public class DragEventAliasExpression : Expression<DragEvent> {

            public override Type YieldedType => typeof(DragEvent);

            public override DragEvent Evaluate(ExpressionContext context) {
                UIElement element = (UIElement) context.currentObject;
                return element.View.Application.InputSystem.CurrentDragEvent;
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }

}