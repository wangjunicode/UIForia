using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_BorderRadius_BottomRight : StyleBinding {

        private readonly Expression<UIFixedLength> expression;

        public StyleBinding_BorderRadius_BottomRight(StyleState state, Expression<UIFixedLength> expression) : base(RenderConstants.BorderRadiusBottomRight, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;
            
            UIFixedLength borderRadius = expression.EvaluateTyped(context);
            UIFixedLength currentBorderRadius = element.style.computedStyle.BorderRadiusBottomRight;
            if (borderRadius != currentBorderRadius) {
                element.style.SetBorderRadiusBottomRight(borderRadius, state);
            }
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.BorderRadiusBottomRight = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderRadiusBottomRight(expression.EvaluateTyped(context), state);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}