using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_PaddingLeft : StyleBinding {

        public readonly Expression<UIFixedLength> expression;

        public StyleBinding_PaddingLeft(StyleState state, Expression<UIFixedLength> expression) : base(RenderConstants.PaddingLeft, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            UIFixedLength value = element.style.computedStyle.PaddingLeft;
            UIFixedLength newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetPaddingLeft(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.PaddingLeft = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetPaddingLeft(expression.EvaluateTyped(context), state);
        }

    }

}