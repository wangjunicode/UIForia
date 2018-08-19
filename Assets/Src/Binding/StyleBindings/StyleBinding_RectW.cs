using Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_RectW : StyleBinding {

        public readonly Expression<UIMeasurement> expression;

        public StyleBinding_RectW(StyleStateType state, Expression<UIMeasurement> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            UIMeasurement width = element.style.GetRectWidthForState(state);
            UIMeasurement newWidth = expression.EvaluateTyped(context);
            if (width != newWidth) {
                element.style.SetRectWidthForState(state, newWidth);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.rect.width = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetRectWidthForState(state, expression.EvaluateTyped(context));
        }

    }

    public class StyleBinding_RectH : StyleBinding {

        public readonly Expression<UIMeasurement> expression;

        public StyleBinding_RectH(StyleStateType state, Expression<UIMeasurement> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            UIMeasurement width = element.style.GetRectHeightForState(state);
            UIMeasurement newHeight = expression.EvaluateTyped(context);
            if (width != newHeight) {
                element.style.SetRectHeightForState(state, newHeight);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.rect.height = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetRectHeightForState(state, expression.EvaluateTyped(context));
        }

    }
}