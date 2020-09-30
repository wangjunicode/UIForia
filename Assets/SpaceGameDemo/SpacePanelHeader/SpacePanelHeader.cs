using UIForia.Attributes;
using UIForia.Elements;
using static SpaceGameDemo.Controllers;

namespace SpaceGameDemo.SpacePanelHeader {
   
    [Template("SpacePanelHeader/SpacePanelHeader.xml")]
    public class SpacePanelHeader : UIElement {
        // Parameter / Property
        public string title;

        // Parameter / Property
        public string crumb;

        public void GoToStartMenu() => GetSpacePanelController().LookAtRandomSpace("StartMenu", this);
    }
}