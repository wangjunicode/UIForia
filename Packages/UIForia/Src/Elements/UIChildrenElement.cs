using UIForia.Layout;
using UIForia.Rendering;

namespace UIForia.Elements {

    public class UIChildrenElement : UIElement {

        internal UIChildrenElement() {
            flags |= UIElementFlags.BuiltIn;
        }

        public override void OnCreate() {
            style.SetLayoutBehavior(LayoutBehavior.TranscludeChildren, StyleState.Normal);
        }

        public override string GetDisplayName() {
            return "Children";
        }

    }

}