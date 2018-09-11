using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_MarginLeft : StyleBinding {

        private readonly Expression<float> expression;

        public StyleBinding_MarginLeft(StyleState state, Expression<float> expression) : base(RenderConstants.MarginLeft, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            float value = element.style.GetMarginLeft(state);
            float newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetMarginLeft(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.margin.left = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetMarginLeft(expression.EvaluateTyped(context), state);
        }

    }

}