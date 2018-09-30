using System;
using Rendering;
using Src.Rendering;
using UnityEngine;

namespace Src.StyleBindings {

    public class StyleBinding_BackgroundImage : StyleBinding {

        public readonly Expression<AssetPointer<Texture2D>> expression;
        
        public StyleBinding_BackgroundImage(StyleState state, Expression<AssetPointer<Texture2D>> expression) : base(RenderConstants.BackgroundImage, state) {
            this.expression = expression;
        }        
        
        public override void Execute(UIElement element, UITemplateContext context) {
            throw new NotImplementedException();
//            AssetPointer<Texture2D> textureAssetPtr = expression.EvaluateTyped(context);
//            Texture2D currentTexture = element.style.GetBackgroundImage(state).asset;
//            if (texture != currentTexture) {
//                element.style.SetBackgroundImage(texture, state);
//            }
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