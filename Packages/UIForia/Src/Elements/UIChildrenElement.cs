using UIForia.Attributes;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Templates;

namespace UIForia.Elements {

    [TemplateTagName("Children")]
    public class UIChildrenElement : UISlotContent {

        internal UITemplate template;
        internal TemplateScope templateScope;

        internal UIChildrenElement() {
        }

        public override void OnCreate() {
            if (template == null) {
                style.SetLayoutBehavior(LayoutBehavior.TranscludeChildren, StyleState.Normal);
            }
        }

        public UIElement InstantiateTemplate() {
            if (template == null) return null;
            UIElement newItem = template.CreateScoped(templateScope);
            newItem.templateContext.rootObject = templateContext.rootObject;
            return newItem;
        }

        public override string GetDisplayName() {
            return "Children";
        }

    }

}