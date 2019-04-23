using UIForia.Layout;
using UIForia.Rendering;

namespace UIForia.Elements {

    public class RepeatMultiChildContainerElement : UIElement {

        public RepeatMultiChildContainerElement() {
            flags |= UIElementFlags.BuiltIn | UIElementFlags.ImplicitElement;
            style.SetLayoutBehavior(LayoutBehavior.TranscludeChildren, StyleState.Normal);
        }

    }


}