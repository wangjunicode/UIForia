using Rendering;

namespace Src.StyleBindings.Src.StyleBindings {

    public class StyleBinding_MinHeight : StyleBinding {

        public readonly Expression<UIMeasurement> expression;

        public StyleBinding_MinHeight(StyleState state, Expression<UIMeasurement> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            UIMeasurement value = element.style.GetMinHeight(state);
            UIMeasurement newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetMinHeight(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.minHeight = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetMinHeight(expression.EvaluateTyped(context), state);
        }

    }

}