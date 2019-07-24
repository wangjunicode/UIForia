using System.Diagnostics;
using SVGX;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Layout {

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

        // optimized to use bits for units & holds resolved value 
        public OffsetRect paddingBox;
        public OffsetRect borderBox;

        public UIMeasurement marginTop;
        public UIMeasurement marginRight;
        public UIMeasurement marginBottom;
        public UIMeasurement marginLeft;

        public UIMeasurement minWidth;
        public UIMeasurement maxWidth;
        public UIMeasurement prefWidth;
        public UIMeasurement minHeight;
        public UIMeasurement maxHeight;
        public UIMeasurement prefHeight;

        public int traversalIndex;

        public Size allocatedSize;
        public Size contentSize;
        public Size size;
        public Vector2 allocatedPosition;
        public Vector2 alignedPosition;

        public Size containingBox;

        public Fit selfFitHorizontal;
        public Fit selfFitVertical;

        public Alignment targetAlignmentHorizontal;
        public Alignment targetAlignmentVertical;

        public UIElement element;
        public FastLayoutSystem.LayoutOwner owner;
        public SVGXMatrix localMatrix;

        public virtual void AddChild(FastLayoutBox child) {
            child.parent = this;
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
                firstChild = child;
                OnChildAdded(child, idx);
            }
            else {
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

        public virtual void UpdateStyleData() {
            // update style properties
            minWidth = element.style.MinWidth;
            maxWidth = element.style.MaxWidth;
            prefWidth = element.style.PreferredWidth;

            minHeight = element.style.MinHeight;
            maxHeight = element.style.MaxHeight;
            prefHeight = element.style.PreferredHeight;

            marginTop = element.style.MarginTop;
            marginRight = element.style.MarginRight;
            marginBottom = element.style.MarginBottom;
            marginLeft = element.style.MarginLeft;
//            var localPositionX = element.style.TransformPositionX;
//            var localPositionY = element.style.TransformPositionY;
//            var rotation = element.style.TransformRotation;
//            var scaleX = element.style.TransformScaleX;
//            var scaleY = element.style.TransformScaleY;

            MarkNeedsLayout();
        }

        public virtual void ComputeContentSize(Size blockSize) {
            // optimize for when we need to compute width & height together so we avoid a width pass and do both at once
        }

        protected virtual void OnChildAdded(FastLayoutBox child, int index) { }

        protected virtual void OnChildRemoved(FastLayoutBox child, int index) { }

        public virtual float GetIntrinsicMinWidth() {
            return 0;
        }

        protected virtual float GetIntrinsicMinHeight() {
            return 0;
        }

        protected virtual float GetIntrinsicMaxWidth() {
            return float.MaxValue;
        }

        protected virtual float GetIntrinsicMaxHeight() {
            return float.MaxValue;
        }

        public virtual float ResolveAutoWidth() {
            return 0;
        }

        public virtual float ResolveAutoHeight() {
            return 0;
        }

        public float ResolveWidth(float resolvedBlockSize, UIMeasurement measurement) {
            float value = measurement.value;

            switch (measurement.unit) {
                case UIMeasurementUnit.Content: {
                    float width = ComputeContentWidth(resolvedBlockSize);
                    float old = size.width;
                    size.width = width;
                    // todo -- try not to fuck with style here
                    width += ResolveFixedWidth(element.style.PaddingLeft);
                    width += ResolveFixedWidth(element.style.PaddingRight);
                    width += ResolveFixedWidth(element.style.BorderRight);
                    width += ResolveFixedWidth(element.style.BorderLeft);
                    size.width = old;
                    if (width < 0) width = 0;
                    return width;
                }

                case UIMeasurementUnit.FitContent:
                    float min = GetIntrinsicMinWidth();
                    float pref = GetIntrinsicMaxWidth();
                    return Mathf.Max(min, Mathf.Min(resolvedBlockSize, pref));

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
                
                case UIMeasurementUnit.ParentSize:
                case UIMeasurementUnit.ParentContentArea:
                    return resolvedBlockSize * measurement.value;
            }

            return 0;
        }

        public float ResolveHeight(float width, float blockWidth, float blockHeight, UIMeasurement measurement) {
            float value = measurement.value;

            switch (measurement.unit) {
                case UIMeasurementUnit.Content:
                    float height = ComputeContentHeight(width, blockWidth, blockHeight);
                    float old = size.height;
                    size.height = height;
                    height += ResolveFixedHeight(element.style.PaddingTop);
                    height += ResolveFixedHeight(element.style.PaddingBottom);
                    height += ResolveFixedHeight(element.style.BorderBottom);
                    height += ResolveFixedHeight(element.style.BorderTop);
                    size.height = old;
                    if (height < 0) height = 0;
                    return height;

                case UIMeasurementUnit.FitContent:
                    float min = GetIntrinsicMinHeight();
                    float pref = GetIntrinsicMaxHeight();
                    return Mathf.Max(min, Mathf.Min(blockHeight, pref));

                case UIMeasurementUnit.Pixel:
                    return value;

                case UIMeasurementUnit.Em:
                    return 0;

                case UIMeasurementUnit.Percentage:
                    // percentage of last resolved size (ie containing block)
                    return blockHeight * value;

                case UIMeasurementUnit.ViewportWidth:
                    return element.View.Viewport.width * value;

                case UIMeasurementUnit.ViewportHeight:
                    return element.View.Viewport.height * value;

                case UIMeasurementUnit.MinContent:
                    return GetIntrinsicMinHeight();

                case UIMeasurementUnit.MaxContent:
                    return GetIntrinsicMaxHeight();
                
                case UIMeasurementUnit.ParentSize:
                case UIMeasurementUnit.ParentContentArea:
                    return blockHeight * measurement.value;
            }

            return 0;
        }


        protected virtual float ComputeContentWidth(float blockWidth) {
            return 0;
        }

        protected virtual float ComputeContentHeight(float width, float blockWidth, float blockHeight) {
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

            ref SizeSet sizeSet = ref owner.sizeSetList.array[traversalIndex];
            sizeSet.size = size;
            sizeSet.allocatedSize = allocatedSize;

            AlignmentBehavior alignmentBehavior = element.style.AlignmentBehaviorX;
            if (alignmentBehavior == AlignmentBehavior.Self) {
                targetAlignmentHorizontal.value = element.style.AlignmentOffsetX;
                targetAlignmentHorizontal.pivot = element.style.AlignmentPivotX;
                targetAlignmentHorizontal.target = element.style.AlignmentTargetX;
            }
            else {
                targetAlignmentHorizontal = alignment;
            }

            switch (targetAlignmentHorizontal.target) {
                case AlignmentTarget.AllocatedBox:
                    alignedPosition.x = (allocatedWidth * targetAlignmentHorizontal.value) - (size.width * targetAlignmentHorizontal.pivot);
                    break;

                case AlignmentTarget.Parent:
                    alignedPosition.x = (parent.size.width * targetAlignmentHorizontal.value) - (size.width * targetAlignmentHorizontal.pivot);
                    break;

                case AlignmentTarget.ParentContentArea:
                    alignedPosition.x = ((parent.size.width - parent.paddingBox.top - parent.paddingBox.bottom) * targetAlignmentHorizontal.value) - (size.width * targetAlignmentHorizontal.pivot);
                    break;

                case AlignmentTarget.View:
                    alignedPosition.x = ((element.View.Viewport.width - parent.paddingBox.top - parent.paddingBox.bottom) * targetAlignmentHorizontal.value) - (size.width * targetAlignmentHorizontal.pivot);
                    break;

                case AlignmentTarget.Screen:
                    alignedPosition.x = ((Screen.width - parent.paddingBox.top - parent.paddingBox.bottom) * targetAlignmentHorizontal.value) - (size.width * targetAlignmentHorizontal.pivot);
                    break;
            }

            alignedPosition.x += localX;

            ref PositionSet positionSet = ref owner.positionSetList.array[traversalIndex];
            positionSet.allocatedPosition = allocatedPosition;
            positionSet.alignedPosition = alignedPosition;

            // if content size changed we need to layout todo account for padding
            if ((int) oldSize.width != (int) size.width) {
                flags |= LayoutRenderFlag.NeedsLayout;
            }

            // size = how big am I actually
            // allocated size = size my parent told me to be
            // content size = extents of my content
        }

        public void ApplyVerticalLayout(float localY, float containingHeight, float allocatedHeight, float preferredHeight, in Alignment alignment, Fit fit) {
            allocatedPosition.y = localY;

            if (selfFitVertical != Fit.Unset) {
                fit = selfFitVertical;
            }

            Size oldSize = size;

            paddingBox.top = ResolveFixedHeight(element.style.PaddingTop);
            paddingBox.bottom = ResolveFixedHeight(element.style.PaddingBottom);
            borderBox.top = ResolveFixedHeight(element.style.BorderTop);
            borderBox.bottom = ResolveFixedHeight(element.style.BorderBottom);

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

            AlignmentBehavior alignmentBehavior = element.style.AlignmentBehaviorY;
            if (alignmentBehavior == AlignmentBehavior.Self) {
                targetAlignmentVertical.value = element.style.AlignmentOffsetY;
                targetAlignmentVertical.pivot = element.style.AlignmentPivotY;
                targetAlignmentVertical.target = element.style.AlignmentTargetY;
            }
            else {
                targetAlignmentVertical = alignment;
            }
            
            switch (targetAlignmentVertical.target) {
                case AlignmentTarget.AllocatedBox:
                    alignedPosition.y = (allocatedHeight * targetAlignmentVertical.value) - (size.height * targetAlignmentVertical.pivot);
                    break;

                // todo -- do the following cases need to be offset again by margin size?
                case AlignmentTarget.Parent:
                    alignedPosition.y = (parent.size.height * targetAlignmentVertical.value) - (size.height * targetAlignmentVertical.pivot);
                    break;

                case AlignmentTarget.ParentContentArea:
                    alignedPosition.y = ((parent.size.height - parent.paddingBox.top - parent.paddingBox.bottom) * targetAlignmentVertical.value) - (size.height * targetAlignmentVertical.pivot);
                    break;

                case AlignmentTarget.View:
                    alignedPosition.y = ((element.View.Viewport.height - parent.paddingBox.top - parent.paddingBox.bottom) * targetAlignmentVertical.value) - (size.height * targetAlignmentVertical.pivot);
                    break;

                case AlignmentTarget.Screen:
                    alignedPosition.y = ((Screen.height - parent.paddingBox.top - parent.paddingBox.bottom) * targetAlignmentVertical.value) - (size.height * targetAlignmentVertical.pivot);
                    break;
            }

            alignedPosition.y += localY;

            ref SizeSet sizeSet = ref owner.sizeSetList.array[traversalIndex];
            sizeSet.size = size;
            sizeSet.allocatedSize = allocatedSize;

            ref PositionSet positionSet = ref owner.positionSetList.array[traversalIndex];
            positionSet.allocatedPosition = allocatedPosition;
            positionSet.alignedPosition = alignedPosition;

            if ((int) oldSize.height != (int) size.height) {
                flags |= LayoutRenderFlag.NeedsLayout;
            }
        }

        public void GetWidth(float lastResolvedWidth, ref SizeConstraints output) {
            output.minWidth = ResolveWidth(lastResolvedWidth, minWidth);
            output.maxWidth = ResolveWidth(lastResolvedWidth, maxWidth);
            output.prefWidth = ResolveWidth(lastResolvedWidth, prefWidth);

            if (output.prefWidth < output.minWidth) output.prefWidth = output.minWidth;
            if (output.prefWidth > output.maxWidth) output.prefWidth = output.maxWidth;
        }

        public void GetHeight(float width, float blockWidth, float blockHeight, ref SizeConstraints output) {
            output.minHeight = ResolveHeight(width, blockWidth, blockHeight, minHeight);
            output.maxHeight = ResolveHeight(width, blockWidth, blockHeight, maxHeight);
            output.prefHeight = ResolveHeight(width, blockWidth, blockHeight, prefHeight);

            if (output.prefHeight < output.minHeight) output.prefHeight = output.minHeight;
            if (output.prefHeight > output.maxHeight) output.prefHeight = output.maxHeight;
        }

        public void Layout() {
            // todo -- size check

            SVGXMatrix m = localMatrix;

            float pivotX = ResolveFixedWidth(element.style.TransformPivotX);
            float pivotY = ResolveFixedWidth(element.style.TransformPivotY);
            float scaleX = element.style.TransformScaleX;
            float scaleY = element.style.TransformScaleY;
            float rotation = element.style.TransformRotation;

            // todo -- need to apply alignment here
            // todo -- ResolveTransformOffset();
            Vector2 localPosition = alignedPosition;

            m = SVGXMatrix.TRS(localPosition, rotation, new Vector2(scaleX, scaleY));
            SVGXMatrix pivotMat = new SVGXMatrix(1, 0, 0, 1, size.width * pivotX, size.height * pivotY);
            localMatrix = pivotMat * m * new SVGXMatrix(1, 0, 0, 1, -size.width * pivotX, -size.height * pivotY);

            owner.localMatrixList.array[traversalIndex] = localMatrix;

            if ((flags & LayoutRenderFlag.NeedsLayout) == 0) {
                return;
            }

            PerformLayout();

            flags &= ~LayoutRenderFlag.NeedsLayout;

            FastLayoutBox child = firstChild;
            while (child != null) {
                child.Layout();
                child = child.nextSibling;
            }

            // todo -- compute content size & local overflow? might need to happen elsewhere
        }

        protected abstract void PerformLayout();

        protected virtual void OnChildSizeChanged(FastLayoutBox child) {
            if ((flags & LayoutRenderFlag.NeedsLayout) != 0) {
                return;
            }

            if ((child.flags & LayoutRenderFlag.Ignored) != 0) {
                return;
            }

            if (prefWidth.unit == UIMeasurementUnit.Content || prefHeight.unit == UIMeasurementUnit.Content) {
                MarkNeedsLayout();
            }
        }

        public void MarkNeedsContentSizeLayout(FastLayoutBox child) {
            if ((flags & LayoutRenderFlag.NeedsLayout) != 0) {
                return;
            }
        }

        public void MarkNeedsLayout() {
            if ((flags & LayoutRenderFlag.NeedsLayout) != 0) {
                return;
            }

            // go upwards until we find a relayout boundary
            if (relayoutBoundary != this && parent != null) {
                parent.MarkNeedsLayout();
            }
            else {
                // add to root list of nodes needing layout
                flags |= LayoutRenderFlag.NeedsLayout;
                owner.toLayout.Add(this);
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
                        prefWidth = property.AsUIMeasurement;
                        marked = true;
                        break;

                    case StylePropertyId.PreferredHeight:
                        prefHeight = property.AsUIMeasurement;
                        marked = true;
                        break;

                    case StylePropertyId.MinWidth:
                        minWidth = property.AsUIMeasurement;
                        marked = true;
                        break;

                    case StylePropertyId.MinHeight:
                        minHeight = property.AsUIMeasurement;
                        marked = true;
                        break;

                    case StylePropertyId.MaxWidth:
                        maxWidth = property.AsUIMeasurement;
                        marked = true;
                        break;

                    case StylePropertyId.MaxHeight:
                        maxHeight = property.AsUIMeasurement;
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
                parent?.OnChildSizeChanged(this);
            }
        }

        public virtual void SetChildren(LightList<FastLayoutBox> container) {
            if (container.size == 0) {
                firstChild = null;
                return;
            }

            firstChild = container[0];
            for (int i = 0;i < container.size;i++) {
                FastLayoutBox ptr = container[i];
                ptr.parent = this;
                
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

        public void GetMarginHorizontal(float blockWidth, ref OffsetRect margin) {
            switch (marginLeft.unit) {
                case UIMeasurementUnit.Pixel:
                    margin.left = marginLeft.value;
                    break;

                case UIMeasurementUnit.Em:
                    margin.left = element.style.GetResolvedFontSize() * marginLeft.value;
                    break;

                case UIMeasurementUnit.Percentage:
                    margin.left = blockWidth * marginLeft.value;
                    break;
            }

            switch (marginRight.unit) {
                case UIMeasurementUnit.Pixel:
                    margin.right = marginRight.value;
                    break;

                case UIMeasurementUnit.Em:
                    margin.right = element.style.GetResolvedFontSize() * marginRight.value;
                    break;

                case UIMeasurementUnit.Percentage:
                    margin.right = blockWidth * marginRight.value;
                    break;
            }
        }

        public void GetMarginVertical(float blockHeight, ref OffsetRect margin) {
            switch (marginTop.unit) {
                case UIMeasurementUnit.Pixel:
                    margin.top = marginTop.value;
                    break;

                case UIMeasurementUnit.Em:
                    margin.top = element.style.GetResolvedFontSize() * marginTop.value;
                    break;

                case UIMeasurementUnit.Percentage:
                    margin.top = blockHeight * marginTop.value;
                    break;
            }

            switch (marginBottom.unit) {
                case UIMeasurementUnit.Pixel:
                    margin.bottom = marginBottom.value;
                    break;

                case UIMeasurementUnit.Em:
                    margin.bottom = element.style.GetResolvedFontSize() * marginBottom.value;
                    break;

                case UIMeasurementUnit.Percentage:
                    margin.bottom = blockHeight * marginBottom.value;
                    break;
            }
        }

        public void Replace(FastLayoutBox box) {
            firstChild = box.firstChild;
            parent = box.parent;
            FastLayoutBox ptr = firstChild;
            while (ptr != null) {
                ptr.parent = this;
                ptr = ptr.nextSibling;
            }
        }

        protected virtual void OnChildStyleChanged(FastLayoutBox child, StructList<StyleProperty> changeList) { }

    }

}