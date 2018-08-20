using Rendering;

namespace Src.StyleBindings{

    public class StyleBinding_BorderLeft : StyleBinding {

        public readonly Expression<float> expression;

        public StyleBinding_BorderLeft(StyleState state, Expression<float> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            float value = element.style.GetBorderLeft(state);
            float newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetBorderLeft(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.border.left = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderLeft(expression.EvaluateTyped(context), state);
        }

    }

}