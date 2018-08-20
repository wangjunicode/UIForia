using Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_RectHeight : StyleBinding {

        public readonly Expression<UIMeasurement> expression;

        public StyleBinding_RectHeight(StyleState state, Expression<UIMeasurement> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            UIMeasurement width = element.style.GetRectHeight(state);
            UIMeasurement newHeight = expression.EvaluateTyped(context);
            if (width != newHeight) {
                element.style.SetRectHeight(newHeight, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.rect.height = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetRectHeight(expression.EvaluateTyped(context), state);
        }

    }

}