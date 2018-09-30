using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_BorderRadius_BottomLeft : StyleBinding {

        private readonly Expression<UIMeasurement> expression;

        public StyleBinding_BorderRadius_BottomLeft(StyleState state, Expression<UIMeasurement> expression) : base(RenderConstants.BorderRadiusBottomLeft, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;
            
            UIMeasurement borderRadius = expression.EvaluateTyped(context);
            UIMeasurement currentBorderRadius = element.style.computedStyle.RareData.borderRadiusBottomLeft;
            if (borderRadius != currentBorderRadius) {
                element.style.SetBorderRadiusBottomLeft(borderRadius, state);
            }
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.BorderRadiusBottomLeft = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderRadiusBottomLeft(expression.EvaluateTyped(context), state);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}