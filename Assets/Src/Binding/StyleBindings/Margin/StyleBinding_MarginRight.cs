using Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_MarginRight : StyleBinding {

        public readonly Expression<float> expression;

        public StyleBinding_MarginRight(StyleState state, Expression<float> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            float value = element.style.GetMarginRight(state);
            float newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetMarginRight(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.margin.right = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetMarginRight(expression.EvaluateTyped(context), state);
        }

    }

}