using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_Padding : StyleBinding {

        private readonly Expression<FixedLengthRect> expression;

        public StyleBinding_Padding(StyleState state, Expression<FixedLengthRect> expression) : base(RenderConstants.Padding, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            FixedLengthRect value = element.style.computedStyle.padding;
            FixedLengthRect newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetPadding(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            FixedLengthRect padding = expression.EvaluateTyped(context);
            style.PaddingTop = padding.top;
            style.PaddingRight = padding.right;
            style.PaddingBottom = padding.bottom;
            style.PaddingLeft = padding.left;
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetPadding(expression.EvaluateTyped(context), state);
        }

    }

}