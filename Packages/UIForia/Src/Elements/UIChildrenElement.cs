using UIForia.Attributes;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Templates;

namespace UIForia.Elements {

    [TemplateTagName("Children")]
    public class UIChildrenElement : UISlotContent {

        public UIChildrenElement() {
        }

        public override void OnCreate() {
         
        }

        public UIElement InstantiateTemplate() {
            return null;
        }

        public override string GetDisplayName() {
            return "Children";
        }

    }

}