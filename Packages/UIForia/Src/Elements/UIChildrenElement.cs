using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Templates;

namespace UIForia.Elements {

    public class UIChildrenElement : UIElement {

        internal UITemplate template;
        internal TemplateScope templateScope;

        internal UIChildrenElement() {
            flags |= UIElementFlags.BuiltIn;
        }

        public override void OnCreate() {
            style.SetLayoutBehavior(LayoutBehavior.TranscludeChildren, StyleState.Normal);
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