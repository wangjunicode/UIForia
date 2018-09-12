using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_Width : StyleBinding {

        private readonly Expression<UIMeasurement> expression;

        public StyleBinding_Width(StyleState state, Expression<UIMeasurement> expression) : base(RenderConstants.Width, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            UIMeasurement width = element.style.GetWidth(state);
            UIMeasurement newWidth = expression.EvaluateTyped(context);
            if (width != newWidth) {
                element.style.SetWidth(newWidth, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.dimensions.width = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetWidth(expression.EvaluateTyped(context), state);
        }

    }

}