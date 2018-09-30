using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_Height : StyleBinding {

        private readonly Expression<UIMeasurement> expression;

        public StyleBinding_Height(StyleState state, Expression<UIMeasurement> expression) : base(RenderConstants.Height, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            UIMeasurement height = element.style.computedStyle.PreferredHeight;
            UIMeasurement newHeight = expression.EvaluateTyped(context);
            
            if (height != newHeight) {
                element.style.SetPreferredHeight(newHeight, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.PreferredHeight = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetPreferredHeight(expression.EvaluateTyped(context), state);
        }

    }

}