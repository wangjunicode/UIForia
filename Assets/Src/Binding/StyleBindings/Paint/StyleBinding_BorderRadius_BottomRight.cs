using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_BorderRadius_BottomRight : StyleBinding {

        private readonly Expression<float> expression;

        public StyleBinding_BorderRadius_BottomRight(StyleState state, Expression<float> expression) : base(RenderConstants.BorderRadiusBottomRight, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            float borderRadius = expression.EvaluateTyped(context);
            float currentBorderRadius = element.style.GetBorderRadiusBottomRight(state);
            if (borderRadius != currentBorderRadius) {
                element.style.SetBorderRadiusBottomRight(borderRadius, state);
            }
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.borderRadius = new BorderRadius(
                style.borderRadius.topLeft,
                style.borderRadius.topRight,
                expression.EvaluateTyped(context),
                style.borderRadius.bottomLeft
            );
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderRadiusBottomRight(expression.EvaluateTyped(context), state);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}