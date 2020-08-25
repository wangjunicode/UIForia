using UIForia.Attributes;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia.Elements {

    [TemplateTagName("Image")]
    public class UIImageElement : UIContainerElement {

        public string src;
        public Texture texture;

        [OnPropertyChanged(nameof(src))]
        public void OnSrcChanged() {
            if (src != null) {
                texture = application.ResourceManager.GetTexture(src);
            }
            else {
                texture = null;
            }

            style.SetBackgroundImage(texture, StyleState.Normal);

        }

        [OnPropertyChanged(nameof(texture))]
        public void OnTextureChanged() {
            SetupBackground();

        }

        private void SetupBackground() {
            style.SetBackgroundImage(texture, StyleState.Normal);
            // if (Width > 0) {
            //     style.SetPreferredHeight(Width * texture.height / texture.width, StyleState.Normal);
            //     style.SetPreferredWidth(Width, StyleState.Normal);
            // }
            //
            // if (Height > 0) {
            //     style.SetPreferredWidth(Height * texture.width / texture.height, StyleState.Normal);
            //     style.SetPreferredHeight(Height, StyleState.Normal);
            // } 
        }

        public override string GetDisplayName() {
            return "Image";
        }

    }

}