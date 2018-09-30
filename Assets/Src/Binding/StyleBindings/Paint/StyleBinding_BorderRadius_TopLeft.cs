using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_BorderRadius_TopLeft : StyleBinding {

        private readonly Expression<UIMeasurement> expression;

        public StyleBinding_BorderRadius_TopLeft(StyleState state, Expression<UIMeasurement> expression) : base(RenderConstants.BorderRadiusTopLeft, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;
            
            UIMeasurement borderRadius = expression.EvaluateTyped(context);
            UIMeasurement currentBorderRadius = element.style.computedStyle.RareData.borderRadiusTopLeft;
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