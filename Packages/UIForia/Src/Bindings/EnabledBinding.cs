using UIForia.Elements;
using UIForia.Expressions;
using UnityEngine;

namespace UIForia.Bindings {

    public class EnabledBinding : Binding {

        private readonly Expression<bool> expression;

        public EnabledBinding(Expression<bool> expression) : base("enabled") {
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            element.SetEnabled(expression.Evaluate(context));
            Debug.Log("Running Disable for " + element.ToString());
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}