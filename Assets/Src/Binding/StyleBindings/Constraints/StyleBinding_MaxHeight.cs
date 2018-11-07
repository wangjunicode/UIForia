using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_MaxHeight : StyleBinding {

        private readonly Expression<UIMeasurement> expression;

        public StyleBinding_MaxHeight(StyleState state, Expression<UIMeasurement> expression) : base(RenderConstants.MaxHeight, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            UIMeasurement value = element.style.GetMaxHeight(state);
            UIMeasurement newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetMaxHeight(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.MaxHeight = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetMaxHeight(expression.EvaluateTyped(context), state);
        }

    }

}