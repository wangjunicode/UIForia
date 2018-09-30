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

            UIMeasurement preferredWidth = element.style.computedStyle.PreferredWidth;
            UIMeasurement preferredHeight= element.style.computedStyle.PreferredHeight;
            
            Dimensions newSize = expression.EvaluateTyped(context);
            
            if (preferredWidth != newSize.width) {
                element.style.SetPreferredWidth(preferredWidth, state);
            }

            if (preferredHeight != newSize.height) {
                element.style.SetPreferredHeight(newSize.height, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            Dimensions dimensions = expression.EvaluateTyped(context);
            style.PreferredWidth = dimensions.width;
            style.PreferredHeight = dimensions.height;
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            Dimensions size = expression.EvaluateTyped(context);
            styleSet.SetPreferredWidth(size.width, state);
            styleSet.SetPreferredHeight(size.height, state);
        }

    }

}