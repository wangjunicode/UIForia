using System;
using Rendering;
using Src.Elements;
using Src.Systems;
using UnityEngine;

namespace Src.Layout {

    public class LayoutBox {

        [Flags]
        public enum LayoutBoxDirtyFlags {

            None = 0,
            PreferredWidth = 1 << 0,
            PreferredHeight = 1 << 1,
            MinWidth = 1 << 2,
            MaxWidth = 1 << 3,
            MinHeight = 1 << 4,
            MaxHeight = 1 << 5,
            Width = MinWidth | MaxWidth | PreferredWidth,
            Height = MinHeight | MaxHeight | PreferredHeight

        }

        public LayoutBox parent;
        public LayoutBox firstChild;
        public LayoutBox nextSibling;
        public LayoutBox prevSibling;

        public Layout2 layout;

        public UIElement element;
        public LayoutSystem2 layoutSystem;
        public bool sizingDirty;

        public float preferredWidth;
        public float preferredHeight;
        public float minWidth;
        public float minHeight;
        public float maxWidth;
        public float maxHeight;
        public float computedWidth;
        public float computedHeight;

        public int activeChildCount;
        public int totalChildCount;
        public LayoutUpdateType layoutUpdateType;
        public UIStyle style;

        public LayoutBox(LayoutSystem2 layoutSystem, UIElement element) {
            this.layoutSystem = layoutSystem;
            this.element = element;
            this.layoutUpdateType = LayoutUpdateType.Full;
            this.style = element?.style.computedStyle;

            this.isImageElement = (element as UIImageElement) != null;
            this.isTextElement = (element as UITextElement) != null;
            this.isShapeElement = (element as UIShapeElement) != null;
            this.dirtyFlags = LayoutBoxDirtyFlags.Height | LayoutBoxDirtyFlags.Width;
        }

        public bool isTextElement;
        public bool isImageElement;
        public bool isShapeElement;
        public bool movedThisFrame;

        public LayoutBoxDirtyFlags dirtyFlags;

        public bool NeedsLayout => (dirtyFlags != LayoutBoxDirtyFlags.None);

        public void SetTextSize(TextSizeResult textSize) {
            preferredWidth = textSize.preferredWidth;
            minWidth = textSize.minWidth;
            dirtyFlags |= LayoutBoxDirtyFlags.Width;
        }

        public void SetLayoutParameters(LayoutParameters layoutParameters) { }

        public void SetContentBox(ContentBoxRect rect) { }

