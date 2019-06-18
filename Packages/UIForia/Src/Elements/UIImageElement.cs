using UIForia.Attributes;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia.Elements {

    public class UIImageElement : UIElement {

        public string src;
        public Texture2D texture;
        private Mesh mesh;
        
        public UIImageElement() {
            flags |= UIElementFlags.Primitive;
        }
        
        [OnPropertyChanged(nameof(src))]
        [OnPropertyChanged(nameof(texture))]
        public void OnSrcChanged(string name) {
            if (src != null) {
                texture = ResourceManager.GetTexture(src);
            }
            style.SetBackgroundImage(texture, StyleState.Normal);
        }
        
        public override string GetDisplayName() {
            return "Image";
        }

    }

}