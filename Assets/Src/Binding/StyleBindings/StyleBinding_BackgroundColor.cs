using Rendering;
using UnityEngine;

namespace Src.StyleBindings {

    public class StyleBinding_BackgroundColor : StyleBinding {

        public readonly Expression<Color> expression;
        
        public StyleBinding_BackgroundColor(StyleStateType stateType, Expression<Color> expression) : base(stateType) {
            this.expression = expression;
        }        
        
        public override void Execute(UIElement element, UITemplateContext context) {
           // context.SetMethodAlias("rgb", rgbInfo);
            Color color = expression.EvaluateTyped(context);
            Color currentColor = element.style.GetBackgroundColorForState(state);
            if (color != currentColor) {
                element.style.SetBackgroundColorForState(state, color);
            }
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.paint.backgroundColor = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBackgroundColorForState(state, expression.EvaluateTyped(context));
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }
        
    }

}