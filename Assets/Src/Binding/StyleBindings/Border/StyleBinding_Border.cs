using Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_Border : StyleBinding {

        public readonly Expression<ContentBoxRect> expression;

        public StyleBinding_Border(StyleState state, Expression<ContentBoxRect> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            ContentBoxRect value = element.style.GetBorder(state);
            ContentBoxRect newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetBorder(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.border = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorder(expression.EvaluateTyped(context), state);
        }

    }

}