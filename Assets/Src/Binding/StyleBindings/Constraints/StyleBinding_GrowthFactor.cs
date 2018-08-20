using Rendering;

namespace Src.StyleBindings.Src.StyleBindings {

    public class StyleBinding_GrowthFactor : StyleBinding {

        public readonly Expression<int> expression;

        public StyleBinding_GrowthFactor(StyleState state, Expression<int> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            int value = element.style.GetGrowthFactor(state);
            int newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetGrowthFactor(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.growthFactor = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetGrowthFactor(expression.EvaluateTyped(context), state);
        }

    }

}