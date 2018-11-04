using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_Border : StyleBinding {

        private readonly Expression<FixedLengthRect> expression;

        public StyleBinding_Border(StyleState state, Expression<FixedLengthRect> expression) : base(RenderConstants.Border, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            FixedLengthRect value = element.style.computedStyle.border;
            FixedLengthRect newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetBorder(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            FixedLengthRect border = expression.EvaluateTyped(context);
            style.BorderTop = border.top;
            style.BorderRight = border.right;
            style.BorderBottom = border.bottom;
            style.BorderLeft = border.left;
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorder(expression.EvaluateTyped(context), state);
        }

    }

}