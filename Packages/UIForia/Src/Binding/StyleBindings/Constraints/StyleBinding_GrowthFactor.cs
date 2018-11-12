using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_GrowthFactor : StyleBinding {

        private readonly Expression<int> expression;

        public StyleBinding_GrowthFactor(StyleState state, Expression<int> expression) : base(RenderConstants.GrowthFactor, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            int value = element.style.computedStyle.FlexItemGrow;
            int newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetFlexItemGrowFactor(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.FlexItemGrow = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetFlexItemGrowFactor(expression.EvaluateTyped(context), state);
        }

    }

}