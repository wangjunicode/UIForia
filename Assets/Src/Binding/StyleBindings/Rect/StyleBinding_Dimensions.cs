using Rendering;
using Src.Rendering;

namespace Src.StyleBindings {

    public class StyleBinding_Dimensions : StyleBinding {

        private readonly Expression<Dimensions> expression;

        public StyleBinding_Dimensions(StyleState state, Expression<Dimensions> expression) : base(RenderConstants.Size, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;
            Dimensions size = element.style.GetDimensions(state);
            Dimensions newSize = expression.EvaluateTyped(context);
            if (size.width != newSize.width || size.height != newSize.height) {
                element.style.SetDimensions(newSize, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.dimensions = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetDimensions(expression.EvaluateTyped(context), state);
        }

    }

}