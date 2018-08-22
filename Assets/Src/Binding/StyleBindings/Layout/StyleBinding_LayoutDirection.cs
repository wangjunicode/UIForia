using Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_LayoutDirection : StyleBinding {

        public readonly Expression<LayoutDirection> expression;

        public StyleBinding_LayoutDirection(StyleState state, Expression<LayoutDirection> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            LayoutDirection direction = element.style.GetLayoutDirection(state);
            LayoutDirection newDirection = expression.EvaluateTyped(context);
            if (direction != newDirection) {
                element.style.SetLayoutDirection(newDirection, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.layoutParameters.direction = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetLayoutDirection(expression.EvaluateTyped(context), state);
        }

    }

}