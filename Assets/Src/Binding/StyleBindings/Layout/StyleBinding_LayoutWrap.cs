using Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_LayoutWrap : StyleBinding {

        public readonly Expression<LayoutWrap> expression;

        public StyleBinding_LayoutWrap(StyleState state, Expression<LayoutWrap> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            LayoutWrap wrap = element.style.GetLayoutWrap(state);
            LayoutWrap newWrap = expression.EvaluateTyped(context);
            if (wrap != newWrap) {
                element.style.SetLayoutWrap(newWrap, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.layoutWrap = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetLayoutWrap(expression.EvaluateTyped(context), state);
        }

    }

}