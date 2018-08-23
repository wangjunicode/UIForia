using Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_BorderRadius_TopLeft : StyleBinding {

        private readonly Expression<float> expression;

        public StyleBinding_BorderRadius_TopLeft(StyleState state, Expression<float> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            float borderRadius = expression.EvaluateTyped(context);
            float currentBorderRadius = element.style.GetBorderRadiusTopLeft(state);
            if (borderRadius != currentBorderRadius) {
                element.style.SetBorderRadiusTopLeft(borderRadius, state);
            }
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.borderRadius = new BorderRadius(
                expression.EvaluateTyped(context),
                style.borderRadius.topRight,
                style.borderRadius.bottomRight,
                style.borderRadius.bottomLeft
            );
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderRadiusTopLeft(expression.EvaluateTyped(context), state);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}