using UIForia.Layout;
using UIForia.Rendering;

namespace UIForia.Elements {

    public sealed class RepeatMultiChildContainerElement : UIElement {

        public override void OnCreate() {
            style.SetLayoutBehavior(LayoutBehavior.TranscludeChildren, StyleState.Normal);
        }

    }

}