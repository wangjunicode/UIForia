using UIForia.Attributes;
using UIForia.Elements;

namespace SpaceGameDemo.Settings {
    
    [Template("SpaceGameDemo/Settings/Settings.xml")]
    public class Settings : UIElement {
        public string title = "Graphics";
        public bool bloom;
        public bool vsync;

        public void Show(string page) {
            title = page;
        }
    }
}