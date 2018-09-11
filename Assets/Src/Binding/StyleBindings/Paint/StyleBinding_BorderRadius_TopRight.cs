using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_BorderRadius_TopRight : StyleBinding {

        private readonly Expression<float> expression;

        public StyleBinding_BorderRadius_TopRight(StyleState state, Expression<float> expression) : base(RenderConstants.BorderRadiusTopRight, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            float borderRadius = expression.EvaluateTyped(context);
            float currentBorderRadius = element.style.GetBorderRadiusTopRight(state);
            if (borderRadius != currentBorderRadius) {
                element.style.SetBorderRadiusTopRight(borderRadius, state);
            }
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.borderRadius = new BorderRadius(
                style.borderRadius.topLeft,
                expression.EvaluateTyped(context),
                style.borderRadius.bottomRight,
                style.borderRadius.bottomLeft
            );
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderRadiusTopRight(expression.EvaluateTyped(context), state);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}