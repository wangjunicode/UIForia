using Rendering;

namespace Src.StyleBindings.Src.StyleBindings {

    public class StyleBinding_MinWidth : StyleBinding {

        public readonly Expression<UIMeasurement> expression;

        public StyleBinding_MinWidth(StyleState state, Expression<UIMeasurement> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            UIMeasurement value = element.style.GetMinWidth(state);
            UIMeasurement newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetMinWidth(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.minWidth = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetMinWidth(expression.EvaluateTyped(context), state);
        }

    }

}