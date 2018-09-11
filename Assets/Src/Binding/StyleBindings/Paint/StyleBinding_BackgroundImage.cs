using Rendering;
using Src.Rendering;
using UnityEngine;

namespace Src.StyleBindings {

    public class StyleBinding_BackgroundImage : StyleBinding {

        public readonly Expression<Texture2D> expression;
        
        public StyleBinding_BackgroundImage(StyleState state, Expression<Texture2D> expression) : base(RenderConstants.BackgroundImage, state) {
            this.expression = expression;
        }        
        
        public override void Execute(UIElement element, UITemplateContext context) {
            Texture2D texture = expression.EvaluateTyped(context);
            Texture2D currentTexture = element.style.GetBackgroundImage(state);
            if (texture != currentTexture) {
                element.style.SetBackgroundImage(texture, state);
            }
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.paint.backgroundImage = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBackgroundImage(expression.EvaluateTyped(context), state);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }
        
    }

}