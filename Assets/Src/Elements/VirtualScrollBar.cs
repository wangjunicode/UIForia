using System;
using UIForia.Input;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Elements {

    public enum ScrollState {

        Normal,
        Hover,
        Drag,
        Fading,
        Hidden

    }

    public class VirtualScrollbar : VirtualElement {

        public readonly UIElement targetElement;
        public readonly ScrollbarOrientation orientation;

        public Rect trackRect;
        public Rect handleRect;
        public Rect incrementButtonRect;
        public Rect decrementButtonRect;

        public Mesh trackMesh;
        public Mesh handleMesh;
        public Mesh incrementButtonMesh;
        public Mesh decrementButtonMesh;

        public Material trackMaterial;
        public Material handleMaterial;
        public Material incrementButtonMaterial;
        public Material decrementButtonMaterial;

        public ScrollState scrollState;
        
        public VirtualScrollbar(UIElement target, ScrollbarOrientation orientation) {
            this.targetElement = target;
            this.parent = target;

            this.siblingIndex = int.MaxValue - (orientation == ScrollbarOrientation.Horizontal ? 1 : 0);

            this.children = ArrayPool<UIElement>.Empty;
            this.orientation = orientation;

            if (target.isEnabled) {
                flags |= UIElementFlags.AncestorEnabled;
            }

            scrollState = ScrollState.Normal;
            Renderer = ElementRenderer.DefaultScrollbar;
            trackMaterial = StandardRenderer.CreateMaterial();
            handleMaterial = StandardRenderer.CreateMaterial();
            incrementButtonMaterial = StandardRenderer.CreateMaterial();
            decrementButtonMaterial = StandardRenderer.CreateMaterial();
        }


        public Vector2 handlePosition => new Vector2(
            orientation == ScrollbarOrientation.Vertical ? 0 : targetElement.scrollOffset.x * (targetElement.layoutResult.AllocatedWidth - handleWidth),
            orientation == ScrollbarOrientation.Horizontal ? 0 : targetElement.scrollOffset.y * (targetElement.layoutResult.AllocatedHeight - handleHeight)
        );

        public float handleWidth => orientation == ScrollbarOrientation.Vertical
            ? 0
            : (targetElement.layoutResult.AllocatedWidth / targetElement.layoutResult.actualSize.width) * targetElement.layoutResult.AllocatedWidth;

        public float handleHeight => orientation == ScrollbarOrientation.Horizontal
            ? 0
            : (targetElement.layoutResult.AllocatedHeight / targetElement.layoutResult.actualSize.height) * targetElement.layoutResult.AllocatedHeight;

        public bool IsVertical => orientation == ScrollbarOrientation.Vertical;

        [OnDragCreate]
        public ScrollbarDragEvent CreateDragEvent(MouseInputEvent evt) {
            if (handleRect.Contains(evt.MouseDownPosition)) {
                float baseOffset;
                if (orientation == ScrollbarOrientation.Vertical) {
                    baseOffset = evt.MouseDownPosition.y - (trackRect.y + handlePosition.y);
                }
                else {
                    baseOffset = evt.MouseDownPosition.x - (trackRect.x + handlePosition.x);
                }

                return new ScrollbarDragEvent(baseOffset, this);
            }

            return null;
        }

        internal void OnMouseEnter() {
            scrollState = ScrollState.Normal;
        }

        internal void OnMouseMoveOrHover() {
            scrollState = ScrollState.Hover;
        }

        internal void OnMouseExit() {
            scrollState = ScrollState.Normal;
        }
        
        public void RunLayout() {
            ComputedStyle targetStyle = targetElement.ComputedStyle;
            LayoutResult targetResult = targetElement.layoutResult;

            if (orientation == ScrollbarOrientation.Vertical) {
                float trackSize = targetStyle.ScrollbarVerticalTrackSize;
                float incrementButtonSize = targetStyle.ScrollbarVerticalIncrementSize;
                float decrementButtonSize = targetStyle.ScrollbarVerticalDecrementSize;
                float totalTrackHeight = targetResult.allocatedSize.height;

                if (targetStyle.OverflowX != Overflow.None && targetStyle.OverflowX != Overflow.Hidden) {
                    totalTrackHeight -= targetStyle.ScrollbarHorizontalTrackSize;
                }
                
                ScrollbarButtonPlacement placement = targetStyle.ScrollbarVerticalButtonPlacement;

                switch (placement) {
                    case ScrollbarButtonPlacement.Unset:
                    case ScrollbarButtonPlacement.Hidden:
                        trackRect.y = 0;
                        trackRect.height = totalTrackHeight;
                        incrementButtonRect.height = 0;
                        decrementButtonRect.height = 0;
                        break;
                    case ScrollbarButtonPlacement.TogetherAfter:
                        trackRect.y = 0;
                        trackRect.height = totalTrackHeight - (incrementButtonSize + decrementButtonSize);
                        incrementButtonRect.y = trackRect.height;
                        incrementButtonRect.height = incrementButtonSize;
                        decrementButtonRect.y = trackRect.height + incrementButtonSize;
                        decrementButtonRect.height = decrementButtonSize;
                        break;
                    case ScrollbarButtonPlacement.TogetherBefore:
                        trackRect.y = incrementButtonSize + decrementButtonSize;
                        trackRect.height = totalTrackHeight - trackRect.y;
                        incrementButtonRect.y = 0;
                        incrementButtonRect.height = incrementButtonSize;
                        decrementButtonRect.y = incrementButtonSize;
                        decrementButtonRect.height = decrementButtonSize;
                        break;
                    case ScrollbarButtonPlacement.Apart:
                        trackRect.y = decrementButtonSize;
                        trackRect.height = totalTrackHeight - trackRect.y;
                        incrementButtonRect.y = 0;
                        incrementButtonRect.height = incrementButtonSize;
                        decrementButtonRect.y = incrementButtonSize + trackRect.height;
                        decrementButtonRect.height = decrementButtonSize;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                handleRect.height = (targetResult.AllocatedHeight / targetResult.actualSize.height) * targetResult.AllocatedHeight;
                trackRect.width = trackSize;
                incrementButtonRect.width = trackSize;
                decrementButtonRect.width = trackSize;
                handleRect.width = targetStyle.ScrollbarVerticalHandleSize;

                float elementWidth = targetStyle.OverflowX == Overflow.None
                    ? targetElement.layoutResult.actualSize.width
                    : targetElement.layoutResult.allocatedSize.width;

                float offsetX = targetStyle.ScrollbarVerticalAttachment == VerticalScrollbarAttachment.Left
                    ? 0
                    : elementWidth - trackRect.width;

                trackRect.x = offsetX;
                handleRect.x = offsetX;
                incrementButtonRect.x = offsetX;
                decrementButtonRect.x = offsetX;
            }
            else {
                float trackSize = targetStyle.ScrollbarHorizontalTrackSize;
                float incrementButtonSize = targetStyle.ScrollbarHorizontalIncrementSize;
                float decrementButtonSize = targetStyle.ScrollbarHorizontalDecrementSize;
                float totalTrackWidth = targetResult.allocatedSize.width;

                if (targetStyle.OverflowY != Overflow.None && targetStyle.OverflowY != Overflow.Hidden) {
                    totalTrackWidth -= targetStyle.ScrollbarVerticalTrackSize;
                }
                
                ScrollbarButtonPlacement placement = targetStyle.ScrollbarHorizontalButtonPlacement;

                switch (placement) {
                    case ScrollbarButtonPlacement.Unset:
                    case ScrollbarButtonPlacement.Hidden:
                        trackRect.x = 0;
                        trackRect.width = totalTrackWidth;
                        incrementButtonRect.width = 0;
                        decrementButtonRect.width = 0;
                        break;
                    case ScrollbarButtonPlacement.TogetherAfter:
                        trackRect.x = 0;
                        trackRect.width = totalTrackWidth - (incrementButtonSize + decrementButtonSize);
                        incrementButtonRect.x = trackRect.width;
                        incrementButtonRect.width = incrementButtonSize;
                        decrementButtonRect.x = trackRect.width + incrementButtonSize;
                        decrementButtonRect.width = decrementButtonSize;
                        break;
                    case ScrollbarButtonPlacement.TogetherBefore:
                        trackRect.x = incrementButtonSize + decrementButtonSize;
                        trackRect.width = totalTrackWidth - trackRect.x;
                        incrementButtonRect.x = 0;
                        incrementButtonRect.width = incrementButtonSize;
                        decrementButtonRect.x = incrementButtonSize;
                        decrementButtonRect.width = decrementButtonSize;
                        break;
                    case ScrollbarButtonPlacement.Apart:
                        trackRect.x = incrementButtonSize;
                        trackRect.width = totalTrackWidth - trackRect.x;
                        incrementButtonRect.x = 0;
                        incrementButtonRect.width = incrementButtonSize;
                        decrementButtonRect.x = incrementButtonSize + trackRect.width;
                        decrementButtonRect.width = decrementButtonSize;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                handleRect.width = (targetResult.AllocatedWidth / targetResult.ActualWidth) * targetResult.AllocatedWidth;
                
                trackRect.height = trackSize;
                incrementButtonRect.height = trackSize;
                decrementButtonRect.height = trackSize;
                handleRect.height = targetStyle.ScrollbarHorizontalHandleSize;

                float elementHeight = targetStyle.OverflowY == Overflow.None
                    ? targetElement.layoutResult.ContentRect.height
//                    ? targetElement.layoutResult.actualSize.height
                    : targetElement.layoutResult.allocatedSize.height;

                float offsetY = targetStyle.ScrollbarHorizontalAttachment == HorizontalScrollbarAttachment.Top
                    ? 0
                    : elementHeight - trackRect.height;

                trackRect.y = offsetY;
                handleRect.y = offsetY;
                incrementButtonRect.y = offsetY;
                decrementButtonRect.y = offsetY;
            }
        }

    }

}