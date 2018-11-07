using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_PaddingTop : StyleBinding {

        public readonly Expression<UIFixedLength> expression;

        public StyleBinding_PaddingTop(StyleState state, Expression<UIFixedLength> expression) : base(RenderConstants.PaddingTop, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            UIFixedLength value = element.style.computedStyle.PaddingTop;
            UIFixedLength newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetPaddingTop(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.PaddingTop = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetPaddingTop(expression.EvaluateTyped(context), state);
        }

    }

}