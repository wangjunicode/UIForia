using System;
using UIForia.Input;

namespace UIForia.Compilers {

    public abstract class ExpressionAliasResolver {

        public string aliasName;

        public ExpressionAliasResolver(string aliasName) {
            if (aliasName[0] != '$') {
                aliasName = "$" + aliasName;
            }

            this.aliasName = aliasName;
        }

        public virtual bool CanCompile(ExpressionNode node) {
            return true;
        }

        public abstract Expression Compile(ContextDefinition context, ExpressionNode node, Func<ExpressionNode, Expression> visit);

    }

    public class MouseEventResolver : ExpressionAliasResolver {

        public MouseEventResolver(string aliasName) : base(aliasName) { }

        private static readonly MouseEventAliasExpression s_Expression = new MouseEventAliasExpression();
        
        public override Expression Compile(ContextDefinition context, ExpressionNode node, Func<ExpressionNode, Expression> visit) {
            if (node.expressionType == ExpressionNodeType.AliasAccessor) {
                return s_Expression;
            }

            return null;
        }

        public class MouseEventAliasExpression : Expression<MouseInputEvent> {

            public override Type YieldedType => typeof(MouseInputEvent);
            
            public override object Evaluate(ExpressionContext context) {
                UIElement element = (UIElement) context.currentObject;
                return element.view.Application.InputSystem.CurrentMouseEvent;
            }

            public override MouseInputEvent EvaluateTyped(ExpressionContext context) {
                UIElement element = (UIElement) context.currentObject;
                return element.view.Application.InputSystem.CurrentMouseEvent;
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }
    
    public class KeyboardEventResolver : ExpressionAliasResolver {

        public KeyboardEventResolver(string aliasName) : base(aliasName) { }

        private static readonly KeyboardEventAliasExpression s_Expression = new KeyboardEventAliasExpression();
        
        public override Expression Compile(ContextDefinition context, ExpressionNode node, Func<ExpressionNode, Expression> visit) {
            if (node.expressionType == ExpressionNodeType.AliasAccessor) {
                return s_Expression;
            }

            return null;
        }

        public class KeyboardEventAliasExpression : Expression<KeyboardInputEvent> {

            public override Type YieldedType => typeof(MouseInputEvent);
            
            public override object Evaluate(ExpressionContext context) {
                UIElement element = (UIElement) context.currentObject;
                return element.view.Application.InputSystem.CurrentKeyboardEvent;
            }

            public override KeyboardInputEvent EvaluateTyped(ExpressionContext context) {
                UIElement element = (UIElement) context.currentObject;
                return element.view.Application.InputSystem.CurrentKeyboardEvent;
            }

            public override bool IsConstant() {
                return false;
            }

        }

    }
}