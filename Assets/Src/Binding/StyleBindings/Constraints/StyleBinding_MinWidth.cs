using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_MinWidth : StyleBinding {

        private readonly Expression<UIMeasurement> expression;

        public StyleBinding_MinWidth(StyleState state, Expression<UIMeasurement> expression) : base(RenderConstants.MinWidth, state) {
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
            style.MinWidth = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetMinWidth(expression.EvaluateTyped(context), state);
        }

    }

}