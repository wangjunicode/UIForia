using Rendering;
using Src.Layout;

namespace Src.StyleBindings {

    public class StyleBinding_MainAxisAlignment : StyleBinding {

        public readonly Expression<MainAxisAlignment> expression;

        public StyleBinding_MainAxisAlignment(StyleState state, Expression<MainAxisAlignment> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            MainAxisAlignment alignment = element.style.GetMainAxisAlignment(state);
            MainAxisAlignment newAlignment = expression.EvaluateTyped(context);
            if (alignment != newAlignment) {
                element.style.SetMainAxisAlignment(newAlignment, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.layoutParameters.mainAxisAlignment = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetMainAxisAlignment(expression.EvaluateTyped(context), state);
        }

    }

}