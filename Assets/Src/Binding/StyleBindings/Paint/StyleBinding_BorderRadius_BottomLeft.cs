using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_BorderRadius_BottomLeft : StyleBinding {

        private readonly Expression<float> expression;

        public StyleBinding_BorderRadius_BottomLeft(StyleState state, Expression<float> expression) : base(RenderConstants.BorderRadiusBottomLeft, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            float borderRadius = expression.EvaluateTyped(context);
            float currentBorderRadius = element.style.GetBorderRadiusBottomLeft(state);
            if (borderRadius != currentBorderRadius) {
                element.style.SetBorderRadiusBottomLeft(borderRadius, state);
            }
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.borderRadius = new BorderRadius(
                style.borderRadius.topLeft,
                style.borderRadius.topRight,
                style.borderRadius.bottomRight,
                expression.EvaluateTyped(context)
            );
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderRadiusBottomLeft(expression.EvaluateTyped(context), state);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}