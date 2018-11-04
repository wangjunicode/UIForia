using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_MarginBottom : StyleBinding {

        private readonly Expression<UIMeasurement> expression;

        public StyleBinding_MarginBottom(StyleState state, Expression<UIMeasurement> expression) : base(RenderConstants.MarginBottom, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            UIMeasurement value = element.style.computedStyle.MarginBottom;
            UIMeasurement newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetMarginBottom(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.MarginBottom = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetMarginBottom(expression.EvaluateTyped(context), state);
        }

    }

}