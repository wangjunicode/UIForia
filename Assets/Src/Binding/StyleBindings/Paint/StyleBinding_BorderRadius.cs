using Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_BorderRadius : StyleBinding {

        private readonly Expression<BorderRadius> expression;

        public StyleBinding_BorderRadius(StyleState state, Expression<BorderRadius> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            BorderRadius borderRadius = expression.EvaluateTyped(context);
            BorderRadius currentBorderRadius = element.style.GetBorderRadius(state);
            if (borderRadius != currentBorderRadius) {
                element.style.SetBorderRadius(borderRadius, state);
            }
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.borderRadius = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderRadius(expression.EvaluateTyped(context), state);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}