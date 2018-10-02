using Src.Input;
using Src.Rendering;
using Src.Systems;
using UnityEngine;

namespace Src.Elements {

    public enum ScrollbarOrientation {

        Horizontal,
        Vertical

    }

    public class VirtualScrollbar : VirtualElement {

        public readonly UIElement targetElement;
        public readonly ScrollbarOrientation orientation;
        public float handleSize;
        public float trackSize;

        public VirtualScrollbar(UIElement target, ScrollbarOrientation orientation) {
            this.targetElement = target;
            this.orientation = orientation;
            this.trackSize = 5f;
            this.handleSize = 5f;
            this.depth = target.depth;
            this.siblingIndex = int.MaxValue - (orientation == ScrollbarOrientation.Horizontal ? 1 : 0);
        }

        public Vector2 handlePosition => new Vector2(
            orientation == ScrollbarOrientation.Vertical ? 0 : targetElement.scrollOffset.x * (targetElement.layoutResult.allocatedWidth - handleWidth),
            orientation == ScrollbarOrientation.Horizontal ? 0 : targetElement.scrollOffset.y * (targetElement.layoutResult.allocatedHeight - handleHeight)
        );

        public float handleWidth => orientation == ScrollbarOrientation.Vertical ? handleSize : (targetElement.layoutResult.allocatedWidth / targetElement.layoutResult.contentWidth) * targetElement.layoutResult.allocatedWidth;
        public float handleHeight => orientation == ScrollbarOrientation.Horizontal ? handleSize : (targetElement.layoutResult.allocatedHeight / targetElement.layoutResult.contentHeight) * targetElement.layoutResult.allocatedHeight;

        public ScrollbarDragEvent CreateDragEvent(MouseInputEvent evt) {
            if (HandleRect.Contains(evt.MouseDownPosition)) {
                float baseOffset;
                if (orientation == ScrollbarOrientation.Vertical) {
                    baseOffset = evt.MouseDownPosition.y - (GetTrackRect().y + handlePosition.y);
                }
                else {
                    baseOffset = evt.MouseDownPosition.x - (GetTrackRect().x + handlePosition.x);
                }

                return new ScrollbarDragEvent(baseOffset, this);
            }

            return null;
        }

        public Rect GetTrackRect() {
            LayoutResult targetElementLayoutResult = targetElement.layoutResult;
            Vector2 parentWorld = targetElement.layoutResult.screenPosition;
            if (orientation == ScrollbarOrientation.Vertical) {
                if (targetElement.style.verticalScrollbarAttachment == VerticalScrollbarAttachment.Right) {
                    float x = parentWorld.x + targetElementLayoutResult.allocatedWidth - trackSize;
                    float y = parentWorld.y;
                    float w = trackSize;
                    float h = targetElementLayoutResult.allocatedHeight;
                    return new Rect(x, y, w, h);
                }
                else {
                    float x = parentWorld.x;
                    float y = parentWorld.y;
                    float w = trackSize;
                    float h = targetElementLayoutResult.allocatedHeight;
                    return new Rect(x, y, w, h);
                }
            }

            if (targetElement.style.horizontalScrollbarAttachment == HorizontalScrollbarAttachment.Top) {
                float x = parentWorld.x;
                float y = parentWorld.y;
                float w = targetElementLayoutResult.allocatedWidth;
                float h = trackSize;
                return new Rect(x, y, w, h);
            }
            else {
                float x = parentWorld.x;
                float y = parentWorld.y + targetElementLayoutResult.allocatedHeight - trackSize;
                float w = targetElementLayoutResult.allocatedWidth;
                float h = trackSize;
                return new Rect(x, y, w, h);
            }
        }

        public Rect HandleRect {
            get {
                Vector2 parentWorld = targetElement.layoutResult.screenPosition;
                if (orientation == ScrollbarOrientation.Vertical) {
                    if (targetElement.style.verticalScrollbarAttachment == VerticalScrollbarAttachment.Right) {
                        float x = parentWorld.x + targetElement.layoutResult.allocatedWidth - trackSize;
                        float y = parentWorld.y + handlePosition.y;
                        float w = handleSize;
                        float h = handleHeight;
                        return new Rect(x, y, w, h);
                    }
                    else {
                        float x = parentWorld.x;
                        float y = parentWorld.y;
                        float w = handleSize;
                        float h = handleHeight;
                        return new Rect(x, y, w, h);
                    }
                }

                if (targetElement.style.horizontalScrollbarAttachment == HorizontalScrollbarAttachment.Top) {
                    float x = parentWorld.x;
                    float y = parentWorld.y;
                    float w = handleWidth;
                    float h = handleSize;
                    return new Rect(x, y, w, h);
                }
                else {
                    float x = parentWorld.x;
                    float y = parentWorld.y + targetElement.layoutResult.allocatedHeight - trackSize;
                    float w = handleWidth;
                    float h = handleSize;
                    return new Rect(x, y, w, h);
                }
            }
        }

        public class ScrollbarDragEvent : DragEvent {

            public readonly float baseOffset;
            public readonly VirtualScrollbar scrollbar;

            public ScrollbarDragEvent(float baseOffset, VirtualScrollbar scrollbar) {
                this.baseOffset = baseOffset;
                this.scrollbar = scrollbar;
            }

            public override void Update() {
                Rect trackRect = scrollbar.GetTrackRect();
                if (scrollbar.orientation == ScrollbarOrientation.Vertical) {
                    float max = trackRect.height - scrollbar.handleHeight;
                    float offset = Mathf.Clamp(MousePosition.y - trackRect.y - baseOffset, 0, max);
                    scrollbar.targetElement.scrollOffset = new Vector2(scrollbar.targetElement.scrollOffset.x, offset / max);
                }
                else {
                    float max = trackRect.width - scrollbar.handleWidth;
                    float offset = Mathf.Clamp(MousePosition.x - trackRect.x - baseOffset, 0, max);
                    scrollbar.targetElement.scrollOffset = new Vector2(offset / max, scrollbar.targetElement.scrollOffset.y);
                }
            }

        }

    }

}