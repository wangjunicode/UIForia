using Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_LayoutType : StyleBinding {

        public readonly Expression<LayoutType> expression;

        public StyleBinding_LayoutType(StyleState state, Expression<LayoutType> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            LayoutType direction = element.style.GetLayoutType(state);
            LayoutType newType = expression.EvaluateTyped(context);
            if (direction != newType) {
                element.style.SetLayoutType(newType, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.layoutParameters.type = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetLayoutType(expression.EvaluateTyped(context), state);
        }

    }

}