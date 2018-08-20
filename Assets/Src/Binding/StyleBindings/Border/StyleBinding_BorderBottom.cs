using Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_BorderBottom : StyleBinding {

        public readonly Expression<float> expression;

        public StyleBinding_BorderBottom(StyleState state, Expression<float> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            float value = element.style.GetBorderBottom(state);
            float newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetBorderBottom(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.border.bottom = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderBottom(expression.EvaluateTyped(context), state);
        }

    }

}