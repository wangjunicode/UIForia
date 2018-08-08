using UnityEngine;
using UnityEngine.UI;

namespace Src {

    public class UnityImagePrimitive : ImagePrimitive {

        private readonly RawImage imageComponent;
        
        public UnityImagePrimitive(RawImage imageComponent) {
            this.imageComponent = imageComponent;
        }

        public override Texture2D Image { get; set; }

    }

}