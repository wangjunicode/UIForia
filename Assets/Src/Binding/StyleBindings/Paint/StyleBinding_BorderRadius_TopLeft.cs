using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_BorderRadius_TopLeft : StyleBinding {

        private readonly Expression<UIFixedLength> expression;

        public StyleBinding_BorderRadius_TopLeft(StyleState state, Expression<UIFixedLength> expression) : base(RenderConstants.BorderRadiusTopLeft, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;
            
            UIFixedLength borderRadius = expression.EvaluateTyped(context);
            UIFixedLength currentBorderRadius = element.style.computedStyle.BorderRadiusTopLeft;
            if (borderRadius != currentBorderRadius) {
                element.style.SetBorderRadiusTopLeft(borderRadius, state);
            }
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.BorderRadiusTopLeft = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderRadiusTopLeft(expression.EvaluateTyped(context), state);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}