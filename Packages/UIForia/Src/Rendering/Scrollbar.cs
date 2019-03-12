using System;
using SVGX;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.UIInput;
using UnityEngine;

namespace UIForia {

    public abstract class Scrollbar {

        public abstract Size RunLayout(UIElement hostElement);

        public abstract void Paint(UIElement hostElement, Size size, ImmediateRenderContext ctx, SVGXMatrix matrix);

        public void HandleMouseInputEvent(UIElement element, MouseInputEvent evt) {
            switch (evt.type) {
                case InputEventType.MouseEnter:
                    OnMouseEnter(element, evt);
                    break;
                case InputEventType.MouseExit:
                    OnMouseExit(element, evt);
                    break;
                case InputEventType.MouseUp:
                    OnMouseUp(element, evt);
                    break;
                case InputEventType.MouseDown:
                    OnMouseDown(element, evt);
                    break;
                case InputEventType.MouseMove:
                    OnMouseMove(element, evt);
                    break;
                case InputEventType.MouseHover:
                    OnMouseHover(element, evt);
                    break;
                case InputEventType.MouseContext:
                    OnMouseContext(element, evt);
                    break;
                case InputEventType.MouseScroll:
                    OnMouseScroll(element, evt);
                    break;
                case InputEventType.MouseClick:
                    OnMouseClick(element, evt);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public virtual DragEvent CreateDragEvent(UIElement element, MouseInputEvent evt) {
            return null; 
        }

        protected virtual void OnMouseDown(UIElement element, MouseInputEvent evt) { }
        protected virtual void OnMouseUp(UIElement element, MouseInputEvent evt) { }
        protected virtual void OnMouseEnter(UIElement element, MouseInputEvent evt) { }
        protected virtual void OnMouseExit(UIElement element, MouseInputEvent evt) { }
        protected virtual void OnMouseMove(UIElement element, MouseInputEvent evt) { }
        protected virtual void OnMouseHover(UIElement element, MouseInputEvent evt) { }
        protected virtual void OnMouseContext(UIElement element, MouseInputEvent evt) { }
        protected virtual void OnMouseScroll(UIElement element, MouseInputEvent evt) { }
        protected virtual void OnMouseClick(UIElement element, MouseInputEvent evt) { }

    }

    public class DefaultScrollbarDragEvent : DragEvent {

        public readonly float baseOffset;
        public readonly DefaultScrollbar scrollbar;
        
        public DefaultScrollbarDragEvent(UIElement origin, DefaultScrollbar scrollbar, float baseOffset) : base(origin) {
            this.baseOffset = baseOffset;
            this.scrollbar = scrollbar;
        }

        public override void Update() {
            if (origin.isDisabled || !origin.layoutResult.HasScrollbarVertical) {
                Cancel();
                return;
            }

            Rect trackRect = scrollbar.GetVerticalTrackRect(origin);
            Rect handleRect = scrollbar.GetVerticalHandleRect(origin);
            
            float max = trackRect.height - handleRect.height;
            float offset = Mathf.Clamp(MousePosition.y - trackRect.y - baseOffset, 0, max);
            origin.scrollOffset = new Vector2(origin.scrollOffset.x, offset / max);
        }

    }

    public class DefaultScrollbar : Scrollbar {

        protected float verticalHandleWidth;
        protected float horizontalHandleHeight;
        
        public DefaultScrollbar(float horizontalHandleHeight = 10f, float verticalHandleWidth = 10f) {
            this.horizontalHandleHeight = horizontalHandleHeight;
            this.verticalHandleWidth = verticalHandleWidth;
        }

        public override DragEvent CreateDragEvent(UIElement element, MouseInputEvent evt) {
            Rect trackRect = GetVerticalTrackRect(element);
            Rect handleRect = GetVerticalHandleRect(element);
            return new DefaultScrollbarDragEvent(element, this, evt.MouseDownPosition.y - (trackRect.y + handleRect.y));
        }
        
        public override Size RunLayout(UIElement hostElement) {
            return new Size(verticalHandleWidth, hostElement.layoutResult.allocatedSize.height);    
        }

        public override void Paint(UIElement hostElement, Size size, ImmediateRenderContext ctx, SVGXMatrix matrix) {
            ctx.SetTransform(matrix);
            ctx.SetFill(Color.gray);
            ctx.FillRect(0, 0, size.width, size.height);
            ctx.SetFill(Color.red);
            float handleY = GetVerticalHandleY(hostElement);
            ctx.RoundedRect(new Rect(0, handleY, verticalHandleWidth, GetVerticalHandleHeight(hostElement)), 200, 200, 200, 200);
            ctx.Fill();
        }

        public float GetVerticalHandleY(UIElement element) {
            return element.scrollOffset.y * (element.layoutResult.allocatedSize.height - GetVerticalHandleHeight(element));
        }

        public Rect GetVerticalHandleRect(UIElement hostElement) {
            return new Rect(0, GetVerticalHandleY(hostElement), verticalHandleWidth, GetVerticalHandleHeight(hostElement));
        }

        public Rect GetVerticalTrackRect(UIElement hostElement) {
            Size size = hostElement.layoutResult.allocatedSize;
            Size horizontalSize = hostElement.layoutResult.scrollbarHorizontalSize;
            float height = size.height;
            if (horizontalSize.IsDefined()) {
                height -= horizontalSize.height;
            }
            return new Rect(0, 0, size.width, height);
        }
        
        public float GetVerticalHandleHeight(UIElement element) {
            LayoutResult layoutResult = element.layoutResult;
            float allocatedHeight = layoutResult.allocatedSize.height;
            float actualHeight = layoutResult.actualSize.height;
            return (allocatedHeight / actualHeight) * allocatedHeight;
        }

        protected override void OnMouseClick(UIElement element, MouseInputEvent evt) {
            Rect trackRect = GetVerticalTrackRect(element);
            if (trackRect.Contains(evt.MouseDownPosition)) {
                float y = evt.MouseDownPosition.y - element.layoutResult.screenPosition.y;
                element.scrollOffset = new Vector2(element.scrollOffset.x, y / trackRect.height);
            }
        }

    }

}