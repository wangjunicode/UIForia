using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_PaddingRight : StyleBinding {

        public readonly Expression<UIMeasurement> expression;

        public StyleBinding_PaddingRight(StyleState state, Expression<UIMeasurement> expression) : base(RenderConstants.PaddingRight, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            UIMeasurement value = element.style.computedStyle.PaddingRight;
            UIMeasurement newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetPaddingRight(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.PaddingRight = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetPaddingRight(expression.EvaluateTyped(context), state);
        }

    }

}