using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_Padding : StyleBinding {

        private readonly Expression<ContentBoxRect> expression;

        public StyleBinding_Padding(StyleState state, Expression<ContentBoxRect> expression) : base(RenderConstants.Padding, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            ContentBoxRect value = element.style.GetPadding(state);
            ContentBoxRect newValue = expression.EvaluateTyped(context);
            if (value != newValue) {
                element.style.SetPadding(value, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.padding = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetPadding(expression.EvaluateTyped(context), state);
        }

    }

}