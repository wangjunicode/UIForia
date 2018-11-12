using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_BorderRadius_TopRight : StyleBinding {

        private readonly Expression<UIFixedLength> expression;

        public StyleBinding_BorderRadius_TopRight(StyleState state, Expression<UIFixedLength> expression) : base(RenderConstants.BorderRadiusTopRight, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;
            
            UIFixedLength borderRadius = expression.EvaluateTyped(context);
            UIFixedLength currentBorderRadius = element.style.computedStyle.BorderRadiusTopRight;
            if (borderRadius != currentBorderRadius) {
                element.style.SetBorderRadiusTopRight(borderRadius, state);
            }
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.BorderRadiusTopRight = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderRadiusTopRight(expression.EvaluateTyped(context), state);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}