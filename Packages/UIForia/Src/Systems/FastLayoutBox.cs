using System;
using System.Diagnostics;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public struct SizeConstraints {

        public float minWidth;
        public float maxWidth;
        public float prefWidth;
        public float minHeight;
        public float maxHeight;
        public float prefHeight;

    }

    public abstract class FastLayoutBox {

        public LayoutRenderFlag flags;
        public FastLayoutBox relayoutBoundary;

        public FastLayoutBox parent;
        public FastLayoutBox firstChild;
        public FastLayoutBox nextSibling;
        public FastLayoutBox prevSibling; // maybe drop since we use index traversal anyway when adding / removing

        // optimized to use bits for units & holds resolved value 
        public OffsetRect paddingBox;
        public OffsetRect borderBox;

        public UIMeasurement marginTop;
        public UIMeasurement marginRight;
        public UIMeasurement marginBottom;
        public UIMeasurement marginLeft;
        
        // holds units in bit field
        public MeasurementSet widthMeasurement;
        public MeasurementSet heightMeasurement;

        public FastLayoutSystem layoutSystem;

        public int traversalIndex;
        public bool sizedByParent;

        public Size lastResolvedBlockSize;
        public Size allocatedSize;
        public Size contentSize;
        public Size size;
        public Vector2 allocatedPosition;

        public int depth;

        public Size containingBox;

        public int enabledFrame;

        public Fit selfFitHorizontal;
        public Fit selfFitVertical;

        public Alignment selfAlignmentHorizontal;
        public Alignment selfAlignmentVertical;

        public Alignment targetAlignmentHorizontal;
        public Alignment targetAlignmentVertical;

        public UIElement element;

        protected FastLayoutBox(UIElement element) {
            this.element = element;
        }

        public virtual void AddChild(FastLayoutBox child) {
            if (firstChild == null) {
                firstChild = child;
                return;
            }

            FastLayoutBox ptr = firstChild;
            FastLayoutBox trail = null;
            int idx = 0;

            while (ptr.traversalIndex < child.traversalIndex) {
                ptr = ptr.nextSibling;
                trail = ptr;
                idx++;
            }

            if (trail == null) {
                child.nextSibling = firstChild;
                firstChild.prevSibling = child;
                firstChild = child;
                OnChildAdded(child, idx);
            }
            else {
                child.prevSibling = ptr;
                child.nextSibling = ptr.nextSibling;
                ptr.nextSibling = child;
                OnChildAdded(child, idx);
            }
        }

        public virtual void RemoveChild(FastLayoutBox child) {
            int idx = 0;
            FastLayoutBox ptr = firstChild;
            while (ptr != child) {
                idx++;
                ptr = ptr.nextSibling;
            }

            OnChildRemoved(child, idx);
        }

        public virtual void OnChildAdded(FastLayoutBox child, int index) { }

        public virtual void OnChildRemoved(FastLayoutBox child, int index) { }

        public virtual float GetIntrinsicMinWidth() {
            return 0;
        }

        public virtual float GetIntrinsicMinHeight() {
            return 0;
        }

        public virtual float GetIntrinsicMaxWidth() {
            return float.MaxValue;
        }

        public virtual float GetIntrinsicMaxHeight() {
            return float.MaxValue;
        }

        public virtual float ResolveAutoWidth() {
            return 0;
        }

        public virtual float ResolveAutoHeight() {
            return 0;
        }

        private Vector2 ApplyAlignment() {
            Vector2 retn = default;
            return retn;
        }

        public float ResolveWidth(float resolvedBlockSize, float value, UIMeasurementUnit unit) {
            switch (unit) {
                case UIMeasurementUnit.Content:
                    return ComputeContentWidth(resolvedBlockSize);

                case UIMeasurementUnit.FitContent:
                    float minWidth = GetIntrinsicMinWidth();
                    float pref = GetIntrinsicMaxWidth();
                    return Mathf.Max(minWidth, Mathf.Min(resolvedBlockSize, pref));

                case UIMeasurementUnit.Pixel:
                    return value;

                case UIMeasurementUnit.Em:
                    return 0;

                case UIMeasurementUnit.Percentage:
                    // percentage of last resolved size (ie containing block)
                    return resolvedBlockSize * value;

                case UIMeasurementUnit.ViewportWidth:
                    return element.View.Viewport.width * value;

                case UIMeasurementUnit.ViewportHeight:
                    return element.View.Viewport.height * value;

                case UIMeasurementUnit.MinContent:
                    return GetIntrinsicMinWidth();

                case UIMeasurementUnit.MaxContent:
                    return GetIntrinsicMaxWidth();
            }

            return 0;
        }

        public float ResolveHeight(float width, float resolvedBlockSize, float value, UIMeasurementUnit unit) {
            switch (unit) {
                case UIMeasurementUnit.Content:
                    return ComputeContentHeight(width, resolvedBlockSize);

                case UIMeasurementUnit.FitContent:
                    float minHeight = GetIntrinsicMinHeight();
                    float pref = GetIntrinsicMaxHeight();
                    return Mathf.Max(minHeight, Mathf.Min(resolvedBlockSize, pref));

                case UIMeasurementUnit.Pixel:
                    return value;

                case UIMeasurementUnit.Em:
                    return 0;

                case UIMeasurementUnit.Percentage:
                    // percentage of last resolved size (ie containing block)
                    return resolvedBlockSize * value;

                case UIMeasurementUnit.ViewportWidth:
                    return element.View.Viewport.width * value;

                case UIMeasurementUnit.ViewportHeight:
                    return element.View.Viewport.height * value;

                case UIMeasurementUnit.MinContent:
                    return GetIntrinsicMinHeight();

                case UIMeasurementUnit.MaxContent:
                    return GetIntrinsicMaxHeight();
            }

            return 0;
        }


        public virtual float ComputeContentWidth(float blockWidth) {
            return 0;
        }

        public virtual float ComputeContentHeight(float width, float blockHeight) {
            return 0;
        }

        [DebuggerStepThrough]
        public float ResolveFixedWidth(UIFixedLength width) {
            switch (width.unit) {
                case UIFixedUnit.Pixel:
                    return width.value;

                case UIFixedUnit.Percent:
                    return size.width * width.value;

                case UIFixedUnit.ViewportHeight:
                    return element.View.Viewport.height * width.value;

                case UIFixedUnit.ViewportWidth:
                    return element.View.Viewport.width * width.value;

                case UIFixedUnit.Em:
                    return element.style.GetResolvedFontSize() * width.value;

                case UIFixedUnit.LineHeight:
                    return element.style.LineHeightSize * width.value;

                default:
                    return 0;
            }
        }

        [DebuggerStepThrough]
        public float ResolveFixedHeight(UIFixedLength height) {
            
            switch (height.unit) {
                
                case UIFixedUnit.Pixel:
                    return height.value;

                case UIFixedUnit.Percent:
                    return size.height * height.value;

                case UIFixedUnit.ViewportHeight:
                    return element.View.Viewport.height * height.value;

                case UIFixedUnit.ViewportWidth:
                    return element.View.Viewport.width * height.value;

                case UIFixedUnit.Em:
                    return element.style.GetResolvedFontSize() * height.value;

                case UIFixedUnit.LineHeight:
                    return element.style.LineHeightSize * height.value;

                default:
                    return 0;
            }
            
        }

        public void ApplyHorizontalLayout(float localX, float containingWidth, float allocatedWidth, float preferredWidth, in Alignment alignment, Fit fit) {
            // need to know the actual size of what im laying out in order to grow...oder?

            allocatedPosition.x = localX;

            if (selfFitHorizontal != Fit.Unset) {
                fit = selfFitHorizontal;
            }
            
            Size oldSize = size;

            paddingBox.left = ResolveFixedWidth(element.style.PaddingLeft);
            paddingBox.right = ResolveFixedWidth(element.style.PaddingRight);
            borderBox.left = ResolveFixedWidth(element.style.BorderLeft);
            borderBox.right = ResolveFixedWidth(element.style.BorderRight);

            switch (fit) {
                case Fit.Unset:
                case Fit.None:
                    size.width = preferredWidth;
                    break;

                case Fit.Grow:
                    if (allocatedWidth > preferredWidth) {
                        size.width = allocatedWidth;
                    }

                    break;

                case Fit.Shrink:
                    if (allocatedWidth < preferredWidth) {
                        size.width = allocatedWidth;
                    }

                    break;

                case Fit.Fit:
                    size.width = allocatedWidth;
                    break;
            }

            allocatedSize.width = allocatedWidth;
            containingBox.width = containingWidth;
            
            // if content size changed we need to layout todo account for padding
            if ((int)oldSize.width != (int)size.width) {
                flags |= LayoutRenderFlag.NeedsLayout;
            }

            // size = how big am I actually
            // allocated size = size my parent told me to be
            // content size = extents of my content
        }

        public void ApplyLayoutVertical(float localY, float containingHeight, float allocatedHeight, float preferredHeight, in Alignment alignment, Fit fit) {
            allocatedPosition.y = localY;

            if (selfFitHorizontal != Fit.Unset) {
                fit = selfFitHorizontal;
            }

            Size oldSize = size;
            
            switch (fit) {
                case Fit.Unset:
                case Fit.None:
                    size.height = preferredHeight;
                    break;

                case Fit.Grow:
                    if (allocatedHeight > preferredHeight) {
                        size.height = allocatedHeight;
                    }

                    break;

                case Fit.Shrink:
                    if (allocatedHeight < preferredHeight) {
                        size.height = allocatedHeight;
                    }

                    break;

                case Fit.Fit:
                    size.height = allocatedHeight;
                    break;
            }

            allocatedSize.height = allocatedHeight;
            containingBox.height = containingHeight;

            if ((int)oldSize.height != (int)size.height) {
                flags |= LayoutRenderFlag.NeedsLayout;
            }

        }

        public void GetWidth(float lastResolvedWidth, ref SizeConstraints output) {
            output.minWidth = ResolveWidth(lastResolvedWidth, widthMeasurement.minValue, widthMeasurement.minUnit);

            output.maxWidth = ResolveWidth(lastResolvedWidth, widthMeasurement.maxValue, widthMeasurement.maxUnit);

            output.prefWidth = ResolveWidth(lastResolvedWidth, widthMeasurement.prefValue, widthMeasurement.prefUnit);

            if (output.prefWidth < output.minWidth) output.prefWidth = output.minWidth;
            if (output.prefWidth > output.maxWidth) output.prefWidth = output.maxWidth;
        }

        public void GetHeight(float width, float lastResolvedHeight, ref SizeConstraints output) {
            output.minHeight = ResolveHeight(width, lastResolvedHeight, heightMeasurement.minValue, heightMeasurement.minUnit);

            output.maxHeight = ResolveHeight(width, lastResolvedHeight, heightMeasurement.maxValue, heightMeasurement.maxUnit);

            output.prefHeight = ResolveHeight(width, lastResolvedHeight, heightMeasurement.prefValue, heightMeasurement.prefUnit);

            if (output.prefHeight < output.minHeight) output.prefHeight = output.minHeight;
            if (output.prefHeight > output.maxHeight) output.prefHeight = output.maxHeight;
        }

        public bool IsLayoutBoundary() {
            
            if ((flags & LayoutRenderFlag.Ignored) != 0) {
                return true;
            }

            if (parent == null) {
                return true;
            }
            
            
            // if parent doesn't use my size
            
            return false;
        }

        public void Layout() {
            
            if ((flags & LayoutRenderFlag.NeedsLayout) == 0) {
                return;
            }
            
            PerformLayout();
            
        }
        
//        public void Layout(ContainingBlock containingBlock, bool parentUsesSize = false) {
//            FastLayoutBox relayoutBoundary;
//
//            // when would parent not use size?
//            // fr grid cell
//            // table cell?
//            // ignored
//
//            // also account for fixed size on both dimensions
//
//            bool tightWidth = (widthMeasurement.prefUnit == UIMeasurementUnit.Pixel ||
//                               widthMeasurement.prefUnit == UIMeasurementUnit.Em ||
//                               widthMeasurement.prefUnit == UIMeasurementUnit.ViewportWidth ||
//                               widthMeasurement.prefUnit == UIMeasurementUnit.ViewportHeight);
//
//            bool tight = tightWidth && (heightMeasurement.prefUnit == UIMeasurementUnit.Pixel ||
//                                        heightMeasurement.prefUnit == UIMeasurementUnit.Em ||
//                                        heightMeasurement.prefUnit == UIMeasurementUnit.ViewportWidth ||
//                                        heightMeasurement.prefUnit == UIMeasurementUnit.ViewportHeight);
//
//            if (element.style.LayoutBehavior == LayoutBehavior.Ignored || !parentUsesSize || sizedByParent || tight || parent == null) {
//                relayoutBoundary = this;
//            }
//            else {
//                relayoutBoundary = parent.relayoutBoundary;
//            }
//
//            // if sized based on containing block & containing block size didn't change, don't layout.
//            // can be per axis probably and mixed w/ tight width or height
//
//            if ((flags & LayoutRenderFlag.NeedsLayout) != 0 && this.blockSize.Equals(containingBlock) && this.relayoutBoundary == relayoutBoundary) {
//                return;
//            }
//
//            this.blockSize = containingBlock;
//            this.relayoutBoundary = relayoutBoundary;
//
//            if (sizedByParent) {
//                PerformResize();
//            }
//
//            PerformLayout();
//
//            flags &= ~(LayoutRenderFlag.NeedsLayout);
//
//            MarkNeedsPaint();
//        }

        public void PerformResize() { }

        public abstract void PerformLayout();

        public void MarkNeedsPaint() { }

        public void MarkNeedsLayout() {
            if ((flags & LayoutRenderFlag.NeedsLayout) != 0) {
                return;
            }

            // go upwards until we find a relayout boundary
            if (relayoutBoundary != this) {
                parent.MarkNeedsLayout();
            }
            else {
                flags |= LayoutRenderFlag.NeedsLayout;
                layoutSystem.nodesNeedingLayout.Add(this); // add to root list of nodes needing layout
            }
        }

        public virtual void OnStyleChanged(StructList<StyleProperty> changeList) {
            bool marked = false;

            int count = changeList.size;
            StyleProperty[] properties = changeList.array;

            for (int i = 0; i < count; i++) {
                ref StyleProperty property = ref properties[i];

                switch (property.propertyId) {
                    case StylePropertyId.PaddingLeft:
                        paddingBox.left = ResolveFixedWidth(property.AsUIFixedLength);
                        marked = true;
                        break;

                    case StylePropertyId.PaddingRight:
                        paddingBox.right = ResolveFixedWidth(property.AsUIFixedLength);
                        marked = true;
                        break;

                    case StylePropertyId.PaddingTop:
                        paddingBox.top = ResolveFixedHeight(property.AsUIFixedLength);
                        marked = true;
                        break;

                    case StylePropertyId.PaddingBottom:
                        paddingBox.bottom = ResolveFixedHeight(property.AsUIFixedLength);
                        marked = true;
                        break;

                    case StylePropertyId.BorderLeft:
                        borderBox.left = ResolveFixedWidth(property.AsUIFixedLength);
                        marked = true;
                        break;

                    case StylePropertyId.BorderRight:
                        borderBox.right = ResolveFixedWidth(property.AsUIFixedLength);
                        marked = true;
                        break;

                    case StylePropertyId.BorderTop:
                        borderBox.top = ResolveFixedHeight(property.AsUIFixedLength);
                        marked = true;
                        break;

                    case StylePropertyId.BorderBottom:
                        borderBox.bottom = ResolveFixedHeight(property.AsUIFixedLength);
                        marked = true;
                        break;

                    case StylePropertyId.TextFontSize:
                        
                        paddingBox.left = ResolveFixedWidth(element.style.PaddingLeft);
                        paddingBox.right = ResolveFixedWidth(element.style.PaddingRight);
                        paddingBox.top = ResolveFixedHeight(element.style.PaddingTop);
                        paddingBox.bottom = ResolveFixedHeight(element.style.PaddingBottom);
                        
                        borderBox.left = ResolveFixedWidth(element.style.BorderLeft);
                        borderBox.right = ResolveFixedWidth(element.style.BorderRight);
                        borderBox.top = ResolveFixedHeight(element.style.BorderTop);
                        borderBox.bottom = ResolveFixedHeight(element.style.BorderBottom);
                        
                        // anything else em sized should be updated here too
                        
                        break;

                    // todo -- margin should be a fixed measurement probably
                    case StylePropertyId.MarginLeft:
                        marginLeft = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MarginRight:
                        marginRight = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MarginTop:
                        marginTop = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MarginBottom:
                        marginBottom = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.PreferredWidth:
                        UIMeasurement prefWidth = property.AsUIMeasurement;
                        widthMeasurement.prefValue = prefWidth.value;
                        widthMeasurement.prefUnit = prefWidth.unit;
                        marked = true;
                        break;

                    case StylePropertyId.PreferredHeight:
                        UIMeasurement prefHeight = property.AsUIMeasurement;
                        heightMeasurement.prefValue = prefHeight.value;
                        heightMeasurement.prefUnit = prefHeight.unit;
                        marked = true;
                        break;

                    case StylePropertyId.MinWidth:
                        UIMeasurement minWidth = property.AsUIMeasurement;
                        widthMeasurement.minValue = minWidth.value;
                        widthMeasurement.minUnit = minWidth.unit;
                        marked = true;
                        break;

                    case StylePropertyId.MinHeight:
                        UIMeasurement minHeight = property.AsUIMeasurement;
                        heightMeasurement.minValue = minHeight.value;
                        heightMeasurement.minUnit = minHeight.unit;
                        marked = true;
                        break;

                    case StylePropertyId.MaxWidth:
                        UIMeasurement maxWidth = property.AsUIMeasurement;
                        widthMeasurement.maxValue = maxWidth.value;
                        widthMeasurement.maxUnit = maxWidth.unit;
                        marked = true;
                        break;

                    case StylePropertyId.MaxHeight:
                        UIMeasurement maxHeight = property.AsUIMeasurement;
                        heightMeasurement.maxValue = maxHeight.value;
                        heightMeasurement.maxUnit = maxHeight.unit;
                        marked = true;
                        break;

                    case StylePropertyId.ZIndex:
                        // zIndex = property.AsInt;
                        break;

                    case StylePropertyId.Layer:
                        // layer = property.AsInt;
                        break;

                    case StylePropertyId.LayoutBehavior:
                        // layoutBehavior = property.AsLayoutBehavior;
                        // UpdateChildren();
                        break;

                    case StylePropertyId.LayoutType:
                        //layoutTypeChanged = true;
                        break;

                    case StylePropertyId.OverflowX:
                        // overflowX = property.AsOverflow;
                        break;

                    case StylePropertyId.OverflowY:
                        // overflowY = property.AsOverflow;
                        break;
                }
            }

            if (marked) {
                MarkNeedsLayout();
            }
            
        }

        public virtual void SetChildren(LightList<FastLayoutBox> container) {
            if (container.size == 0) {
                firstChild = null;
                return;
            }

            firstChild = container[0];
            for (int i = 0;
                i < container.size;
                i++) {
                FastLayoutBox ptr = container[i];
                ptr.parent = this;
                if (i != 0) {
                    ptr.prevSibling = container[i - 1];
                }

                if (i != container.size - 1) {
                    ptr.nextSibling = container[i + 1];
                }
            }
        }

        public FastLayoutBox[] __DebugChildren {
            get {
                LightList<FastLayoutBox> boxList = new LightList<FastLayoutBox>();

                FastLayoutBox ptr = firstChild;
                while (ptr != null) {
                    boxList.Add(ptr);
                    ptr = ptr.nextSibling;
                }

                return boxList.ToArray();
            }
        }

        public void GetMarginHorizontal(ref float containingBlockWidth) { }

    }

}