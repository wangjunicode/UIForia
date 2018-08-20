using Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_MarginBottom : StyleBinding {

        public readonly Expression<float> expression;

        public StyleBinding_MarginBottom(StyleState state, Expression<float> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            float value = element.style.GetMarginBottom(state);
            float newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetMarginBottom(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.margin.bottom = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetMarginBottom(expression.EvaluateTyped(context), state);
        }

    }

}