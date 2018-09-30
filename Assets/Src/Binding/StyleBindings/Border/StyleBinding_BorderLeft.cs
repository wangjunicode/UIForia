using Rendering;
using Src.Rendering;

namespace Src.StyleBindings{

    public class StyleBinding_BorderLeft : StyleBinding {

        private readonly Expression<UIMeasurement> expression;

        public StyleBinding_BorderLeft(StyleState state, Expression<UIMeasurement> expression) : base(RenderConstants.BorderLeft, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            UIMeasurement value = element.style.computedStyle.BorderLeft;
            UIMeasurement newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetBorderLeft(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.BorderLeft = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderLeft(expression.EvaluateTyped(context), state);
        }

    }

}