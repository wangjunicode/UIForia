using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UnityEngine;

namespace SpaceGameDemo {
    [Template("SpaceGameDemo/SpaceGameRoot.xml")]
    public class SpaceGameRoot : UIElement {
        
        public override void OnUpdate() {
            RenderTexture rt = RenderTexture.GetTemporary(128, 128, 24);
            style.SetBackgroundImage(rt, StyleState.Normal);
            Controllers.GetSpacePanelController().UpdateRotation();
        }
    }
}