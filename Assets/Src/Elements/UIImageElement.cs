using UnityEngine;

namespace Src {

    public class UIImageElement : UIElement {

        public AssetPointer<Texture2D> src;

        public UIImageElement() {
            flags |= UIElementFlags.Image;
            flags |= UIElementFlags.Primitive;
        }
    }

}