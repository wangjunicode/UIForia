using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_Margin : StyleBinding {

        private readonly Expression<ContentBoxRect> expression;

        public StyleBinding_Margin(StyleState state, Expression<ContentBoxRect> expression) : base(RenderConstants.Margin, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            ContentBoxRect value = element.style.computedStyle.margin;
            ContentBoxRect newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetMargin(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            ContentBoxRect margin = expression.EvaluateTyped(context);
            style.MarginTop = margin.top;
            style.MarginRight = margin.right;
            style.MarginBottom = margin.bottom;
            style.MarginLeft = margin.left;
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetMargin(expression.EvaluateTyped(context), state);
        }

    }

}