using UnityEngine;

namespace Src {

    public struct AssetPointer<T> where T : Object {

        public readonly T asset;

        public AssetPointer(T asset) {
            this.asset = asset;
        }

    }
    
    public class UIImageElement : UIElement {

        public AssetPointer<Texture2D> src;

        public UIImageElement() {
            flags |= UIElementFlags.Image;
        }
    }

}