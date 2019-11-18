using System;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIForia.Systems {

    public abstract class AwesomeLayoutBox {

        public float finalWidth;
        public float finalHeight;

        public float paddingBorderHorizontalStart;
        public float paddingBorderHorizontalEnd;
        public float paddingBorderVerticalStart;
        public float paddingBorderVerticalEnd;

        public AwesomeLayoutBoxFlags flags;
        public float cachedContentWidth;
        public float cachedContentHeight;
        public LayoutType layoutBoxType;

        public int childCount;
        public UIElement element;
        public AwesomeLayoutBox firstChild;
        public AwesomeLayoutBox nextSibling;
        public AwesomeLayoutBox parent;
        public float transformX;
        public float transformY;
        public float transformRotation;

        public void Initialize(UIElement element, int frameId) {
            this.element = element;
            cachedContentWidth = -1;
            cachedContentHeight = -1;
            finalWidth = -1;
            finalHeight = -1;
            string boxName = GetType().ToString();
            element.layoutHistory = element.layoutHistory ?? new LayoutHistory(element);
            element.layoutHistory.AddLogEntry(LayoutDirection.Horizontal, frameId, LayoutReason.Initialized, boxName);
            element.layoutHistory.AddLogEntry(LayoutDirection.Vertical, frameId, LayoutReason.Initialized, boxName);
            flags |= AwesomeLayoutBoxFlags.RequireLayoutHorizontal | AwesomeLayoutBoxFlags.RequireLayoutVertical | AwesomeLayoutBoxFlags.RequiresMatrixUpdate;
            UpdateBlockProviderWidth();
            UpdateBlockProviderHeight();

            UpdateRequiresHorizontalAlignment();
            UpdateRequiresVerticalAlignment();

            OnInitialize();
        }

        protected virtual void OnInitialize() { }

        public void Destroy() {
            OnDestroy();
            flags = 0;
            element = null;
            parent = null;
            nextSibling = null;
            firstChild = null;
        }

        protected virtual void OnDestroy() { }

        protected abstract float ComputeContentWidth();

        protected abstract float ComputeContentHeight();

        // update list of children and set their layout parent accordingly
        public void SetChildren(LightList<AwesomeLayoutBox> layoutBoxes) {
            firstChild = null;
            childCount = layoutBoxes.size;
            if (childCount > 0) {
                LayoutResult result = element.layoutResult;
                firstChild = layoutBoxes[0];
                firstChild.parent = this;
                firstChild.element.layoutResult.layoutParent = result;
                AwesomeLayoutBox ptr = firstChild;
                for (int i = 1; i < layoutBoxes.size; i++) {
                    layoutBoxes.array[i].parent = this;
                    layoutBoxes.array[i].element.layoutResult.layoutParent = result;
                    ptr.nextSibling = layoutBoxes.array[i];
                    ptr = ptr.nextSibling;
                }
            }

            OnChildrenChanged(layoutBoxes);
        }

        public abstract void OnChildrenChanged(LightList<AwesomeLayoutBox> childList);

        public void ApplyLayoutHorizontal(float localX, float alignedPosition, float size, float availableSize, LayoutFit defaultFit, int frameId) {
            
            LayoutFit fit = element.style.LayoutFitHorizontal;
            if (fit == LayoutFit.Default || fit == LayoutFit.Unset) {
                fit = defaultFit;
            }

            float newWidth = size;

            switch (fit) {
                case LayoutFit.Unset:
                case LayoutFit.None:
                case LayoutFit.Default:
                    newWidth = size;
                    break;

                case LayoutFit.Grow:
                    if (availableSize > size) {
                        newWidth = availableSize;
                        alignedPosition = localX;
                    }

                    break;

                case LayoutFit.Shrink:
                    if (availableSize < size) {
                        newWidth = availableSize;
                        alignedPosition = localX;
                    }

                    break;

                case LayoutFit.Fill:
                    newWidth = availableSize;
                    alignedPosition = localX;
                    break;
            }

            Vector2 viewSize = element.View.Viewport.size;
            float emSize = 0; // todo -- read off of style (cached)
            float paddingLeft = MeasurementUtil.ResolveFixedSize(newWidth, viewSize.x, viewSize.y, emSize, element.style.PaddingLeft);
            float paddingRight = MeasurementUtil.ResolveFixedSize(newWidth, viewSize.x, viewSize.y, emSize, element.style.PaddingRight);
            float borderLeft = MeasurementUtil.ResolveFixedSize(newWidth, viewSize.x, viewSize.y, emSize, element.style.BorderLeft);
            float borderRight = MeasurementUtil.ResolveFixedSize(newWidth, viewSize.x, viewSize.y, emSize, element.style.BorderRight);

            // write to layout result here? would need to flag layout result for changes anyway
            LayoutResult layoutResult = element.layoutResult;
            
            float previousPosition = layoutResult.alignedPosition.x; 
            
            // todo -- layout result change flags (and maybe history entry if enabled)
            layoutResult.alignedPosition.x = alignedPosition;
            layoutResult.allocatedPosition.x = localX;
            layoutResult.padding.left = paddingLeft;
            layoutResult.padding.right = paddingRight;
            layoutResult.border.left = borderLeft;
            layoutResult.border.right = borderRight;
            layoutResult.actualSize.width = newWidth;
            layoutResult.allocatedSize.width = availableSize;
            layoutResult.pivot.x = newWidth * 0.5f; // todo -- resolve pivot

            // todo -- margin

            paddingBorderHorizontalStart = paddingLeft + borderLeft;
            paddingBorderHorizontalEnd = paddingRight + borderRight;

            if ((flags & AwesomeLayoutBoxFlags.RequireAlignmentHorizontal) != 0 && !Mathf.Approximately(previousPosition, alignedPosition)) {
                flags |= AwesomeLayoutBoxFlags.RequiresMatrixUpdate;
            }
            
            // todo -- should probably be when content area size changes, not just overall size
            if (newWidth != finalWidth) {
                flags |= AwesomeLayoutBoxFlags.RequireLayoutHorizontal;
                element.layoutHistory.AddLogEntry(LayoutDirection.Horizontal, frameId, LayoutReason.FinalSizeChanged, string.Empty);
                finalWidth = newWidth;
            }
        }

        public void ApplyLayoutVertical(float localY, float alignedPosition, float size, float availableSize, LayoutFit defaultFit, int frameId) {
            
            LayoutFit fit = element.style.LayoutFitVertical;
            if (fit == LayoutFit.Default || fit == LayoutFit.Unset) {
                fit = defaultFit;
            }

            float newHeight = size;

            switch (fit) {
                case LayoutFit.Unset:
                case LayoutFit.None:
                case LayoutFit.Default:
                    newHeight = size;
                    break;

                case LayoutFit.Grow:
                    if (availableSize > size) {
                        newHeight = availableSize;
                        alignedPosition = localY;
                    }

                    break;

                case LayoutFit.Shrink:
                    if (availableSize < size) {
                        newHeight = availableSize;
                        alignedPosition = localY;
                    }

                    break;

                case LayoutFit.Fill:
                    newHeight = availableSize;
                    alignedPosition = localY;
                    break;
            }
            
            // if aligned position changed -> flag for matrix recalc 

            Vector2 viewSize = element.View.Viewport.size;
            float emSize = 0; // todo -- read off of style (cached)
            float paddingTop = MeasurementUtil.ResolveFixedSize(newHeight, viewSize.x, viewSize.y, emSize, element.style.PaddingTop);
            float paddingBottom = MeasurementUtil.ResolveFixedSize(newHeight, viewSize.x, viewSize.y, emSize, element.style.PaddingBottom);
            float borderTop = MeasurementUtil.ResolveFixedSize(newHeight, viewSize.x, viewSize.y, emSize, element.style.BorderTop);
            float borderBottom = MeasurementUtil.ResolveFixedSize(newHeight, viewSize.x, viewSize.y, emSize, element.style.BorderBottom);

            // write to layout result here? would need to flag layout result for changes anyway
            LayoutResult layoutResult = element.layoutResult;

            // todo -- layout result change flags (and maybe history entry if enabled)
            
            float previousPosition = layoutResult.alignedPosition.y;
            
            layoutResult.alignedPosition.y = alignedPosition;
            layoutResult.allocatedPosition.y = localY;
            layoutResult.padding.top = paddingTop;
            layoutResult.padding.bottom = paddingBottom;
            layoutResult.border.top = borderTop;
            layoutResult.border.bottom = borderBottom;
            layoutResult.actualSize.height = newHeight;
            layoutResult.allocatedSize.height = availableSize;
            layoutResult.pivot.y = newHeight * 0.5f; // todo -- resolve pivot

            // todo -- margin

            paddingBorderVerticalStart = paddingTop + borderTop;
            paddingBorderVerticalEnd = paddingBottom + borderBottom;
            
            if ((flags & AwesomeLayoutBoxFlags.RequireAlignmentVertical) != 0 && !Mathf.Approximately(previousPosition, alignedPosition)) { 
                flags |= AwesomeLayoutBoxFlags.RequiresMatrixUpdate;
            }

            if (newHeight != finalHeight) {
                flags |= AwesomeLayoutBoxFlags.RequireLayoutVertical;
                element.layoutHistory.AddLogEntry(LayoutDirection.Vertical, frameId, LayoutReason.FinalSizeChanged, string.Empty);
                finalHeight = newHeight;
            }
        }

        public float ResolveWidth(in UIMeasurement measurement) {
            float value = measurement.value;

            switch (measurement.unit) {
                case UIMeasurementUnit.Content: {
                    float width = 0;

                    // todo -- this cached value is only valid if the current block size is the same as when the size was computed
                    // probably makes sense to hold at least 2 versions of content cache, 1 for baseline one for 2nd pass (ie fit)
                    if (cachedContentWidth >= 0) {
                        width = cachedContentWidth; // todo -- might not need to resolve size for padding / border in this case
                    }
                    else {
                        width = ComputeContentWidth();
                    }

                    float baseVal = width;
                    // todo -- try not to fuck with style here
                    // todo -- view and em size
                    Vector2 viewSize = element.View.Viewport.size;
                    baseVal += MeasurementUtil.ResolveFixedSize(width, viewSize.x, viewSize.y, 0, element.style.PaddingLeft);
                    baseVal += MeasurementUtil.ResolveFixedSize(width, viewSize.x, viewSize.y, 0, element.style.PaddingRight);
                    baseVal += MeasurementUtil.ResolveFixedSize(width, viewSize.x, viewSize.y, 0, element.style.BorderRight);
                    baseVal += MeasurementUtil.ResolveFixedSize(width, viewSize.x, viewSize.y, 0, element.style.BorderLeft);

                    if (baseVal < 0) baseVal = 0;

                    float retn = measurement.value * baseVal;

                    return retn > 0 ? retn : 0;
                }

                case UIMeasurementUnit.FitContent:
                    throw new NotImplementedException();

                case UIMeasurementUnit.Pixel:
                    return value;

                case UIMeasurementUnit.Em:
                    return element.style.GetResolvedFontSize() * value;

                case UIMeasurementUnit.ViewportWidth:
                    return element.View.Viewport.width * value;

                case UIMeasurementUnit.ViewportHeight:
                    return element.View.Viewport.height * value;

                case UIMeasurementUnit.IntrinsicMinimum:
                    return 0; //GetIntrinsicMinWidth();

                case UIMeasurementUnit.IntrinsicPreferred:
                    return 0; //GetIntrinsicPreferredWidth();

                case UIMeasurementUnit.BlockSize: {
                    // ignored elements can use the output size of their parent since it has been resolved already
                    return ComputeBlockWidth(measurement.value);
                }
                case UIMeasurementUnit.Percentage:
                case UIMeasurementUnit.ParentContentArea: {
                    return ComputeBlockContentWidth(measurement.value);
                }
            }

            return 0;
        }

        protected float ComputeBlockContentWidth(float value) {
            AwesomeLayoutBox ptr = parent;
            float paddingBorder = 0;

            // ignored elements can use the output size of their parent since it has been resolved already
            if ((flags & AwesomeLayoutBoxFlags.Ignored) != 0) {
                LayoutResult parentResult = element.layoutResult.layoutParent;
                paddingBorder = parentResult.padding.left + parentResult.padding.right + parentResult.border.left + parentResult.padding.right;
                return Math.Max(0, (parentResult.actualSize.width - paddingBorder) * value);
            }

            while (ptr != null) {
                paddingBorder += ptr.paddingBorderHorizontalStart + ptr.paddingBorderHorizontalEnd;
                if ((ptr.flags & AwesomeLayoutBoxFlags.WidthBlockProvider) != 0) {
                    Assert.AreNotEqual(-1, ptr.finalWidth);
                    return Math.Max(0, (ptr.finalWidth - paddingBorder) * value);
                }

                ptr = ptr.parent;
            }

            return Math.Max(0, (element.View.Viewport.width - paddingBorder) * value);
        }

        protected float ComputeBlockWidth(float value) {
            if ((flags & AwesomeLayoutBoxFlags.Ignored) != 0) {
                LayoutResult parentResult = element.layoutResult.layoutParent;
                return Math.Max(0, parentResult.actualSize.width * value);
            }

            AwesomeLayoutBox ptr = parent;
            while (ptr != null) {
                if ((ptr.flags & AwesomeLayoutBoxFlags.WidthBlockProvider) != 0) {
                    Assert.AreNotEqual(-1, ptr.finalWidth);
                    return Math.Max(0, ptr.finalWidth * value);
                }

                ptr = ptr.parent;
            }

            return Math.Max(0, element.View.Viewport.width * value);
        }

        protected float ComputeBlockContentHeight(float value) {
            AwesomeLayoutBox ptr = parent;
            float paddingBorder = 0;

            // ignored elements can use the output size of their parent since it has been resolved already
            if ((flags & AwesomeLayoutBoxFlags.Ignored) != 0) {
                LayoutResult parentResult = element.layoutResult.layoutParent;
                paddingBorder = parentResult.padding.top + parentResult.padding.bottom + parentResult.border.top + parentResult.padding.bottom;
                return Math.Max(0, (parentResult.actualSize.height - paddingBorder) * value);
            }

            while (ptr != null) {
                paddingBorder += ptr.paddingBorderVerticalStart + ptr.paddingBorderVerticalEnd;
                if ((ptr.flags & AwesomeLayoutBoxFlags.HeightBlockProvider) != 0) {
                    Assert.AreNotEqual(-1, ptr.finalHeight);
                    return Math.Max(0, (ptr.finalHeight - paddingBorder) * value);
                }

                ptr = ptr.parent;
            }

            return Math.Max(0, (element.View.Viewport.height - paddingBorder) * value);
        }

        protected float ComputeBlockHeight(float value) {
            AwesomeLayoutBox ptr = parent;

            // ignored elements can use the output size of their parent since it has been resolved already
            if ((flags & AwesomeLayoutBoxFlags.Ignored) != 0) {
                LayoutResult parentResult = element.layoutResult.layoutParent;
                return Math.Max(0, (parentResult.actualSize.height) * value);
            }

            while (ptr != null) {
                if ((ptr.flags & AwesomeLayoutBoxFlags.HeightBlockProvider) != 0) {
                    Assert.AreNotEqual(-1, ptr.finalHeight);
                    return Math.Max(0, ptr.finalHeight * value);
                }

                ptr = ptr.parent;
            }

            return Math.Max(0, element.View.Viewport.height * value);
        }

        public float ResolveHeight(in UIMeasurement measurement) {
            float value = measurement.value;

            switch (measurement.unit) {
                case UIMeasurementUnit.Content: {
                    float height = 0;

                    if (cachedContentHeight >= 0) {
                        height = cachedContentHeight;
                    }
                    else {
                        height = ComputeContentHeight();
                    }

                    float baseVal = height;
                    Vector2 viewSize = element.View.Viewport.size;
                    float emSize = element.style.GetResolvedFontSize(); // todo -- optimize this
                    baseVal += MeasurementUtil.ResolveFixedSize(height, viewSize.x, viewSize.y, emSize, element.style.PaddingTop);
                    baseVal += MeasurementUtil.ResolveFixedSize(height, viewSize.x, viewSize.y, emSize, element.style.PaddingBottom);
                    baseVal += MeasurementUtil.ResolveFixedSize(height, viewSize.x, viewSize.y, emSize, element.style.BorderTop);
                    baseVal += MeasurementUtil.ResolveFixedSize(height, viewSize.x, viewSize.y, emSize, element.style.BorderBottom);

                    if (baseVal < 0) baseVal = 0;
                    float retn = measurement.value * baseVal;
                    return retn > 0 ? retn : 0;
                }
                case UIMeasurementUnit.FitContent:
                    throw new NotImplementedException();

                case UIMeasurementUnit.Pixel:
                    return value;

                case UIMeasurementUnit.Em:
                    return element.style.GetResolvedFontSize() * value;

                case UIMeasurementUnit.ViewportWidth:
                    return element.View.Viewport.width * value;

                case UIMeasurementUnit.ViewportHeight:
                    return element.View.Viewport.height * value;

                case UIMeasurementUnit.IntrinsicMinimum: {
                    throw new NotImplementedException();
                }

                case UIMeasurementUnit.IntrinsicPreferred:
                    throw new NotImplementedException();

                case UIMeasurementUnit.BlockSize: {
                    return ComputeBlockHeight(value);
                }

                case UIMeasurementUnit.Percentage:
                case UIMeasurementUnit.ParentContentArea: {
                    return ComputeBlockContentHeight(value);
                }
            }

            return 0;
        }

        public abstract void RunLayoutHorizontal(int frameId);

        public abstract void RunLayoutVertical(int frameId);

        public void GetWidths(ref LayoutSize size) {
            size.preferred = ResolveWidth(element.style.PreferredWidth);
            size.minimum = ResolveWidth(element.style.MinWidth);
            size.maximum = ResolveWidth(element.style.MaxWidth);
            size.marginStart = MeasurementUtil.ResolveFixedSize(0, 0, 0, 0, element.style.MarginLeft);
            size.marginEnd = MeasurementUtil.ResolveFixedSize(0, 0, 0, 0, element.style.MarginRight);
            // todo -- not sure this is right or desired
            element.layoutResult.margin.left = size.marginStart;
            element.layoutResult.margin.right = size.marginEnd;
        }

        public void GetHeights(ref LayoutSize size) {
            size.preferred = ResolveHeight(element.style.PreferredHeight);
            size.minimum = ResolveHeight(element.style.MinHeight);
            size.maximum = ResolveHeight(element.style.MaxHeight);
            size.marginStart = MeasurementUtil.ResolveFixedSize(0, 0, 0, 0, element.style.MarginTop);
            size.marginEnd = MeasurementUtil.ResolveFixedSize(0, 0, 0, 0, element.style.MarginBottom);
            // todo -- not sure this is right or desired
            element.layoutResult.margin.top = size.marginStart;
            element.layoutResult.margin.bottom = size.marginEnd;
        }

        public virtual void OnStyleChanged(StructList<StyleProperty> propertyList) { }

        public virtual void OnChildStyleChanged(AwesomeLayoutBox child, StructList<StyleProperty> propertyList) { }

        protected virtual void OnChildWidthDirty(AwesomeLayoutBox child) {
            bool contentSizedWidth = (
                element.style.PreferredWidth.unit == UIMeasurementUnit.Content ||
                element.style.MinWidth.unit == UIMeasurementUnit.Content ||
                element.style.MaxWidth.unit == UIMeasurementUnit.Content
            );

            cachedContentWidth = -1;

            if (contentSizedWidth) {
                flags |= AwesomeLayoutBoxFlags.RequireLayoutHorizontal;
                parent?.OnChildWidthDirty(this);
            }
        }

        protected virtual void OnChildHeightDirty(AwesomeLayoutBox child) { }

        protected void MarkForLayout(LayoutDirtyFlag dirtyFlag) {
            // if parent depends on this child size
            // pref / min / max is content size -> also mark parent for layout

            // if the width was content sized we need to tell our parent that our size changed
            if ((dirtyFlag & LayoutDirtyFlag.InvalidateSizeHorizontal) != 0) {
                bool contentSizedWidth = (
                    element.style.PreferredWidth.unit == UIMeasurementUnit.Content ||
                    element.style.MinWidth.unit == UIMeasurementUnit.Content ||
                    element.style.MaxWidth.unit == UIMeasurementUnit.Content
                );

                cachedContentWidth = -1;

                if (contentSizedWidth) {
                    parent?.OnChildWidthDirty(this);
                }
            }

            if ((dirtyFlag & LayoutDirtyFlag.LayoutHorizontal) != 0) {
                flags |= AwesomeLayoutBoxFlags.RequireLayoutHorizontal;
                finalWidth = -1;
            }

            if ((dirtyFlag & LayoutDirtyFlag.InvalidateSizeVertical) != 0) {
                cachedContentWidth = -1;
                finalWidth = -1;
            }

            // layoutHistory.Add(new WidthLayout(LayoutReason.));
        }

        public void MarkContentParentsHorizontalDirty(int frameId,LayoutReason reason) {
            AwesomeLayoutBox ptr = parent;

            while (ptr != null) {
                // once we hit a block provider we can safely stop traversing since the provider doesn't care about content size changing
                if ((ptr.flags & AwesomeLayoutBoxFlags.WidthBlockProvider) != 0) {
                    break;
                }

                // can't break out if already flagged for layout because parent of parent might not be and might be content sized
                ptr.flags |= AwesomeLayoutBoxFlags.RequireLayoutHorizontal;
                ptr.element.layoutHistory.AddLogEntry(LayoutDirection.Horizontal, frameId, reason);
                ptr = ptr.parent;
            }
        }

          public void MarkContentParentsVerticalDirty(int frameId,LayoutReason reason) {
            AwesomeLayoutBox ptr = parent;

            while (ptr != null) {
                // once we hit a block provider we can safely stop traversing since the provider doesn't care about content size changing
                if ((ptr.flags & AwesomeLayoutBoxFlags.HeightBlockProvider) != 0) {
                    break;
                }

                // can't break out if already flagged for layout because parent of parent might not be and might be content sized
                ptr.flags |= AwesomeLayoutBoxFlags.RequireLayoutVertical;
                ptr.element.layoutHistory.AddLogEntry(LayoutDirection.Vertical, frameId, reason);
                ptr = ptr.parent;
            }
        }


        public struct LayoutSize {

            public float preferred;
            public float minimum;
            public float maximum;
            public float marginStart;
            public float marginEnd;

            public float Clamped {
                get {
                    float f = preferred;
                    if (preferred > maximum) {
                        f = maximum;
                    }

                    if (minimum > f) {
                        f = minimum;
                    }

                    return f;
                }
            }

        }


        [Flags]
        public enum LayoutDirtyFlag {

            LayoutHorizontal = 1,
            LayoutVertical = 1 << 1,
            InvalidateSizeHorizontal = 1 << 2,
            InvalidateSizeVertical = 1 << 3,

            All = LayoutHorizontal | LayoutVertical | InvalidateSizeHorizontal | InvalidateSizeVertical

        }

        public void UpdateBlockProviderWidth() {
            bool contentBased = (
                element.style.PreferredWidth.unit == UIMeasurementUnit.Content ||
                element.style.MinWidth.unit == UIMeasurementUnit.Content ||
                element.style.MaxWidth.unit == UIMeasurementUnit.Content
            );

            if (contentBased) {
                flags &= ~AwesomeLayoutBoxFlags.WidthBlockProvider;
            }
            else {
                flags |= AwesomeLayoutBoxFlags.WidthBlockProvider;
            }
        }

        public void UpdateBlockProviderHeight() {
            bool contentBased = (
                element.style.PreferredHeight.unit == UIMeasurementUnit.Content ||
                element.style.MinHeight.unit == UIMeasurementUnit.Content ||
                element.style.MaxHeight.unit == UIMeasurementUnit.Content
            );

            if (contentBased) {
                flags &= ~AwesomeLayoutBoxFlags.HeightBlockProvider;
            }
            else {
                flags |= AwesomeLayoutBoxFlags.HeightBlockProvider;
            }
        }

        public void UpdateRequiresHorizontalAlignment() {
            UIStyleSet style = element.style;
            AlignmentBehavior alignment = style.AlignmentBehaviorX;

            if (alignment != AlignmentBehavior.Default && alignment != AlignmentBehavior.Unset) {
                flags |= AwesomeLayoutBoxFlags.RequireAlignmentHorizontal;
                return;
            }

            if (style.AlignmentOffsetX.value != 0) {
                flags |= AwesomeLayoutBoxFlags.RequireAlignmentHorizontal;
                return;
            }

            if (style.AlignmentOriginX.value != 0) {
                flags |= AwesomeLayoutBoxFlags.RequireAlignmentHorizontal;
                return;
            }

            if (style.AlignmentDirectionX != AlignmentDirection.Start) {
                flags |= AwesomeLayoutBoxFlags.RequireAlignmentHorizontal;
                return;
            }

            flags &= ~AwesomeLayoutBoxFlags.RequireAlignmentHorizontal;
        }

        public void UpdateRequiresVerticalAlignment() {
            UIStyleSet style = element.style;
            AlignmentBehavior alignment = style.AlignmentBehaviorY;

            if (alignment != AlignmentBehavior.Default && alignment != AlignmentBehavior.Unset) {
                flags |= AwesomeLayoutBoxFlags.RequireAlignmentVertical;
                return;
            }

            if (style.AlignmentOffsetY.value != 0) {
                flags |= AwesomeLayoutBoxFlags.RequireAlignmentVertical;
                return;
            }

            if (style.AlignmentOriginY.value != 0) {
                flags |= AwesomeLayoutBoxFlags.RequireAlignmentVertical;
                return;
            }

            if (style.AlignmentDirectionY != AlignmentDirection.Start) {
                flags |= AwesomeLayoutBoxFlags.RequireAlignmentVertical;
                return;
            }

            flags &= ~AwesomeLayoutBoxFlags.RequireAlignmentVertical;
        }

    }

}