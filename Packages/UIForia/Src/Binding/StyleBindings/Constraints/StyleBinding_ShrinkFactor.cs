using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_ShrinkFactor : StyleBinding {

        private readonly Expression<int> expression;

        public StyleBinding_ShrinkFactor(StyleState state, Expression<int> expression) : base(RenderConstants.ShrinkFactor, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            int value = element.style.computedStyle.FlexItemShrink;
            int newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetFlexItemShrinkFactor(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.FlexItemShrink = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetFlexItemShrinkFactor(expression.EvaluateTyped(context), state);
        }

    }

}