using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public class UIImageElement : UIElement, IMeshProvider {

        public string src;
        public Texture2D texture;
        private Mesh mesh;
        
        public UIImageElement() {
            flags |= UIElementFlags.Image;
            flags |= UIElementFlags.Primitive;
        }
        
        [OnPropertyChanged(nameof(src))]
        public void OnSrcChanged(string name) {
            texture = ResourceManager.GetTexture(src);
            style.SetBackgroundImage(texture, StyleState.Normal);
        }

        public override string GetDisplayName() {
            return "Image";
        }

        public Mesh GetMesh() {
            mesh = MeshUtil.ResizeStandardUIMesh(mesh, layoutResult.actualSize);
            return mesh;
        }

    }

}