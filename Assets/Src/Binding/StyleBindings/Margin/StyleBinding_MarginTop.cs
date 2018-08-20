using Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_MarginTop : StyleBinding {

        public readonly Expression<float> expression;

        public StyleBinding_MarginTop(StyleState state, Expression<float> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            float value = element.style.GetMarginTop(state);
            float newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetMarginTop(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.margin.top = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetMarginTop(expression.EvaluateTyped(context), state);
        }

    }

}