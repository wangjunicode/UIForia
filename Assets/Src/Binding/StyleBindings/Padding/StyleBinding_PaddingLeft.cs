using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_PaddingLeft : StyleBinding {

        public readonly Expression<float> expression;

        public StyleBinding_PaddingLeft(StyleState state, Expression<float> expression) : base(RenderConstants.PaddingLeft, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            float value = element.style.GetPaddingLeft(state);
            float newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetPaddingLeft(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.padding.left = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetPaddingLeft(expression.EvaluateTyped(context), state);
        }

    }

}