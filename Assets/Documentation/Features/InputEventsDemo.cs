using UIForia;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.UIInput;

namespace Documentation.Features {

    [Template("Documentation/Features/InputEventsDemo")]
    public class InputEventsDemo : UIElement {

        public bool IsInStartZone;
        public bool IsInDropZone;

        public override void OnEnable() {
            IsInDropZone = false;
            IsInStartZone = true;
        }

        public DragEvent StartDrag(UIElement element) {
            return new DemoDragEvent(element);
        }

        public void DropDrag(DragEvent evt, UIElement target) {
            IsInDropZone = target.GetAttribute("id") == "dropZone";
            IsInStartZone = target.GetAttribute("id") == "startZone";
        }

    }

    public class DemoDragEvent : DragEvent {

        public DemoDragEvent(UIElement element) : base(element) {
        }

        public override void Update() {
            origin.style.SetAnchorTarget(AnchorTarget.Screen, StyleState.Normal);
            origin.style.SetAnchorLeft(new UIFixedLength(0), StyleState.Normal);
            origin.style.SetAnchorTop(new UIFixedLength(0), StyleState.Normal);
            origin.style.SetTransformBehavior(TransformBehavior.AnchorMinOffset, StyleState.Normal);
            origin.style.SetTransformPositionX(new TransformOffset(MousePosition.x -  origin.layoutResult.ActualWidth / 2), StyleState.Normal);
            origin.style.SetTransformPositionY(new TransformOffset(MousePosition.y - origin.layoutResult.ActualHeight / 2), StyleState.Normal);
        }

        public override void Drop(bool success) {
            base.Drop(success);
            origin.style.SetAnchorTarget(AnchorTarget.Parent, StyleState.Normal);
            origin.style.SetTransformBehavior(TransformBehavior.LayoutOffset, StyleState.Normal);
            origin.style.SetTransformPositionX(new TransformOffset(0), StyleState.Normal);
            origin.style.SetTransformPositionY(new TransformOffset(0), StyleState.Normal);
        }

    }

}