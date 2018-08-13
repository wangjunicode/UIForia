using Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace Src {

    public class UnityImagePrimitive : ImagePrimitive {

        private readonly ProceduralImage imageComponent;
        
        public UnityImagePrimitive(ProceduralImage imageComponent) {
            this.imageComponent = imageComponent;
        }

        public override Texture2D Image { get; set; }
       
        public override void ApplyStyleSettings(UIStyleSet style) {
            
        }

    }

}