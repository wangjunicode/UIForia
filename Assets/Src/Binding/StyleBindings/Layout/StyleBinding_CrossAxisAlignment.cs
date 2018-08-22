using Rendering;
using Src.Layout;

namespace Src.StyleBindings {

    public class StyleBinding_CrossAxisAlignment : StyleBinding {

        public readonly Expression<CrossAxisAlignment> expression;

        public StyleBinding_CrossAxisAlignment(StyleState state, Expression<CrossAxisAlignment> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            CrossAxisAlignment alignment = element.style.GetCrossAxisAlignment(state);
            CrossAxisAlignment newAlignment = expression.EvaluateTyped(context);
            if (alignment != newAlignment) {
                element.style.SetCrossAxisAlignment(newAlignment, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.layoutParameters.crossAxisAlignment = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetCrossAxisAlignment(expression.EvaluateTyped(context), state);
        }

    }

}