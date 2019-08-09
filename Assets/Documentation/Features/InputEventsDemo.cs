using System.ComponentModel.Design;
using UIForia;
using UIForia.Animation;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.UIInput;
using UnityEngine;

namespace Documentation.Features {

    [Template("Documentation/Features/InputEventsDemo")]
    public class InputEventsDemo : UIElement {

        public bool IsInStartZone;
        public bool IsInDropZone;

        public string LastHoverX;
        public string LastHoverY;
        public float LastDragEnter;
        public float LastDragExit;
        public Vector2 LastMove;
        public Vector2 LastEnter;
        public Vector2 LastExit;
        public Vector2 LastClick;
        public Vector2 LastUp;
        public Vector2 LastDown;
        public Vector2 LastContext;

        public bool IsContextMenuVisible = false;

        private string activeEvent;

        public override void OnEnable() {
            IsInDropZone = false;
            IsInStartZone = true;
        }

        public DragEvent StartDrag(MouseInputEvent evt, UIElement element) {
            activeEvent = "onDragCreate";
            return new DemoDragEvent(evt.MousePosition, element);
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
            LastDragEnter = Time.realtimeSinceStartup;
        }

        public void OnDragExit(DragEvent evt) {
            LastDragExit = Time.realtimeSinceStartup;
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
        
        public string ReturnActiveEvent(string evt, float threshold) {
            switch (evt) {
                case "onDragEnter": return Time.realtimeSinceStartup - LastDragEnter < threshold ? "onDragEnter" : string.Empty;  
                case "onDragExit": return Time.realtimeSinceStartup - LastDragExit < threshold ? "onDragExit" : string.Empty;  
            }
            return string.Empty;
        }
        
        public void Hover(MouseInputEvent evt) {
            LastHoverX = evt.MousePosition.x.ToString();
            LastHoverY = evt.MousePosition.y.ToString();
        }
        
        public void ShowContextMenu(MouseInputEvent evt) {
            LastContext = evt.MousePosition;
            IsContextMenuVisible = true;
        }
        
        [OnMouseContext]
        public void ToggleContextMenuFromAnyWhere(MouseInputEvent evt) {
            LastContext = evt.MousePosition;
            IsContextMenuVisible = true;
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

        private float baseX;
        private float baseY;
        private float offsetX;
        private float offsetY;

        public DemoDragEvent(Vector2 offset, UIElement element) : base(element) {
            this.baseX = element.layoutResult.screenPosition.x - offset.x;
            this.baseY = element.layoutResult.screenPosition.y - offset.y;
        }

        public override void Update() {
            // mouse position - original position - pivot
            float x = MousePosition.x - baseX;
            float y = MousePosition.y - baseY;
            origin.style.SetTransformPositionX(x, StyleState.Normal);
            origin.style.SetTransformPositionY(y, StyleState.Normal);
        }

        public override void Drop(bool success) {
            base.Drop(success);
//            origin.Application.Animate(origin, new AnimationData() {
//                frames = new [] {
//                    new StyleKeyFrameValue(0, StyleProperty.TransformPositionX()), 
//                }
//            });
        }

    }

}