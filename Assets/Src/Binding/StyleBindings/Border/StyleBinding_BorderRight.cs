using Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_BorderRight : StyleBinding {

        public readonly Expression<float> expression;

        public StyleBinding_BorderRight(StyleState state, Expression<float> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            float value = element.style.GetBorderRight(state);
            float newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetBorderRight(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.border.right = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderRight(expression.EvaluateTyped(context), state);
        }

    }

}