using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_FlexLayoutDirection : StyleBinding {

        private readonly Expression<LayoutDirection> expression;

        public StyleBinding_FlexLayoutDirection(StyleState state, Expression<LayoutDirection> expression)
            : base(RenderConstants.LayoutDirection, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state) || element.style.computedStyle.LayoutType != LayoutType.Flex) {
                return;
            }

            LayoutDirection direction = element.style.computedStyle.FlexLayoutDirection;
            LayoutDirection newDirection = expression.EvaluateTyped(context);
            if (direction != newDirection) {
                element.style.SetFlexDirection(newDirection, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.FlexLayoutDirection = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetFlexDirection(expression.EvaluateTyped(context), state);
        }

    }

}