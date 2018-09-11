using Rendering;
using Src.Rendering;
using UnityEngine;

namespace Src.StyleBindings.Text {

    public class StyleBinding_TextColor : StyleBinding {

        private readonly Expression<Color> expression;

        public StyleBinding_TextColor(StyleState state, Expression<Color> expression) : base(RenderConstants.TextColor, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            Color currentColor = element.style.GetTextColor(state);
            Color newColor = expression.EvaluateTyped(context);
            if (newColor != currentColor) {
                element.style.SetTextColor(newColor, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.textStyle.color = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            Color currentColor = styleSet.GetTextColor(state);
            Color newColor = expression.EvaluateTyped(context);
            if (newColor != currentColor) {
                styleSet.SetTextColor(newColor, state);
            }
        }

    }

}