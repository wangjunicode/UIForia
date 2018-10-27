using Rendering;
using Src.Rendering;
using UnityEngine;

namespace Src.StyleBindings.Text {

    public class StyleBinding_FontSize : StyleBinding {

        private readonly Expression<int> expression;

        public StyleBinding_FontSize(StyleState state, Expression<int> expression) : base(RenderConstants.FontSize, state) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;

            int currentSize = element.ComputedStyle.FontSize;
            int newSize = expression.EvaluateTyped(context);
            if (currentSize != newSize) {
                element.style.SetFontSize(newSize, state);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.FontSize = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            int currentSize = styleSet.GetFontSize(state);
            int newSize = expression.EvaluateTyped(context);
            if (currentSize != newSize) {
                styleSet.SetFontSize(newSize, state);
            }
        }

    }

}