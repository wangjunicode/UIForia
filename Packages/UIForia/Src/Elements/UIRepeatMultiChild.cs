using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Elements {

    [RecordFilePath]
    public sealed class RepeatMultiChildContainerElement : UIElement {

        public RepeatMultiChildContainerElement() {
            flags |= UIElementFlags.ImplicitElement;
        }

        public override void OnCreate() {
            style.SetLayoutBehavior(LayoutBehavior.TranscludeChildren, StyleState.Normal);
        }

    }

}