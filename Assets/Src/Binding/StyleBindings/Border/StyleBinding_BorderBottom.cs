using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_BorderBottom : StyleBinding {

        private readonly Expression<UIFixedLength> expression;

        public StyleBinding_BorderBottom(StyleState state, Expression<UIFixedLength> expression) : base(RenderConstants.BorderBottom, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            UIFixedLength value = element.style.computedStyle.BorderBottom;
            UIFixedLength newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetBorderBottom(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.BorderBottom = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderBottom(expression.EvaluateTyped(context), state);
        }

    }

}