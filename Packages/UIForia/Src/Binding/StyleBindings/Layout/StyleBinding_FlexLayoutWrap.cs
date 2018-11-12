using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public class StyleBinding_FlexLayoutWrap : StyleBinding {

        private readonly Expression<LayoutWrap> expression;

        public StyleBinding_FlexLayoutWrap(StyleState state, Expression<LayoutWrap> expression)
            : base(RenderConstants.LayoutWrap, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state) || element.style.computedStyle.LayoutType != LayoutType.Flex) {
                return;
            }

            LayoutWrap wrap = element.style.computedStyle.FlexLayoutWrap;
            LayoutWrap newWrap = expression.EvaluateTyped(context);
            if (wrap != newWrap) {
                element.style.SetFlexWrapMode(newWrap, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.FlexLayoutWrap = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetFlexWrapMode(expression.EvaluateTyped(context), state);
        }

    }

}