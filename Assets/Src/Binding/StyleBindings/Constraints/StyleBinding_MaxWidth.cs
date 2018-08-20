using Rendering;

namespace Src.StyleBindings.Src.StyleBindings {

    public class StyleBinding_MaxWidth : StyleBinding {

        public readonly Expression<UIMeasurement> expression;

        public StyleBinding_MaxWidth(StyleState state, Expression<UIMeasurement> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            UIMeasurement value = element.style.GetMaxWidth(state);
            UIMeasurement newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetMaxWidth(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.maxWidth = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetMaxWidth(expression.EvaluateTyped(context), state);
        }

    }

}