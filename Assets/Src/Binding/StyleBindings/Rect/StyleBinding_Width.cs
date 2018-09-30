using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_Width : StyleBinding {

        private readonly Expression<UIMeasurement> expression;

        public StyleBinding_Width(StyleState state, Expression<UIMeasurement> expression) : base(RenderConstants.Width, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            UIMeasurement width = element.style.computedStyle.PreferredWidth;
            UIMeasurement newWidth = expression.EvaluateTyped(context);
            if (width != newWidth) {
                element.style.SetPreferredWidth(newWidth, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.PreferredWidth = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetPreferredWidth(expression.EvaluateTyped(context), state);
        }

    }

}