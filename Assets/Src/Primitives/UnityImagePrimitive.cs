using Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace Src {

    public class UnityImagePrimitive : ImagePrimitive {

        private readonly RawImage imageComponent;
        
        public UnityImagePrimitive(RawImage imageComponent) {
            this.imageComponent = imageComponent;
        }

        public override Texture2D Image { get; set; }
       
        public override void ApplyStyleSettings(UIStyle style) {
            imageComponent.color = style.background.color;
            imageComponent.texture = style.background.texture;
        }

    }

}