using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_PaddingBottom : StyleBinding {

        private readonly Expression<UIMeasurement> expression;

        public StyleBinding_PaddingBottom(StyleState state, Expression<UIMeasurement> expression) : base(RenderConstants.PaddingBottom, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            UIMeasurement value = element.style.computedStyle.PaddingBottom;
            UIMeasurement newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetPaddingBottom(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.PaddingBottom = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetPaddingBottom(expression.EvaluateTyped(context), state);
        }

    }

}