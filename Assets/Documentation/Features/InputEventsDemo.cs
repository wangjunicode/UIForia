using System.ComponentModel.Design;
using UIForia;
using UIForia.Animation;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.UIInput;
using UnityEngine;

namespace Documentation.Features {

    [Template("Documentation/Features/InputEventsDemo")]
    public class InputEventsDemo : UIElement {

        public bool IsInStartZone;
        public bool IsInDropZone;

        public float LastHoverX;
        public float LastHoverY;
        public Vector2 LastMove;
        public Vector2 LastEnter;
        public Vector2 LastExit;
        public Vector2 LastClick;
        public Vector2 LastUp;
        public Vector2 LastDown;
        public Vector2 LastContext;

        private string activeEvent;

        public override void OnEnable() {
            IsInDropZone = false;
            IsInStartZone = true;
        }

        public DragEvent StartDrag(UIElement element) {
            activeEvent = "onDragCreate";
            return new DemoDragEvent(element);
        }

        public void DropDrag(DragEvent evt, UIElement target) {
            activeEvent = "onDragDrop";

            IsInDropZone = target.GetAttribute("id") == "dropZone";
            IsInStartZone = target.GetAttribute("id") == "startZone";
        }

        [OnDragMove(typeof(DemoDragEvent))]
        public void OnDragMove(DragEvent evt) {
            activeEvent = "onDragMove";
        }
        
        public void OnDragEnter(DragEvent evt) {
            activeEvent = "onDragEnter";
        }

        public void OnDragExit(DragEvent evt) {
            activeEvent = "onDragExit";
        }

        public void OnDragHover(DragEvent evt) {
            activeEvent = "onDragHover";
            evt.target.style.SetBackgroundColor(Color.yellow, StyleState.Normal);
        }

        public string MousePositionToString(Vector2 mousePosition) {
            return mousePosition.ToString();
        }

        public string ActiveEvent(string evt) {
            return activeEvent == evt ? "active-event" : string.Empty;
        }
        
        public void Hover(MouseInputEvent evt) {
            LastHoverX = evt.MousePosition.x;
            LastHoverY = evt.MousePosition.y;
        }
        //
        // public void Move(MouseInputEvent evt) {
        //     LastMove = evt;
        // }
        //
        // public void Enter(MouseInputEvent evt) {
        //     LastEnter = evt;
        // }
        //
        // public void Exit(MouseInputEvent evt) {
        //     LastExit = evt;
        // }
        //
        // public void Click(MouseInputEvent evt) {
        //     LastClick = evt;
        // }
        //
        // public void Up(MouseInputEvent evt) {
        //     LastUp = evt;
        // }
        //
        // public void Down(MouseInputEvent evt) {
        //     LastDown = evt;
        // }
        //
        // public void Context(MouseInputEvent evt) {
        //     LastContext = evt;
        // }

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