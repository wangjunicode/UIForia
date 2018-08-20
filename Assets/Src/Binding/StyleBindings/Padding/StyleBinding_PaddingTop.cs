using Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_PaddingTop : StyleBinding {

        public readonly Expression<float> expression;

        public StyleBinding_PaddingTop(StyleState state, Expression<float> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            float value = element.style.GetPaddingTop(state);
            float newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetPaddingTop(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.padding.top = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetPaddingTop(expression.EvaluateTyped(context), state);
        }

    }

}