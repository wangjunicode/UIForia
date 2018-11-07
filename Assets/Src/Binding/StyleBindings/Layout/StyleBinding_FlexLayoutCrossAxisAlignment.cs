using UIForia.Layout;
using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_FlexLayoutCrossAxisAlignment : StyleBinding {

        private readonly Expression<CrossAxisAlignment> expression;

        public StyleBinding_FlexLayoutCrossAxisAlignment(StyleState state, Expression<CrossAxisAlignment> expression) 
            : base(RenderConstants.CrossAxisAlignment, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state) || element.style.computedStyle.LayoutType != LayoutType.Flex) {
                return;
            }

            CrossAxisAlignment alignment = element.style.computedStyle.FlexLayoutCrossAxisAlignment;
            CrossAxisAlignment newAlignment = expression.EvaluateTyped(context);
            if (alignment != newAlignment) {
                element.style.SetFlexCrossAxisAlignment(newAlignment, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.FlexLayoutCrossAxisAlignment = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetFlexCrossAxisAlignment(expression.EvaluateTyped(context), state);
        }

    }

}