using Rendering;
using UnityEngine;

namespace Src.StyleBindings.Text {

    public class StyleBinding_TextColor : StyleBinding {

        private readonly Expression<Color> expression;

        public StyleBinding_TextColor(StyleState state, Expression<Color> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {}

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.text.color = expression.EvaluateTyped(context);
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