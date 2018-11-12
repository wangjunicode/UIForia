using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_PaddingBottom : StyleBinding {

        private readonly Expression<UIFixedLength> expression;

        public StyleBinding_PaddingBottom(StyleState state, Expression<UIFixedLength> expression) : base(RenderConstants.PaddingBottom, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            UIFixedLength value = element.style.computedStyle.PaddingBottom;
            UIFixedLength newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetPaddingBottom(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.PaddingBottom = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetPaddingBottom(expression.EvaluateTyped(context), state);
        }

    }

}