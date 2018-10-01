using Rendering;
using Src.Layout;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_FlexLayoutMainAxisAlignment : StyleBinding {

        private readonly Expression<MainAxisAlignment> expression;

        public StyleBinding_FlexLayoutMainAxisAlignment(StyleState state, Expression<MainAxisAlignment> expression)
            : base(RenderConstants.MainAxisAlignment, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state) || element.style.computedStyle.LayoutType != LayoutType.Flex) {
                return;
            }

            MainAxisAlignment alignment = element.style.computedStyle.FlexLayoutMainAxisAlignment;
            MainAxisAlignment newAlignment = expression.EvaluateTyped(context);
            if (alignment != newAlignment) {
                element.style.SetFlexMainAxisAlignment(newAlignment, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.FlexLayoutMainAxisAlignment = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetFlexMainAxisAlignment(expression.EvaluateTyped(context), state);
        }

    }

}