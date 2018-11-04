using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_MarginRight : StyleBinding {

        private readonly Expression<UIMeasurement> expression;

        public StyleBinding_MarginRight(StyleState state, Expression<UIMeasurement> expression) : base(RenderConstants.MarginRight, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            UIMeasurement value = element.style.computedStyle.MarginRight;
            UIMeasurement newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetMarginRight(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.MarginRight = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetMarginRight(expression.EvaluateTyped(context), state);
        }

    }

}