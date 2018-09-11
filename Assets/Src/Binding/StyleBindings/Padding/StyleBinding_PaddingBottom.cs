using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_PaddingBottom : StyleBinding {

        private readonly Expression<float> expression;

        public StyleBinding_PaddingBottom(StyleState state, Expression<float> expression) : base(RenderConstants.PaddingBottom, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            float value = element.style.GetPaddingBottom(state);
            float newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetPaddingBottom(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.padding.bottom = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetPaddingBottom(expression.EvaluateTyped(context), state);
        }

    }

}