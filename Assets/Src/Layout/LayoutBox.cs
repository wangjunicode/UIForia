//using System;
//using System.Diagnostics;
//using Rendering;
//using Src.Elements;
//using Src.Systems;
//using Debug = UnityEngine.Debug;
//
//namespace Src.Layout {
//
//    [DebuggerDisplay("{element.ToString}")]
//    public class LayoutBox {
//
//        [Flags]
//        public enum LayoutBoxDirtyFlags {
//
//            None = 0,
//            PreferredWidth = 1 << 0,
//            PreferredHeight = 1 << 1,
//            MinWidth = 1 << 2,
//            MaxWidth = 1 << 3,
//            MinHeight = 1 << 4,
//            MaxHeight = 1 << 5,
//            Children = 1 << 6,
//            
//            Width = MinWidth | MaxWidth | PreferredWidth,
//            Height = MinHeight | MaxHeight | PreferredHeight,
//
//        }
//
//        public LayoutBox parent;
//        public LayoutBox firstChild;
//        public LayoutBox nextSibling;
//        public LayoutBox prevSibling;
//
//        public Layout2 layout;
//
//        public UIElement element;
//        public LayoutSystem2 layoutSystem;
//
//        public float preferredWidth;
//        public float preferredHeight;
//        public float minWidth;
//        public float minHeight;
//        public float maxWidth;
//        public float maxHeight;
//        public float computedX;
//        public float computedY;
//        public float allocatedWidth;
//        public float allocatedHeight;
//        public float actualWidth;
//        public float actualHeight;
//
//        public int activeChildCount;
//        public int totalChildCount;
//        public LayoutUpdateType layoutUpdateType;
//        public UIStyle style;
//        
//        public bool isTextElement;
//        public bool isImageElement;
//        public bool isShapeElement;
//        public bool movedThisFrame;
//
//        public LayoutBoxDirtyFlags dirtyFlags;
//        
//        public LayoutBox(LayoutSystem2 layoutSystem, UIElement element) {
//            this.layoutSystem = layoutSystem;
//            this.element = element;
//            this.layoutUpdateType = LayoutUpdateType.Full;
//            this.style = element?.style?.computedStyle;
//
//            this.isImageElement = (element as UIImageElement) != null;
//            this.isTextElement = (element as UITextElement) != null;
//            this.isShapeElement = (element as UIShapeElement) != null;
//            this.dirtyFlags = LayoutBoxDirtyFlags.Height | LayoutBoxDirtyFlags.Width;
//        }
//
//        public bool NeedsLayout => (dirtyFlags != LayoutBoxDirtyFlags.None);
//
//        public void SetTextSize(TextSizeResult textSize) {
//            preferredWidth = textSize.preferredWidth;
//            minWidth = textSize.minWidth;
//            dirtyFlags |= LayoutBoxDirtyFlags.Width;
//        }
//
//        public void SetLayoutParameters(LayoutParameters layoutParameters) { }
//
//        public void SetContentBox(ContentBoxRect rect) { }
//
//        private float ResolveFixedWidth(UIMeasurement measurement) {
//            switch (measurement.unit) {
//                case UIUnit.Pixel:
//                    return measurement.value;
//                case UIUnit.ParentSize:
//                    return measurement.value * parent.allocatedWidth;
//                case UIUnit.View:
//                    return measurement.value * layoutSystem.ViewportRect.width;
//                case UIUnit.ParentContentArea:
//                    // todo subtract parent content area
//                    return (parent.allocatedWidth) * measurement.value;
//                case UIUnit.Em:
//                    return measurement.value * style.textStyle.fontSize;
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
//        }
//        
//        private float ResolveFixedHeight(UIMeasurement measurement) {
//            switch (measurement.unit) {
//                case UIUnit.Pixel:
//                    return measurement.value;
//                case UIUnit.ParentSize:
//                    return measurement.value * parent.allocatedHeight;
//                case UIUnit.View:
//                    return measurement.value * layoutSystem.ViewportRect.height;
//                case UIUnit.ParentContentArea:
//                    // todo subtract parent content area
//                    return (parent.allocatedHeight) * measurement.value;
//                case UIUnit.Em:
//                    return measurement.value * style.textStyle.fontSize;
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
//        }
//
//        public void OnChildPreferredWidthChanged(LayoutBox child) {
//            switch (style.layoutParameters.type) {
//                case LayoutType.Unset:
//                    break;
//                case LayoutType.Flow:
//                    break;
//                case LayoutType.Flex:
//
//                    break;
//                case LayoutType.Fixed:
//                    break;
//                case LayoutType.Grid:
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
//        }
//
//        public void OnChildMinWidthChanged(LayoutBox child) {
//            // if child actual width < resolved min  -> do layout
//        }
//
//        public void OnChildMaxWidthChanged(LayoutBox child) {
//            // if child actual width > resolved max  -> do layout
//        }
//
//        public void SetFontSize(int fontSize) {
//            if (style.dimensions.width.unit == UIUnit.Em) {
//                float newPrefWidth = fontSize * style.dimensions.width.value;
//                if (newPrefWidth != preferredWidth) {
//                    preferredWidth = newPrefWidth;
//                    parent.OnChildPreferredWidthChanged(this);
//                }
//            }
//
//            if (style.layoutConstraints.minWidth.unit == UIUnit.Em) {
//                float newMinWidth = fontSize * style.layoutConstraints.minWidth.value;
//                if (newMinWidth != preferredWidth) {
//                    preferredWidth = newMinWidth;
//                    parent.OnChildMinWidthChanged(this);
//                }
//            }
//
//            if (style.layoutConstraints.maxWidth.unit == UIUnit.Em) {
//                float newMaxWidth = fontSize * style.layoutConstraints.maxWidth.value;
//                if (newMaxWidth != preferredWidth) {
//                    preferredWidth = newMaxWidth;
//                    parent.OnChildMaxWidthChanged(this);
//                }
//            }
//        }
//
//        public void UpdateWidthValues() {
//            if ((dirtyFlags & LayoutBoxDirtyFlags.Width) == 0) {
//                return;
//            }
//
//            if ((dirtyFlags & LayoutBoxDirtyFlags.MinWidth) != 0) {
//                UIMeasurement minWidthMeasurement = style.layoutConstraints.minWidth;
//                if (!minWidthMeasurement.IsDefined()) {
//                    minWidth = 0;
//                }
//                else if (minWidthMeasurement.isFixed) {
//                    minWidth = ResolveFixedWidth(minWidthMeasurement);
//                }
//                else {
//                    minWidth = layout.GetContentMinWidth(this);
//                }
//
//                dirtyFlags &= ~LayoutBoxDirtyFlags.MinWidth;
//            }
//
//            if ((dirtyFlags & LayoutBoxDirtyFlags.MaxWidth) != 0) {
//                UIMeasurement maxWidthMeasurement = style.layoutConstraints.maxWidth;
//
//                if (!maxWidthMeasurement.IsDefined()) {
//                    maxWidth = float.MaxValue;
//                }
//                else if (maxWidthMeasurement.isFixed) {
//                    minWidth = ResolveFixedWidth(maxWidthMeasurement);
//                }
//                else {
//                    minWidth = layout.GetContentMaxWidth(this);
//                }
//
//                dirtyFlags &= ~LayoutBoxDirtyFlags.MaxWidth;
//            }
//
//            if ((dirtyFlags & LayoutBoxDirtyFlags.PreferredWidth) != 0) {
//                UIMeasurement prefWidthMeasurement = style.dimensions.width;
//                if (!prefWidthMeasurement.IsDefined()) {
//                    preferredWidth = ResolveFixedWidth(new UIMeasurement(1f, UIUnit.ParentContentArea));
//                }
//                else if (prefWidthMeasurement.isFixed) {
//                    preferredWidth = ResolveFixedWidth(prefWidthMeasurement);
//                }
//                else {
//                    preferredWidth = layout.GetContentPreferredWidth(this);
//                }
//
//                dirtyFlags &= ~LayoutBoxDirtyFlags.PreferredWidth;
//            }
//        }
//        
//        public void UpdateHeightValues() {
//            if ((dirtyFlags & LayoutBoxDirtyFlags.Height) == 0) {
//                return;
//            }
//
//            if ((dirtyFlags & LayoutBoxDirtyFlags.MinHeight) != 0) {
//                UIMeasurement minHeightMeasurement = style.layoutConstraints.minHeight;
//                if (!minHeightMeasurement.IsDefined()) {
//                    minHeight = 0;
//                }
//                else if (minHeightMeasurement.isFixed) {
//                    minHeight = ResolveFixedHeight(minHeightMeasurement);
//                }
//                else {
//                    minHeight = layout.GetContentMinHeight(this, float.MaxValue);
//                }
//
//                dirtyFlags &= ~LayoutBoxDirtyFlags.MinHeight;
//            }
//
//            if ((dirtyFlags & LayoutBoxDirtyFlags.MaxHeight) != 0) {
//                UIMeasurement maxHeightMeasurement = style.layoutConstraints.maxHeight;
//
//                if (!maxHeightMeasurement.IsDefined()) {
//                    maxHeight = float.MaxValue;
//                }
//                else if (maxHeightMeasurement.isFixed) {
//                    minHeight = ResolveFixedHeight(maxHeightMeasurement);
//                }
//                else {
//                    minHeight = layout.GetContentMaxHeight(this, float.MaxValue);
//                }
//
//                dirtyFlags &= ~LayoutBoxDirtyFlags.MaxHeight;
//            }
//
//            if ((dirtyFlags & LayoutBoxDirtyFlags.PreferredHeight) != 0) {
//                UIMeasurement prefHeightMeasurement = style.dimensions.width;
//                if (!prefHeightMeasurement.IsDefined()) {
//                    preferredHeight = ResolveFixedHeight(new UIMeasurement(1f, UIUnit.ParentContentArea));
//                }
//                else if (prefHeightMeasurement.isFixed) {
//                    preferredHeight = ResolveFixedHeight(prefHeightMeasurement);
//                }
//                else {
//                    preferredHeight = layout.GetContentPreferredHeight(this, float.MaxValue);
//                }
//
//                dirtyFlags &= ~LayoutBoxDirtyFlags.PreferredHeight;
//            }
//        }
//
//        public void UpdateHeightValuesUsingWidth(float width) {
//            if (isTextElement) {
//                // measure text in width         
//            }
//
//            if (!style.dimensions.height.isFixed) {
//                preferredHeight = layout.GetContentPreferredHeight(this, width);
//            }
//        }
//
//        public float GetPreferredHeightForWidth(float width) {
//            if (style.dimensions.height.isFixed) {
//                return preferredHeight;
//            }
//            else {
//                return layout.GetContentPreferredHeight(this, width);
//            }
//        }
//
//        // basic rule: ONLY THE PARENT WILL EVER SET SIZE FOR A CHILD
//        // the child can only set its preferred values and inform its parent layout about it
//
//        internal void SetRectFromUser(float x, float y, float width, float height) {
//            
//        }
//
//        public float GetPreferredHeightFromWidth(float width) {
//            return 0f;
//        }
//
//        internal void SetRectFromParentLayout(float x, float y, float width, float height) {
//            // todo -- assert 1 call per layout cycle
//            computedX = x;
//            computedY = y;
//
//            if ((int)allocatedWidth != (int)width) {
//                allocatedWidth = width;
//                LayoutBox ptr = firstChild;
//                while (ptr != null) {
//                    ptr.OnParentComputedWidthChanged();
//                    ptr = parent.nextSibling;
//                }
//            }
//            // if flex & can grow / shrink to fit new allocated space or requires full layout 
//            // run layout
//            if (allocatedHeight != height) {
//                allocatedHeight = height;
//                if (layout.UpdateDimensions(this, allocatedWidth, allocatedHeight)) {
//                    
//                }
//                // ComputeActualWidth(allocatedWidth);
//                // ComputeActualHeight(allocatedWidth, allocatedHeight);
//                LayoutBox ptr = firstChild;
//                while (ptr != null) {
//                    ptr.OnParentComputedHeightChanged(allocatedHeight);
//                    ptr = parent.nextSibling;
//                }
//            }
//            
//            layoutSystem.OnRectChanged(this);
//            
//        }
//
//        public void SetSize(Dimensions dimensions) {
//            UIMeasurement widthMeasurement = style.dimensions.width;
//            switch (widthMeasurement.unit) { }
//
//            // parent.OnChildPreferredSizeChanged(); -> parent might mark for layout
//            // for each child
//            //     if child.width or child.contentbox is parent relative
//            //    dirtyFlags |= requires full layout
//        }
//
//        private void UpdateMinWidth() {
//            if (isTextElement) {
//                return;
//            }
//
//            switch (element.style.constraints.minWidth.unit) { }
//        }
//
//        private void UpdateMaxWidth() { }
//
//        // intrinsic width = width the item things it should be: ie preferred width
//        // intrinsic override = width of item for things like text or image respecting aspect ratio
//        // logical width = width the container says it should be, ie could be stretched or shrunk
//        
//
//        public void SetWidth(float actualWidthValue) { }
//
//        public void SetHeight(float actualHeightValue) { }
//
//        public void SetX(float actualXValue) { }
//
//        public void SetY(float actualYValue) { }
//
//        public virtual void OnStyleChanged() { }
//
//        public void RunLayout() {
//            layout.Run(this, LayoutUpdateType.Full);
//        }
//
//        public void OnParentComputedWidthChanged() {
//
//            // if margin / padding / border changed -> might change preferred width, ask layout if we need to update
//
//            UIMeasurement minWidthMeasurement = style.layoutConstraints.minWidth;
//            UIMeasurement maxWidthMeasurement = style.layoutConstraints.maxWidth;
//            UIMeasurement prfWidthMeasurement = style.dimensions.width;
//            
//            // can directly resolve these here if required
//            
//            if (minWidthMeasurement.isParentRelative) {
//                dirtyFlags |= LayoutBoxDirtyFlags.MinWidth;
//            }
//
//            if (maxWidthMeasurement.isParentRelative) {
//                dirtyFlags |= LayoutBoxDirtyFlags.MaxWidth;
//            }
//
//            if (prfWidthMeasurement.isParentRelative) {
//                dirtyFlags |= LayoutBoxDirtyFlags.PreferredWidth;
//            }
//            
//            // computed size = space parent allocates to us
//            // actual size = the space we actually require to render which might overflow
//
//            // might need layout here, how do I know?
//            // if position changes -> need to do a position layout pass which might just be a no-op actually
//            // if computed size changes -> 
//            // if overflow size changes -> 
//            // layout runs with desired computed size
//            // if overflowing, child layout will still try to fit into it's box, it might overflow / under flow, thats fine.
//            
//            // if unit cares about parent width
//            // && layout cares about our preferred width
//            // layoutUpdateType |= layout.OnPreferredWidthChanged(min, max, preferred);
//            // layoutUpdateType |= layout.OnMinWidthChanged(min, max, preferred);
//            // layoutUpdateType |= layout.OnMaxWidthChanged(min, max, preferred);
//          
//        }
//
//        public void OnParentComputedHeightChanged(float height) {
//            
//        }
//
//        public virtual void OnParentHeightChanged() { }
//
//        public void SetParent(LayoutBox parent, int index = -1) {
//            this.parent?.RemoveChild(this);
//            this.parent = parent;
//            this.parent.InsertChild(this, index);
//        }
//
//        public void InsertChild(LayoutBox child, int index) {
//            totalChildCount++;
//
//            dirtyFlags |= LayoutBoxDirtyFlags.Children;
//            
//            if (child.element.isEnabled) {
//                activeChildCount++;
//                layoutSystem.RequestLayout(this);
//            }
//            
//            child.prevSibling = null;
//            child.nextSibling = null;
//            
//            if (firstChild == null) {
//                firstChild = child;
//                return;
//            }
//
//            if (index < 0 || index >= totalChildCount) {
//                index = totalChildCount - 1;
//            }
//
//            if (index == 0) {
//                firstChild.prevSibling = child;
//                child.nextSibling = firstChild;
//                firstChild = child;
//            }
//            else {
//                LayoutBox ptr = firstChild;
//                
//                for (int i = 1; i < totalChildCount; i++) {
//                    if (i == index) {
//                        child.prevSibling = ptr;
//                        child.nextSibling = ptr.nextSibling;
//                        LayoutBox next = ptr.nextSibling;
//                        ptr.nextSibling = child;
//                        if (next != null) {
//                            next.prevSibling = child;
//                        }
//                        return;
//                    }
//                    ptr = ptr.nextSibling;
//                }               
//                Debug.Assert(false, "Never Get Here");
//            }
//        }
//        
//        public void AppendChild(LayoutBox child) {
//            InsertChild(child, int.MaxValue);
//        }
//
//        public void PrependChild(LayoutBox child) {
//            InsertChild(child, 0);
//        }
//
//        public void RemoveChild(LayoutBox child) {
//            if (child.nextSibling != null) {
//                child.nextSibling.prevSibling = child.prevSibling;
//            }
//
//            if (child.prevSibling != null) {
//                child.prevSibling.nextSibling = child.nextSibling;
//            }
//
//            dirtyFlags |= LayoutBoxDirtyFlags.Children;
//            
//            totalChildCount--;
//            if (child.element.isEnabled) {
//                activeChildCount--;
//                layoutSystem.RequestLayout(this);
//            }
//            
//            child.parent = null;
//        }
//
//        public virtual float GetWidth() {
//            // if ignore rotation
//            // if !ignoreScale
//
//            return 0;
//        }
//
//    }
//
//}