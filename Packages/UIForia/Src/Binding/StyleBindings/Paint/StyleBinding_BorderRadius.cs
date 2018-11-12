using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_BorderRadius : StyleBinding {

        private readonly Expression<BorderRadius> expression;

        public StyleBinding_BorderRadius(StyleState state, Expression<BorderRadius> expression) : base(RenderConstants.BorderRadius, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            BorderRadius borderRadius = expression.EvaluateTyped(context);
            BorderRadius currentBorderRadius = element.style.GetBorderRadius(state);
            if (borderRadius != currentBorderRadius) {
                element.style.SetBorderRadius(borderRadius, state);
            }
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            BorderRadius borderRadius = expression.EvaluateTyped(context);
            style.BorderRadiusTopLeft = borderRadius.topLeft;
            style.BorderRadiusTopRight = borderRadius.topRight;
            style.BorderRadiusBottomLeft = borderRadius.bottomLeft;
            style.BorderRadiusBottomRight = borderRadius.bottomRight;
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderRadius(expression.EvaluateTyped(context), state);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}