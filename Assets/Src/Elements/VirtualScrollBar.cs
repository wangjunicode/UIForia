using Rendering;
using Src.Input;
using Src.Rendering;
using Src.Systems;
using Src.Util;
using UnityEngine;

namespace Src.Elements {

    public enum ScrollbarOrientation {

        Horizontal,
        Vertical

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
                scrollbar.handle.style.SetTransformPosition(new Vector2(0f, offset), StyleState.Normal);
            }
            else {
                float max = trackRect.width - scrollbar.handleWidth;
                float offset = Mathf.Clamp(MousePosition.x - trackRect.x - baseOffset, 0, max);
                scrollbar.targetElement.scrollOffset = new Vector2(offset / max, scrollbar.targetElement.scrollOffset.y);
                scrollbar.handle.style.SetTransformPosition(new Vector2(offset, 0f), StyleState.Normal);
            }
        }

    }

    public class VirtualScrollbarHandle : VirtualElement {

        public VirtualScrollbarHandle() {
            flags |= UIElementFlags.Enabled | UIElementFlags.AncestorEnabled;
            ownChildren = ArrayPool<UIElement>.Empty;
        }

    }

    public class VirtualScrollbar : VirtualElement {

        public readonly UIElement targetElement;
        public readonly ScrollbarOrientation orientation;
        public float handleSize;
        public float trackSize;
        public VirtualScrollbarHandle handle;

        public VirtualScrollbar(UIElement target, ScrollbarOrientation orientation) {
            this.targetElement = target;
            this.handle = new VirtualScrollbarHandle();
            this.handle.parent = this;
            this.ownChildren = ArrayPool<UIElement>.GetExactSize(1);
            this.ownChildren[0] = handle;
            this.orientation = orientation;
            this.trackSize = 15f;
            this.handleSize = 15f;
            this.depth = target.depth;
            this.parent = target;
            this.siblingIndex = int.MaxValue - (orientation == ScrollbarOrientation.Horizontal ? 1 : 0);
            this.handle.depth = depth + 1;
            if (target.isEnabled) {
                flags |= UIElementFlags.AncestorEnabled;
            }
        }

        public Vector2 handlePosition => new Vector2(
            orientation == ScrollbarOrientation.Vertical ? 0 : targetElement.scrollOffset.x * (targetElement.layoutResult.allocatedWidth - handleWidth),
            orientation == ScrollbarOrientation.Horizontal ? 0 : targetElement.scrollOffset.y * (targetElement.layoutResult.allocatedHeight - handleHeight)
        );

        public float handleWidth => orientation == ScrollbarOrientation.Vertical
            ? handleSize
            : (targetElement.layoutResult.allocatedWidth / targetElement.layoutResult.contentWidth) * targetElement.layoutResult.allocatedWidth;

        public float handleHeight => orientation == ScrollbarOrientation.Horizontal
            ? handleSize
            : (targetElement.layoutResult.allocatedHeight / targetElement.layoutResult.contentHeight) * targetElement.layoutResult.allocatedHeight;

        [OnDragCreate]
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

    }

}