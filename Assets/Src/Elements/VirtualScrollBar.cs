using UIForia.Input;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Elements {

    public class VirtualScrollbarButton : VirtualElement { }

    public class VirtualScrollbar : VirtualElement {

        public float trackSize;
        public float handleSize;
        public readonly UIElement targetElement;
        public readonly ScrollbarOrientation orientation;
        
        public readonly VirtualScrollbarHandle handle;
        public readonly VirtualScrollbarButton incrementButton;
        public readonly VirtualScrollbarButton decrementButton;
        
        public VirtualScrollbar(UIElement target, ScrollbarOrientation orientation) {
            this.targetElement = target;
            this.parent = target;
            this.handle = new VirtualScrollbarHandle();
            this.incrementButton = new VirtualScrollbarButton();
            this.decrementButton = new VirtualScrollbarButton();
            
            this.handle.parent = this;
            this.incrementButton.parent = this;
            this.decrementButton.parent = this;
            
            this.siblingIndex = int.MaxValue - (orientation == ScrollbarOrientation.Horizontal ? 1 : 0);
            
            this.children = ArrayPool<UIElement>.GetExactSize(3);
            this.children[0] = handle;
            this.children[1] = incrementButton;
            this.children[2] = decrementButton;
            
            this.orientation = orientation;
            this.trackSize = 15f;
            this.handleSize = 15f;
            
            this.depth = target.depth + 1;
            this.incrementButton.depth = target.depth + 1;
            this.decrementButton.depth = target.depth + 1;
            
            if (target.isEnabled) {
                flags |= UIElementFlags.AncestorEnabled;
            }
        }

        public Vector2 handlePosition => new Vector2(
            orientation == ScrollbarOrientation.Vertical ? 0 : targetElement.scrollOffset.x * (targetElement.layoutResult.AllocatedWidth - handleWidth),
            orientation == ScrollbarOrientation.Horizontal ? 0 : targetElement.scrollOffset.y * (targetElement.layoutResult.AllocatedHeight - handleHeight)
        );

        public float handleWidth => orientation == ScrollbarOrientation.Vertical
            ? handleSize
            : (targetElement.layoutResult.AllocatedWidth / targetElement.layoutResult.actualSize.width) * targetElement.layoutResult.AllocatedWidth;

        public float handleHeight => orientation == ScrollbarOrientation.Horizontal
            ? handleSize
            : (targetElement.layoutResult.AllocatedHeight / targetElement.layoutResult.actualSize.height) * targetElement.layoutResult.AllocatedHeight;

        [OnDragCreate]
        public ScrollbarDragEvent CreateDragEvent(MouseInputEvent evt) {
            if (handle.layoutResult.ScreenRect.Contains(evt.MouseDownPosition)) {
                float baseOffset;
                if (orientation == ScrollbarOrientation.Vertical) {
                    baseOffset = evt.MouseDownPosition.y - (TrackRect.y + handlePosition.y);
                }
                else {
                    baseOffset = evt.MouseDownPosition.x - (TrackRect.x + handlePosition.x);
                }

                return new ScrollbarDragEvent(baseOffset, this);
            }

            return null;
        }
        
        public Rect TrackRect {
            get {
                LayoutResult targetElementLayoutResult = targetElement.layoutResult;
                Vector2 parentWorld = targetElement.layoutResult.screenPosition;
                if (orientation == ScrollbarOrientation.Vertical) {
                    bool isXOverflowing = targetElement.ComputedStyle.OverflowX != Overflow.None;
                    if (targetElement.style.verticalScrollbarAttachment == VerticalScrollbarAttachment.Right) {
                        float xOffset = isXOverflowing ? targetElementLayoutResult.allocatedSize.width : targetElementLayoutResult.actualSize.width;
                        float x = parentWorld.x + xOffset - trackSize;
                        float y = parentWorld.y;
                        float w = trackSize;
                        float h = targetElementLayoutResult.AllocatedHeight;
                        return new Rect(x, y, w, h);
                    }
                    else {
                        float x = parentWorld.x;
                        float y = parentWorld.y;
                        float w = trackSize;
                        float h = targetElementLayoutResult.AllocatedHeight;
                        return new Rect(x, y, w, h);
                    }
                }

                if (targetElement.style.horizontalScrollbarAttachment == HorizontalScrollbarAttachment.Top) {
                    float x = parentWorld.x;
                    float y = parentWorld.y;
                    float w = targetElementLayoutResult.AllocatedWidth;
                    float h = trackSize;
                    return new Rect(x, y, w, h);
                }
                else {
                    float x = parentWorld.x;
                    float y = parentWorld.y + targetElementLayoutResult.AllocatedHeight - trackSize;
                    float w = targetElementLayoutResult.AllocatedWidth;
                    float h = trackSize;
                    return new Rect(x, y, w, h);
                }
            }
        }

        public Rect HandleRect {
            get {
                if (orientation == ScrollbarOrientation.Vertical) {
                    if (targetElement.style.verticalScrollbarAttachment == VerticalScrollbarAttachment.Right) {
                        float x = 0;
                        float y = handlePosition.y;
                        float w = handleSize;
                        float h = handleHeight;
                        return new Rect(x, y, w, h);
                    }
                    else {
                        float x = 0;
                        float y = 0;
                        float w = handleSize;
                        float h = handleHeight;
                        return new Rect(x, y, w, h);
                    }
                }

                if (targetElement.style.horizontalScrollbarAttachment == HorizontalScrollbarAttachment.Top) {
                    float x = 0;
                    float y = 0;
                    float w = handleWidth;
                    float h = handleSize;
                    return new Rect(x, y, w, h);
                }
                else {
                    float x = 0;
                    float y = targetElement.layoutResult.AllocatedHeight - trackSize;
                    float w = handleWidth;
                    float h = handleSize;
                    return new Rect(x, y, w, h);
                }
            }
        }

    }

}