using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_BorderRight : StyleBinding {

        private readonly Expression<UIFixedLength> expression;

        public StyleBinding_BorderRight(StyleState state, Expression<UIFixedLength> expression) : base(RenderConstants.BorderRight, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            UIFixedLength value = element.style.computedStyle.BorderRight;
            UIFixedLength newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetBorderRight(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.BorderRight = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderRight(expression.EvaluateTyped(context), state);
        }

    }

}