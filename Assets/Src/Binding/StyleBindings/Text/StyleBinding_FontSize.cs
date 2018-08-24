using Rendering;

namespace Src.StyleBindings.Text {

    public class StyleBinding_FontSize : StyleBinding {

        private readonly Expression<int> expression;

        public StyleBinding_FontSize(StyleState state, Expression<int> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.text.fontSize = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            int currentSize = styleSet.GetFontSize(state);
            int newSize = expression.EvaluateTyped(context);
            if (currentSize != newSize) {
                styleSet.SetFontSize(newSize, state);
            }
        }

    }

}