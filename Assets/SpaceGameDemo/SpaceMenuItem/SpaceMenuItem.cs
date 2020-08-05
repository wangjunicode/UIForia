using UIForia.Attributes;
using UIForia.Elements;
using static SpaceGameDemo.Controllers;

namespace SpaceGameDemo {

    [Template("SpaceGameDemo/SpaceMenuItem/SpaceMenuItem.xml")]
    public class SpaceMenuItem : UIElement {

        // Parameter / Property
        public string label;

        // Parameter
        public string targetPanel;

        [OnMouseClick]
        public void LookAtNext() {
            if (targetPanel != null) {
                GetSpacePanelController().LookAtRandomSpace(targetPanel, this);
            }
        }

    }

}