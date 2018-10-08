using System;
using Rendering;
using Src.Rendering;
using UnityEngine;

namespace Src.StyleBindings {

    public class StyleBinding_BackgroundImage : StyleBinding {

        public readonly Expression<Texture2DAssetReference> expression;
        
        public StyleBinding_BackgroundImage(StyleState state, Expression<Texture2DAssetReference> expression) 
            : base(RenderConstants.BackgroundImage, state) {
            this.expression = expression;
        }        
        
        public override void Execute(UIElement element, UITemplateContext context) {
            if (!element.style.IsInState(state)) return;
            
            Texture2DAssetReference textureRef = expression.EvaluateTyped(context);
            if (textureRef.assetId != element.style.computedStyle.BackgroundImage.assetId) {
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