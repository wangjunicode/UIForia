using System;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia.StyleBindings {

    public class StyleBinding_BackgroundImage : StyleBinding {

        public readonly Expression<Texture2D> expression;
        
        public StyleBinding_BackgroundImage(StyleState state, Expression<Texture2D> expression) 
            : base(RenderConstants.BackgroundImage, state) {
            this.expression = expression;
        }        
        
        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;
            
            Texture2D textureRef = expression.EvaluateTyped(context);
            if (textureRef != element.style.computedStyle.BackgroundImage) {
                element.style.SetBackgroundImage(textureRef, state);
            }
        }

        public override void Apply(UIStyle style, UITemplateContext context) {
            style.BackgroundImage = expression.EvaluateTyped(context);
        }

        public override void Apply(UIStyleSet styleSet, UITemplateContext context) {
            styleSet.SetBackgroundImage(expression.EvaluateTyped(context), state);
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }
        
    }

}