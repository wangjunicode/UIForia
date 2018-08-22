using Rendering;

namespace Src.StyleBindings.Src.StyleBindings {

    public class StyleBinding_ShrinkFactor : StyleBinding {

        public readonly Expression<int> expression;

        public StyleBinding_ShrinkFactor(StyleState state, Expression<int> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            int value = element.style.GetShrinkFactor(state);
            int newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetShrinkFactor(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.layoutConstraints.shrinkFactor = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetShrinkFactor(expression.EvaluateTyped(context), state);
        }

    }

}