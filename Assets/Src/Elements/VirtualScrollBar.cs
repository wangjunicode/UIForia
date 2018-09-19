using Src.Input;
using Src.Rendering;
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
        public float contentHeight;

        public VirtualScrollbar(UIElement target, ScrollbarOrientation orientation) {
            this.targetElement = target;
            this.orientation = orientation;
            this.trackSize = 5f;
        }

        public ScrollbarDragEvent CreateDragEvent(MouseInputEvent evt) {
            Rect handleRect = HandleRect;
            if (handleRect.Contains(evt.MouseDownPosition)) {
                ScrollbarDragEvent retn = new ScrollbarDragEvent();
                float baseOffset = evt.MouseDownPosition.y - GetTrackRect().y;
                retn.onUpdate += (dragEvent) => {
                    handleRect = HandleRect;
                    Rect trackRect = GetTrackRect();
                    float max = trackRect.height - ((targetElement.height / contentHeight) * targetElement.height);
                    float offset = baseOffset + evt.MousePosition.y - trackRect.y;
                    offset = Mathf.Clamp(offset, 0, max) / targetElement.height;
                    Debug.Log("offset: " + offset + " fix this matt");
                    targetElement.scrollOffset = new Vector2(0, contentHeight * offset);
                };
                return retn;
            }

            return null;
        }

        public Rect GetTrackRect() {
            Vector2 parentWorld = targetElement.screenPosition;
            if (orientation == ScrollbarOrientation.Vertical) {
                if (targetElement.style.verticalScrollbarAttachment == VerticalScrollbarAttachment.Right) {
                    float x = parentWorld.x + targetElement.width - trackSize;
                    float y = parentWorld.y;
                    float w = trackSize;
                    float h = targetElement.height;
                    return new Rect(x, y, w, h);
                }
                else {
                    float x = parentWorld.x;
                    float y = parentWorld.y;
                    float w = trackSize;
                    float h = targetElement.height;
                    return new Rect(x, y, w, h);
                }
            }

            if (targetElement.style.horizontalScrollbarAttachment == HorizontalScrollbarAttachment.Top) {
                float x = parentWorld.x;
                float y = parentWorld.y;
                float w = targetElement.width;
                float h = trackSize;
                return new Rect(x, y, w, h);
            }
            else {
                float x = parentWorld.x;
                float y = parentWorld.y + targetElement.height - trackSize;
                float w = targetElement.width;
                float h = trackSize;
                return new Rect(x, y, w, h);
            }
        }

        public Rect HandleRect {
            get {
                Vector2 parentWorld = targetElement.screenPosition;
                if (orientation == ScrollbarOrientation.Vertical) {
                    if (targetElement.style.verticalScrollbarAttachment == VerticalScrollbarAttachment.Right) {
                        float x = parentWorld.x + targetElement.width - trackSize;
                        float y = parentWorld.y + targetElement.scrollOffset.y;
                        float w = 5f;
                        float h = (targetElement.height / contentHeight) * targetElement.height;
                        return new Rect(x, y, w, h);
                    }
                    else {
                        float x = parentWorld.x;
                        float y = parentWorld.y;
                        float w = handleSize;
                        float h = handleSize;
                        return new Rect(x, y, w, h);
                    }
                }

                if (targetElement.style.horizontalScrollbarAttachment == HorizontalScrollbarAttachment.Top) {
                    float x = parentWorld.x;
                    float y = parentWorld.y;
                    float w = handleSize;
                    float h = handleSize;
                    return new Rect(x, y, w, h);
                }
                else {
                    float x = parentWorld.x;
                    float y = parentWorld.y + targetElement.height - trackSize;
                    float w = targetElement.width;
                    float h = trackSize;
                    return new Rect(x, y, w, h);
                }
            }
        }

        public void SetHandleSize(float size) {
            this.handleSize = size;
        }

        public class ScrollbarDragEvent : DragEvent { }

    }

}