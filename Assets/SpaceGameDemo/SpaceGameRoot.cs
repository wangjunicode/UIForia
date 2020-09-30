using UIForia.Attributes;
using UIForia.Elements;

namespace SpaceGameDemo {

    [Template("SpaceGameRoot.xml")]
    public class SpaceGameRoot : UIElement {

        public override void OnUpdate() {
            Controllers.GetSpacePanelController().UpdateRotation();
        }

    }

}