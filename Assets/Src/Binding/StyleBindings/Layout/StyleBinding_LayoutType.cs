using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_LayoutType : StyleBinding {

        private readonly Expression<LayoutType> expression;

        public StyleBinding_LayoutType(StyleState state, Expression<LayoutType> expression) : base(RenderConstants.LayoutType, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            LayoutType direction = element.style.computedStyle.LayoutType;
            LayoutType newType = expression.EvaluateTyped(context);
            if (direction != newType) {
                element.style.SetLayoutType(newType, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.LayoutType = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetLayoutType(expression.EvaluateTyped(context), state);
        }

    }

}