        private float ResolveFixedWidth(UIMeasurement measurement) {
            switch (measurement.unit) {
                case UIUnit.Pixel:
                    return measurement.value;
                case UIUnit.Parent:
                    return measurement.value * parent.computedWidth;
                case UIUnit.View:
                    return measurement.value * layoutSystem.ViewportRect.width;
                case UIUnit.FillAvailableSpace:
                    return 0; //(parent.computedWidth - parent.horizontal) * measurement.value;
                case UIUnit.Em:
                    return measurement.value * style.textStyle.fontSize;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnChildPreferredWidthChanged(LayoutBox child) {
            switch (style.layoutParameters.type) {
                case LayoutType.Unset:
                    break;
                case LayoutType.Flow:
                    break;
                case LayoutType.Flex:

                    break;
                case LayoutType.Fixed:
                    break;
                case LayoutType.Grid:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnChildMinWidthChanged(LayoutBox child) {
            // if child actual width < resolved min  -> do layout
        }

        public void OnChildMaxWidthChanged(LayoutBox child) {
            // if child actual width > resolved max  -> do layout
        }

        public void SetFontSize(int fontSize) {
            if (style.dimensions.width.unit == UIUnit.Em) {
                float newPrefWidth = fontSize * style.dimensions.width.value;
                if (newPrefWidth != preferredWidth) {
                    preferredWidth = newPrefWidth;
                    parent.OnChildPreferredWidthChanged(this);
                }
            }

            if (style.layoutConstraints.minWidth.unit == UIUnit.Em) {
                float newMinWidth = fontSize * style.layoutConstraints.minWidth.value;
                if (newMinWidth != preferredWidth) {
                    preferredWidth = newMinWidth;
                    parent.OnChildMinWidthChanged(this);
                }
            }

            if (style.layoutConstraints.maxWidth.unit == UIUnit.Em) {
                float newMaxWidth = fontSize * style.layoutConstraints.maxWidth.value;
                if (newMaxWidth != preferredWidth) {
                    preferredWidth = newMaxWidth;
                    parent.OnChildMaxWidthChanged(this);
                }
            }
        }

        public void UpdateWidthValues() {
            if ((dirtyFlags & LayoutBoxDirtyFlags.Width) == 0) {
                return;
            }

            if ((dirtyFlags & LayoutBoxDirtyFlags.MinWidth) != 0) {
                if (style.layoutConstraints.minWidth.isFixed) {
                    minWidth = ResolveFixedWidth(style.layoutConstraints.minWidth);
                }
                else {
                    minWidth = layout.GetContentMinWidth(this);
                }

                dirtyFlags &= ~LayoutBoxDirtyFlags.MinWidth;
            }

            if ((dirtyFlags & LayoutBoxDirtyFlags.MaxWidth) != 0) {
                if (style.layoutConstraints.maxWidth.isFixed) {
                    minWidth = ResolveFixedWidth(style.layoutConstraints.maxWidth);
                }
                else {
                    minWidth = layout.GetContentMaxWidth(this);
                }

                dirtyFlags &= ~LayoutBoxDirtyFlags.MaxWidth;
            }

            if ((dirtyFlags & LayoutBoxDirtyFlags.PreferredWidth) != 0) {
                if (style.layoutConstraints.maxWidth.isFixed) {
                    minWidth = ResolveFixedWidth(style.layoutConstraints.maxWidth);
                }
                else {
                    minWidth = layout.GetContentPreferredWidth(this);
                }

                dirtyFlags &= ~LayoutBoxDirtyFlags.PreferredWidth;
            }
        }

        public void UpdateHeightValuesUsingWidth(float width) {
            if (isTextElement) {
                // measure text in width         
            }

            if (!style.dimensions.height.isFixed) {
                preferredHeight = layout.GetContentPreferredHeight(this, width);
            }
        }

        public float GetPreferredHeightForWidth(float width) {
            if (style.dimensions.height.isFixed) {
                return preferredHeight;
            }
            else {
                return layout.GetContentPreferredHeight(this, width);
            }
        }

        // basic rule: ONLY THE PARENT WILL EVER SET SIZE FOR A CHILD
        // the child can only set its preferred values and inform its parent layout about it

        internal void SetRectFromLayout(float x, float y, float width, float height) {
            // todo -- assert 1 call per layout cycle
//            computedX = x;
//            computedY = y;

            if (computedWidth != width) {
                computedWidth = width;
                LayoutBox ptr = firstChild;
                while (ptr != null) {
                    ptr.OnParentComputedWidthChanged(computedWidth);
                    ptr = parent.nextSibling;
                }
            }
            
            if (computedHeight != height) {
                computedHeight = height;
                LayoutBox ptr = firstChild;
                while (ptr != null) {
                    ptr.OnParentComputedHeightChanged(computedHeight);
                    ptr = parent.nextSibling;
                }
            }
            
            layoutSystem.OnRectChanged(this);
            
        }

        public void SetSize(Dimensions dimensions) {
            UIMeasurement widthMeasurement = style.dimensions.width;
            switch (widthMeasurement.unit) { }

            // parent.OnChildPreferredSizeChanged(); -> parent might mark for layout
            // for each child
            //     if child.width or child.contentbox is parent relative
            //    dirtyFlags |= requires full layout
        }

        private void UpdateMinWidth() {
            if (isTextElement) {
                return;
            }

            switch (element.style.constraints.minWidth.unit) { }
        }

        private void UpdateMaxWidth() { }

        // intrinsic width = width the item things it should be: ie preferred width
        // intrinsic override = width of item for things like text or image respecting aspect ratio
        // logical width = width the container says it should be, ie could be stretched or shrunk
        public void SetParent(LayoutBox parent, int index = -1) {
            this.parent?.RemoveChild(this);
            this.parent = parent;
            this.parent?.InsertChild(this, index);
        }

        public void InsertChild(LayoutBox child, int index) {
            if (firstChild == null) {
                firstChild = child;
                return;
            }

            if (index < 0) {
                index = int.MaxValue;
            }

            if (index == 0) { }
            else { }
        }

        public void SetWidth(float actualWidthValue) { }

        public void SetHeight(float actualHeightValue) { }

        public void SetX(float actualXValue) { }

        public void SetY(float actualYValue) { }

        public virtual void OnStyleChanged() { }

        public void RunLayout() {
            layout.Run(this, LayoutUpdateType.Full);
        }

        public void OnParentComputedWidthChanged(float totalWidth) {
            
            // if unit cares about parent width
            // && layout cares about our preferred width
            // layoutUpdateType |= layout.OnPreferredWidthChanged(min, max, preferred);
            // layoutUpdateType |= layout.OnMinWidthChanged(min, max, preferred);
            // layoutUpdateType |= layout.OnMaxWidthChanged(min, max, preferred);
            //  todo -- this section needs more help to figure out if we need to run the layout again or not
            
            // mark for layout
            
            // if margin / padding / border changed -> might change preferred width, ask layout if we need to update
            
            // if stretched -> width = stretch area; return

            // mark for update preferred / min / max / padding / margin
            // does not update actual width
            // might need to mark for layout pass
            // if
            //    - any child has parent based value
            //    - 
            // update preferred values
            // min and max should resolve to fixed values here
            // update margin and padding
//            switch (element.style.width.unit) {
//                case UIUnit.FillAvailableSpace:
//                    LayoutBox ptr = firstChild;
//                    // compute padding / margin / border if relative
//                    // update min width
//                    float width = contentAreaWidth * element.style.width.value;
//                    // mark for layout
//                    // clamp at 0 for parent based on content size
////                    while (ptr != null) {
////                        ptr.OnParentWidthChanged(width, width - style.paddingAndBorderHorizontal, viewportWidth);
////                        ptr = ptr.nextSibling;
////                    }
//
//                    selfNeedsLayout = true;
//                    // if preserve aspect ratio -> update height
//                    break;
//
//                case UIUnit.Content:
//                    // do nothing
//                    break;
//                case UIUnit.Parent:
//                    // do nothing if parent is content sized or find next fixed container and take that size
//                    // if preserve aspect ratio -> update height
//
//                    break;
//                case UIUnit.View:
//                    break;
//                case UIUnit.Pixel:
//                    break;
//            }
        }

        public void OnParentComputedHeightChanged(float height) {
            
        }

        public virtual void OnParentHeightChanged() { }

        public void AppendChild(LayoutBox child) {
            if (firstChild == null) {
                firstChild = child;
                return;
            }

            LayoutBox ptr = firstChild;
            while (ptr.nextSibling != null) {
                ptr = ptr.nextSibling;
            }

            ptr.nextSibling = child;
            child.prevSibling = ptr;
        }

        public void RemoveChild(LayoutBox child) {
            if (child.nextSibling != null) {
                child.nextSibling.prevSibling = child.prevSibling;
            }

            if (child.prevSibling != null) {
                child.prevSibling.nextSibling = child.nextSibling;
            }

            child.parent = null;
            //NotifyLayoutRequired
           
        }

        public virtual float GetWidth() {
            // if ignore rotation
            // if !ignoreScale

            return 0;
        }

    }

}