using UIForia.Attributes;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Elements {

    public class UIImageElement : UIElement {

        public string src;
        public Texture2D texture;
        private Mesh mesh;

        public int Width;
        public int Height;
        
        public UIImageElement() {
            flags |= UIElementFlags.Primitive;
        }
        
        [OnPropertyChanged(nameof(src))]
        [OnPropertyChanged(nameof(texture))]
        public void OnSrcChanged(string name) {
            if (src != null) {
                texture = Application.ResourceManager.GetTexture(src);
            }
            style.SetBackgroundImage(texture, StyleState.Normal);
            if (Width > 0) {
                style.SetPreferredHeight(texture.height / texture.width * Width, StyleState.Normal);
                style.SetPreferredWidth(Width, StyleState.Normal);
                
            }
            if (Height > 0) {
                style.SetPreferredWidth(texture.width / texture.height * Height, StyleState.Normal);
                style.SetPreferredHeight(Height, StyleState.Normal);
            }
        }
        
        public override string GetDisplayName() {
            return "Image";
        }

    }

}