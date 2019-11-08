using System;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIForia.Systems {

    public abstract class AwesomeLayoutBox {

        public float baseLocalX;
        public float alignedX;
        public float baseWidth;
        public float finalWidth;

        public float baseLocalY;
        public float alignedY;
        public float baseHeight;
        public float finalHeight;

        public float paddingBorderHorizontalStart;
        public float paddingBorderHorizontalEnd;
        public float paddingBorderVerticalStart;
        public float paddingBorderVerticalEnd;

        public AwesomeLayoutBoxFlags flags;
        public float currentHorizontalSize;
        public float cachedContentWidth;
        public float cachedContentHeight;
        public LayoutType layoutBoxType;

        public int childCount;
        public UIElement element;
        public AwesomeLayoutBox firstChild;
        public AwesomeLayoutBox nextSibling;
        public AwesomeLayoutBox parent;

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
            flags |= AwesomeLayoutBoxFlags.RequireLayoutHorizontal | AwesomeLayoutBoxFlags.RequireLayoutVertical;
            UpdateBlockProviderWidth();
            UpdateBlockProviderHeight();
            OnInitialize();
        }

        protected virtual void OnInitialize() { }

        public void Destroy() {
            flags = 0;
            element = null;
            parent = null;
            nextSibling = null;
            firstChild = null;
        }

        protected abstract float ComputeContentWidth();

        protected abstract float ComputeContentHeight();

        public void SetChildren(LightList<AwesomeLayoutBox> layoutBoxes) {
            firstChild = null;
            childCount = layoutBoxes.size;
            if (childCount > 0) {
                firstChild = layoutBoxes[0];
                firstChild.parent = this;
                AwesomeLayoutBox ptr = firstChild;
                for (int i = 1; i < layoutBoxes.size; i++) {
                    layoutBoxes.array[i].parent = this;
                    ptr.nextSibling = layoutBoxes.array[i];
                    ptr = ptr.nextSibling;
                }
            }

            OnChildrenChanged(layoutBoxes);
        }

        public abstract void OnChildrenChanged(LightList<AwesomeLayoutBox> childList);

        public void ApplyLayoutHorizontal(float localX, float size, float availableSize, LayoutFit defaultFit, int frameId) {
            baseWidth = size;
            baseLocalX = localX;

            LayoutFit fit = element.style.LayoutFitHorizontal;
            if (fit == LayoutFit.Default) {
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
                    }

                    break;

                case LayoutFit.Shrink:
                    if (availableSize < size) {
                        newWidth = availableSize;
                    }

                    break;

                case LayoutFit.Fill:
                    newWidth = availableSize;
                    break;
            }

            Vector2 viewSize = element.View.Viewport.size;
            float emSize = 0; // todo -- read off of style (cached)
            float paddingLeft = MeasurementUtil.ResolveFixedSize(newWidth, viewSize.x, viewSize.y, emSize, element.style.PaddingLeft);
            float paddingRight = MeasurementUtil.ResolveFixedSize(newWidth, viewSize.x, viewSize.y, emSize, element.style.PaddingRight);
            float borderLeft = MeasurementUtil.ResolveFixedSize(newWidth, viewSize.x, viewSize.y, emSize, element.style.BorderLeft);
            float borderRight = MeasurementUtil.ResolveFixedSize(newWidth, viewSize.x, viewSize.y, emSize, element.style.BorderRight);

            paddingBorderHorizontalStart = paddingLeft + borderLeft;
            paddingBorderHorizontalEnd = paddingRight + borderRight;

            if (newWidth != finalWidth) {
                flags |= AwesomeLayoutBoxFlags.RequireLayoutHorizontal;
                element.layoutHistory.AddLogEntry(LayoutDirection.Horizontal, frameId, LayoutReason.FinalSizeChanged, string.Empty);
                finalWidth = newWidth;
            }
        }

        public void ApplyLayoutVertical(float localY, float size, float availableSize, LayoutFit defaultFit) { }

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
                    AwesomeLayoutBox ptr = parent;
                    while (ptr != null) {
                        if ((ptr.flags & AwesomeLayoutBoxFlags.WidthBlockProvider) != 0) {
                            Assert.AreNotEqual(-1, ptr.finalWidth);
                            return Math.Max(0, ptr.finalWidth * measurement.value);
                        }

                        ptr = ptr.parent;
                    }

                    return Math.Max(0, element.View.Viewport.width * measurement.value);
                }
                case UIMeasurementUnit.Percentage:
                case UIMeasurementUnit.ParentContentArea: {
                    AwesomeLayoutBox ptr = parent;
                    float paddingBorder = 0;
                    while (ptr != null) {
                        paddingBorder += ptr.paddingBorderHorizontalStart + ptr.paddingBorderHorizontalEnd;
                        if ((ptr.flags & AwesomeLayoutBoxFlags.WidthBlockProvider) != 0) {
                            Assert.AreNotEqual(-1, ptr.finalWidth);
                            return Math.Max(0, (ptr.finalWidth - paddingBorder) * measurement.value);
                        }

                        ptr = ptr.parent;
                    }

                    return Math.Max(0, (element.View.Viewport.width - paddingBorder) * measurement.value);
                }
            }

            return 0;
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

                    baseVal += MeasurementUtil.ResolveFixedSize(height, viewSize.x, viewSize.y, 0, element.style.PaddingTop);
                    baseVal += MeasurementUtil.ResolveFixedSize(height, viewSize.x, viewSize.y, 0, element.style.PaddingBottom);
                    baseVal += MeasurementUtil.ResolveFixedSize(height, viewSize.x, viewSize.y, 0, element.style.BorderTop);
                    baseVal += MeasurementUtil.ResolveFixedSize(height, viewSize.x, viewSize.y, 0, element.style.BorderBottom);

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

                case UIMeasurementUnit.BlockSize:

                    return 0;

                case UIMeasurementUnit.Percentage:
                case UIMeasurementUnit.ParentContentArea:
                    return 0;
            }

            return 0;
        }

        public abstract void RunLayoutHorizontal(int frameId);

        public abstract void RunLayoutVertical();

        public void GetWidths(ref LayoutSize size) {
            size.preferred = ResolveWidth(element.style.PreferredWidth);
            size.minimum = ResolveWidth(element.style.MinWidth);
            size.maximum = ResolveWidth(element.style.MaxWidth);
            size.marginStart = MeasurementUtil.ResolveFixedSize(0, 0, 0, 0, element.style.MarginLeft);
            size.marginEnd = MeasurementUtil.ResolveFixedSize(0, 0, 0, 0, element.style.MarginRight);
        }

        public LayoutSize GetHeights() {
            LayoutSize size = default;
            size.preferred = ResolveHeight(element.style.PreferredHeight);
            size.minimum = ResolveHeight(element.style.MinHeight);
            size.maximum = ResolveHeight(element.style.MaxHeight);
            size.marginStart = MeasurementUtil.ResolveFixedSize(0, 0, 0, 0, element.style.MarginTop);
            size.marginEnd = MeasurementUtil.ResolveFixedSize(0, 0, 0, 0, element.style.MarginBottom);
            return size;
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

        public struct LayoutSize {

            public float preferred;
            public float minimum;
            public float maximum;
            public float marginStart;
            public float marginEnd;

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

    }

}