using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_BorderTop : StyleBinding {

        private readonly Expression<UIFixedLength> expression;

        public StyleBinding_BorderTop(StyleState state, Expression<UIFixedLength> expression) : base(RenderConstants.BorderTop, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            UIFixedLength value = element.style.computedStyle.BorderTop;
            UIFixedLength newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetBorderTop(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.BorderTop = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderTop(expression.EvaluateTyped(context), state);
        }

    }

}