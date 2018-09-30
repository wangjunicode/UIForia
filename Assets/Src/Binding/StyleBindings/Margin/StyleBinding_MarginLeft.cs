using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_MarginLeft : StyleBinding {

        private readonly Expression<UIMeasurement> expression;

        public StyleBinding_MarginLeft(StyleState state, Expression<UIMeasurement> expression) : base(RenderConstants.MarginLeft, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            UIMeasurement value = element.style.computedStyle.MarginLeft;
            UIMeasurement newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetMarginLeft(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.MarginLeft = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetMarginLeft(expression.EvaluateTyped(context), state);
        }

    }

}