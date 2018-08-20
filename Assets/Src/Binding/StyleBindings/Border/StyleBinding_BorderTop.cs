using Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_BorderTop : StyleBinding {

        public readonly Expression<float> expression;

        public StyleBinding_BorderTop(StyleState state, Expression<float> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            float value = element.style.GetBorderTop(state);
            float newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetBorderTop(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.border.top = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderTop(expression.EvaluateTyped(context), state);
        }

    }

}