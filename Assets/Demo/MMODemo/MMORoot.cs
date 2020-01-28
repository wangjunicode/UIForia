using UIForia.Attributes;
using UIForia.Elements;

namespace Demo.MMODemo {
    
    // todo -- the automatically detected template path does not work here; cannot remove the template attribute
    [Template("Demo/MMODemo/MMORoot.xml")]
    public class MMORoot : UIElement {

        public string characterName;
    }
}
