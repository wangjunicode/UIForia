using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_Margin : StyleBinding {

        private readonly Expression<ContentBoxRect> expression;

        public StyleBinding_Margin(StyleState state, Expression<ContentBoxRect> expression) : base(RenderConstants.Margin, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            ContentBoxRect value = element.style.GetMargin(state);
            ContentBoxRect newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetMargin(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.margin = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetMargin(expression.EvaluateTyped(context), state);
        }

    }

}