using UIForia.Attributes;
using UIForia.Elements;

namespace SeedLib {
    
    [Template("SeedLib/List/ListItem.xml")]
    public class ListItem: UIElement {

        public ImageLocator icon;

        public override void OnEnable() {
            UIImageElement image = FindFirstByType<UIImageElement>();
            var itemList = FindParent<__ItemList>();
            if (itemList == null) {
                return;
            }
            
            if (itemList.TryGetAttribute("variant", out string attribute) && attribute.Contains("dense")) {
                image.SetAttribute("dense", "true");    
            } else {
                image.SetAttribute("dense", "false");
            }
        }
    }
}