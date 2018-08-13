using Rendering;
using UnityEngine;

namespace Src {

    
    public abstract class ImagePrimitive : UIRenderPrimitive{

        public abstract Texture2D Image { get; set; }
        
        public abstract void ApplyStyleSettings(UIStyleSet style);

    }


}