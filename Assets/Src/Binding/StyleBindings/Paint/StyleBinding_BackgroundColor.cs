using Rendering;
using UnityEngine;

namespace Src.StyleBindings {

    public class StyleBinding_BackgroundColor : StyleBinding {

        public readonly Expression<Color> expression;
        
        public StyleBinding_BackgroundColor(StyleState state, Expression<Color> expression) : base(state) {
            this.expression = expression;
        }        
        
        public override void Execute(UIElement element, UITemplateContext context) {
           // context.SetMethodAlias("rgb", rgbInfo);
            Color color = expression.EvaluateTyped(context);
            Color currentColor = element.style.GetBackgroundColor(state);
            if (color != currentColor) {
                element.style.SetBackgroundColor(color, state);
            }
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.paint.backgroundColor = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBackgroundColor(expression.EvaluateTyped(context), state);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }
        
    }

}