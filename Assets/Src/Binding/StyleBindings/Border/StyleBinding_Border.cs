using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_Border : StyleBinding {

        private readonly Expression<PaddingBox> expression;

        public StyleBinding_Border(StyleState state, Expression<PaddingBox> expression) : base(RenderConstants.Border, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            PaddingBox value = element.style.computedStyle.border;
            PaddingBox newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetBorder(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            PaddingBox border = expression.EvaluateTyped(context);
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