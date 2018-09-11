using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_PaddingRight : StyleBinding {

        public readonly Expression<float> expression;

        public StyleBinding_PaddingRight(StyleState state, Expression<float> expression) : base(RenderConstants.PaddingRight, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            float value = element.style.GetPaddingRight(state);
            float newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetPaddingRight(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.padding.right = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetPaddingRight(expression.EvaluateTyped(context), state);
        }

    }

}