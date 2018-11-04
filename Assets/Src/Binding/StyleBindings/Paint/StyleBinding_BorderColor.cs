using Src.Rendering;
using UnityEngine;

namespace Src.StyleBindings {

    public class StyleBinding_BorderColor : StyleBinding {

        public readonly Expression<Color> expression;
        
        public StyleBinding_BorderColor(StyleState state, Expression<Color> expression) : base(RenderConstants.BorderColor, state) {
            this.expression = expression;
        }        
        
        public override void Execute(UIElement element, UITemplateContext context) {
            // context.SetMethodAlias("rgb", rgbInfo);
            Color color = expression.EvaluateTyped(context);
            Color currentColor = element.style.GetBorderColor(state);
            if (color != currentColor) {
                element.style.SetBorderColor(color, state);
            }
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.BorderColor = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBorderColor(expression.EvaluateTyped(context), state);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }
        
    }

}