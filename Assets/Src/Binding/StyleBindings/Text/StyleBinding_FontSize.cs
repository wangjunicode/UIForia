using Rendering;
using UnityEngine;

namespace Src.StyleBindings.Text {

    public class StyleBinding_FontSize : StyleBinding {

        private readonly Expression<int> expression;

        public StyleBinding_FontSize(StyleState state, Expression<int> expression) : base(state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            int currentSize = element.style.GetFontSize(state);
            int newSize = expression.EvaluateTyped(context);
            if (currentSize != newSize) {
            Debug.Log("current: " + currentSize + " new " + newSize);
                element.style.SetFontSize(newSize, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.textStyle.fontSize = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            int currentSize = styleSet.GetFontSize(state);
            int newSize = expression.EvaluateTyped(context);
            Debug.Log("current: " + currentSize + " new " + newSize);
            if (currentSize != newSize) {
                styleSet.SetFontSize(newSize, state);
            }
        }

    }

}