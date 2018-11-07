using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_MarginTop : StyleBinding {

        private readonly Expression<UIMeasurement> expression;

        public StyleBinding_MarginTop(StyleState state, Expression<UIMeasurement> expression) : base(RenderConstants.MarginTop, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            UIMeasurement value = element.style.computedStyle.MarginTop;
            UIMeasurement newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetMarginTop(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.MarginTop = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetMarginTop(expression.EvaluateTyped(context), state);
        }

    }

}