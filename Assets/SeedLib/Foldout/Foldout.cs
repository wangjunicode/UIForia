using UIForia.Attributes;
using UIForia.Elements;

namespace SeedLib {

    [Template("SeedLib/Foldout/Foldout.xml")]
    public class Foldout : UIElement {

        public bool expanded = true;
        public string title;

        public void ToggleExpanded() {
            expanded = !expanded;
        }
        
        public override void OnEnable() {
            UIElement children = FindById<UIElement>("foldout-children");
            
            if (TryGetAttribute("variant", out string attribute) && attribute.Contains("dense")) {
                children.SetAttribute("dense", "true");    
            } else {
                children.SetAttribute("dense", "false");
            }
        }
    }

}