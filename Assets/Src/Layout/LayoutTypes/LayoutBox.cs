using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rendering;
using Src.Elements;
using Src.Systems;
using Src.Util;
using UnityEngine;

namespace Src.Layout.LayoutTypes {

    public abstract class LayoutBox {

        public float localX;
        public float localY;

        public float allocatedWidth;
        public float allocatedHeight;

        public float actualWidth;
        public float actualHeight;

        public UIElement element;
        public ComputedStyle style;

        public LayoutBox parent;
        public List<LayoutBox> children;

        public VirtualScrollbar horizontalScrollbar;
        public VirtualScrollbar verticalScrollbar;

        protected LayoutSystem2 layoutSystem;

        // todo compress w/ flags
        public bool markedForLayout;
        protected float cachedPreferredWidth;

        private static readonly Dictionary<int, WidthCache> s_HeightForWidthCache = new Dictionary<int, WidthCache>();

        /*
         * Todo -- When layout happens can probably be optimized a bit
         * Figure out if parent needs to re-layout instead of assuming it does when child properties change
         * Don't always re-calculate preferred width
         * 
         */
        protected LayoutBox(LayoutSystem2 layoutSystem, UIElement element) {
            this.element = element;
            this.layoutSystem = layoutSystem;
            this.style = element?.style?.computedStyle;
            this.children = ListPool<LayoutBox>.Get();
            this.cachedPreferredWidth = -1;
        }

        public abstract void RunLayout();

//        public virtual float MinWidth => Mathf.Max(PaddingHorizontal + BorderHorizontal, ResolveMinOrMaxWidth(style.MinWidth));
//        public virtual float MaxWidth => Mathf.Max(PaddingHorizontal + BorderHorizontal, ResolveMinOrMaxWidth(style.MaxWidth));
//        public virtual float PreferredWidth => ResolveMinOrMaxWidth(style.PreferredWidth);

        // public virtual float MinHeight => Mathf.Max(PaddingVertical + BorderVertical, ResolveHeight(style.MinHeight));
        // public virtual float MaxHeight => Mathf.Max(PaddingVertical + BorderVertical, ResolveHeight(style.MaxHeight));
        // public virtual float PreferredHeight => ResolveHeight(style.PreferredHeight);

        public float TransformX => ResolveFixedWidth(style.TransformPositionX);
        public float TransformY => ResolveFixedHeight(style.TransformPositionY);

        public float PaddingHorizontal => ResolveFixedWidth(style.PaddingLeft) + ResolveFixedWidth(style.PaddingRight);
        public float BorderHorizontal => ResolveFixedWidth(style.BorderLeft) + ResolveFixedWidth(style.BorderRight);

        public float PaddingVertical => ResolveFixedHeight(style.PaddingTop) + ResolveFixedHeight(style.PaddingBottom);
        public float BorderVertical => ResolveFixedHeight(style.BorderTop) + ResolveFixedHeight(style.BorderBottom);

        public float PaddingLeft => ResolveFixedWidth(style.PaddingLeft);
        public float BorderLeft => ResolveFixedWidth(style.BorderLeft);

        public float PaddingTop => ResolveFixedHeight(style.PaddingTop);
        public float BorderTop => ResolveFixedHeight(style.BorderTop);
        public bool IsInitialized { get; set; }

        public virtual void OnInitialize() {}

        public void SetParent(LayoutBox parent) {
            this.parent?.OnChildRemoved(this);
            this.parent = parent;
            this.parent?.OnChildAdded(this);
        }

        // need layout when
        /*
         * - Child Add / Remove / Move / Enable / Disable
         * - Allocated size changes && we give a shit -> ie any child is parent dependent
         * - Parent Allocated size changes & we give a shit -> ie we are parent dependent, handled automatically
         * - Child size changes from style
         * - Child layout behavior changes
         * - Child transform properties change & we care
         * - Child constraint changes && affects output size or position
         * - Layout property changes
         */

        public void ReplaceChild(LayoutBox toReplace, LayoutBox newChild) {
            int index = children.IndexOf(toReplace);
            if (index == -1) {
                throw new Exception("Cannot replace child");
            }

            newChild.SetParent(this);
            children[index] = newChild;
            newChild.AdoptChildren(toReplace);
        }

        protected virtual void OnChildAdded(LayoutBox child) {
            if (child.element.isEnabled) {
                if ((child.style.LayoutBehavior & LayoutBehavior.Ignored) == 0) {
                    children.Add(child);
                    RequestContentSizeChangeLayout();
                }
            }
        }

        protected virtual void OnChildRemoved(LayoutBox child) {
            if (!children.Remove(child)) {
                return;
            }

            if ((child.style.LayoutBehavior & LayoutBehavior.Ignored) == 0) {
                RequestContentSizeChangeLayout();
            }
        }

        protected void AdoptChildren(LayoutBox box) {
            for (int i = 0; i < box.children.Count; i++) {
                children.Add(box.children[i]);
            }

            RequestContentSizeChangeLayout();
        }

        protected void RequestOwnSizeChangedLayout() {
            layoutSystem.RequestLayout(this);
            markedForLayout = true;
        }

        protected void RequestContentSizeChangeLayout() {
            if (markedForLayout) {
                return;
            }

            layoutSystem.RequestLayout(this);
            markedForLayout = true;
            InvalidatePreferredSizeCache();
            LayoutBox ptr = parent;
            while (ptr != null) {
                if (ptr.markedForLayout) {
                    return;
                }

                layoutSystem.RequestLayout(ptr);
                ptr.markedForLayout = true;
                ptr.InvalidatePreferredSizeCache();
                ptr = ptr.parent;
            }
        }

        public void SetAllocatedRect(float x, float y, float width, float height) {
            SetAllocatedXAndWidth(x, width);
            SetAllocatedYAndHeight(y, height);
        }

        public void SetAllocatedXAndWidth(float x, float width) {
            if (localX != x) {
                localX = x;
                layoutSystem.PositionChanged(this);
            }

            if ((int) allocatedWidth != (int) width) {
                allocatedWidth = width;
                layoutSystem.OnRectChanged(this);
                RequestOwnSizeChangedLayout();
            }
        }

        public void SetAllocatedYAndHeight(float y, float height) {
            if (localY != y) {
                localY = y;
                layoutSystem.PositionChanged(this);
            }

            if ((int) allocatedHeight != (int) height) {
                allocatedHeight = height;
                layoutSystem.OnRectChanged(this);
                RequestOwnSizeChangedLayout();
            }
        }

        public virtual void OnChildEnabled(LayoutBox child) {
            children.Add(child);
            RequestContentSizeChangeLayout();
        }

        public virtual void OnChildDisabled(LayoutBox child) {
            children.Remove(child);
            RequestContentSizeChangeLayout();
        }

        [DebuggerStepThrough]
        protected float ResolveFixedWidth(UIFixedLength width) {
            switch (width.unit) {
                case UIFixedUnit.Pixel:
                    return width.value;
                case UIFixedUnit.Percent:
                    return allocatedWidth * width.value;
                case UIFixedUnit.ViewportHeight:
                    return layoutSystem.ViewportRect.height * width.value;
                case UIFixedUnit.ViewportWidth:
                    return layoutSystem.ViewportRect.width * width.value;
                case UIFixedUnit.Em:
                    return style.FontAsset.asset.fontInfo.PointSize * width.value;
                default:
                    return 0;
            }
        }

        [DebuggerStepThrough]
        protected float ResolveFixedHeight(UIFixedLength height) {
            switch (height.unit) {
                case UIFixedUnit.Pixel:
                    return height.value;
                case UIFixedUnit.Percent:
                    return allocatedHeight * height.value;
                case UIFixedUnit.ViewportHeight:
                    return layoutSystem.ViewportRect.height * height.value;
                case UIFixedUnit.ViewportWidth:
                    return layoutSystem.ViewportRect.width * height.value;
                case UIFixedUnit.Em:
                    return style.FontAsset.asset.fontInfo.PointSize * height.value;
                default:
                    return 0;
            }
        }


        public virtual void OnStylePropertyChanged(StyleProperty property) {
            switch (property.propertyId) {
                case StylePropertyId.MinWidth:
                case StylePropertyId.MaxWidth:
                case StylePropertyId.PreferredWidth:
                    InvalidatePreferredSizeCache();
                    break;
                case StylePropertyId.MinHeight:
                case StylePropertyId.MaxHeight:
                case StylePropertyId.PreferredHeight:
                    InvalidatePreferredSizeCache();
                    break;
            }
        }

        public virtual void OnChildStylePropertyChanged(LayoutBox child, StyleProperty property) {
            switch (property.propertyId) {
                case StylePropertyId.MinWidth:
                case StylePropertyId.MaxWidth:
                case StylePropertyId.PreferredWidth:
                    RequestContentSizeChangeLayout();
                    InvalidatePreferredSizeCache();
                    break;
                case StylePropertyId.MinHeight:
                case StylePropertyId.MaxHeight:
                case StylePropertyId.PreferredHeight:
                    RequestContentSizeChangeLayout();
                    InvalidatePreferredSizeCache();
                    break;
            }
        }

        protected static int FindLayoutSiblingIndex(UIElement element) {
            // if parent is not in layout
            // we want to replace it
            // so find parent's sibling index
            // spin through laid out children until finding target
            // use parent index + child index
            if (element.parent == null) return 0;

            int idx = 0;
            for (int i = 0; i < element.parent.ownChildren.Length; i++) {
                UIElement sibling = element.parent.ownChildren[i];
                if (sibling == element) {
                    break;
                }

                if ((sibling.flags & UIElementFlags.RequiresLayout) != 0 && (sibling.style.computedStyle.LayoutBehavior & LayoutBehavior.Ignored) == 0) {
                    idx++;
                }
            }

            if ((element.parent.flags & UIElementFlags.RequiresLayout) == 0) {
                idx += FindLayoutSiblingIndex(element.parent);
            }

            return idx;
        }

        protected void InvalidatePreferredSizeCache() {
            cachedPreferredWidth = -1;
            if (element != null) {
                s_HeightForWidthCache.Remove(element.id);
            }
        }

        protected void SetCachedHeightForWidth(float width, float height) {
            WidthCache retn;
            int intWidth = (int) width;
            if (s_HeightForWidthCache.TryGetValue(element.id, out retn)) {
                if (retn.next == 0) {
                    retn.next = 1;
                    retn.width0 = intWidth;
                    retn.height0 = height;
                }
                else if (retn.next == 1) {
                    retn.next = 2;
                    retn.width1 = intWidth;
                    retn.height1 = height;
                }
                else {
                    retn.next = 0;
                    retn.width2 = intWidth;
                    retn.height2 = height;
                }
            }
            else {
                retn.next = 1;
                retn.width0 = intWidth;
                retn.height0 = height;
            }

            s_HeightForWidthCache[element.id] = retn;
        }

        protected float GetCachedHeightForWidth(float width) {
            WidthCache retn;
            int intWidth = (int) width;
            if (s_HeightForWidthCache.TryGetValue(element.id, out retn)) {
                if (retn.width0 == intWidth) {
                    return retn.height0;
                }

                if (retn.width1 == intWidth) {
                    return retn.height1;
                }

                if (retn.width2 == intWidth) {
                    return retn.height2;
                }

                return -1;
            }

            return -1;
        }

        private struct WidthCache {

            public int next;

            public int width0;
            public int width1;
            public int width2;

            public float height0;
            public float height1;
            public float height2;

        }

        protected virtual float ComputeContentWidth() {
            return 0f;
        }
        
        protected virtual float ComputeContentHeight(float width) {
            return 0f;
        }

        private float GetContentWidth() {
            if (cachedPreferredWidth == -1) {
                cachedPreferredWidth = ComputeContentWidth();
            }

            return cachedPreferredWidth;   
        }

        private float GetContentHeight(float width) {
            float cachedHeight = GetCachedHeightForWidth(width);
            if (cachedHeight == -1) {
                cachedHeight = ComputeContentHeight(width);
                SetCachedHeightForWidth(width, cachedHeight);
            }

            return cachedHeight;
        }
        
        public float GetPreferredWidth() {
            UIMeasurement widthMeasurement = style.PreferredWidth;
            switch (widthMeasurement.unit) {
                case UIUnit.Pixel:
                    return Mathf.Max(0, widthMeasurement.value);

                case UIUnit.Content:
                    if (cachedPreferredWidth == -1) {
                        cachedPreferredWidth = ComputeContentWidth();
                    }

                    return Mathf.Max(0, PaddingHorizontal + BorderHorizontal + (cachedPreferredWidth * widthMeasurement.value));

                case UIUnit.ParentSize:
                    if (parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }
                    return Mathf.Max(0, parent.allocatedWidth * widthMeasurement.value);

                case UIUnit.ViewportWidth:
                    return Mathf.Max(0, layoutSystem.ViewportRect.width * widthMeasurement.value);

                case UIUnit.ViewportHeight:
                    return Mathf.Max(0, layoutSystem.ViewportRect.height * widthMeasurement.value);

                case UIUnit.ParentContentArea:
                    if (parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }
                    return Mathf.Max(0, parent.allocatedWidth * widthMeasurement.value - (parent.style == null ? 0 : parent.PaddingHorizontal - parent.BorderHorizontal));

                case UIUnit.Em:
                    return Math.Max(0, style.FontAsset.asset.fontInfo.PointSize * widthMeasurement.value);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float GetPreferredHeight(float contentWidth) {
            UIMeasurement height = style.PreferredHeight;
            switch (height.unit) {
                case UIUnit.Pixel:
                    return Mathf.Max(0, height.value);

                case UIUnit.Content:
                    float contentHeight = GetCachedHeightForWidth(contentWidth);
                    if (contentHeight == -1) {
                        float cachedWidth = allocatedWidth;
                        contentHeight = ComputeContentHeight(contentWidth);
                        SetCachedHeightForWidth(contentWidth, contentHeight);
                        allocatedWidth = cachedWidth;
                    }

                    return PaddingVertical + BorderVertical + contentHeight * height.value;

                case UIUnit.ParentSize:
                    if (parent.style.PreferredHeight.IsContentBased) {
                        return 0f;
                    }
                    return Mathf.Max(0, parent.allocatedHeight * height.value);

                case UIUnit.ViewportWidth:
                    return Mathf.Max(0, layoutSystem.ViewportRect.width * height.value);

                case UIUnit.ViewportHeight:
                    return Mathf.Max(0, layoutSystem.ViewportRect.height * height.value);

                case UIUnit.ParentContentArea:
                    if (parent.style.PreferredHeight.IsContentBased) {
                        return 0f;
                    }
                    return Mathf.Max(0, parent.allocatedHeight * height.value - (parent.style == null ? 0 : parent.PaddingVertical - parent.BorderVertical));

                case UIUnit.Em:
                    return Mathf.Max(0, style.FontAsset.asset.fontInfo.PointSize * height.value);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [DebuggerStepThrough]
        public float GetMinHeight(float contentHeight) {
            return ResolveMinOrMaxHeight(style.MinHeight, contentHeight);
        }

        [DebuggerStepThrough]
        public float GetMaxHeight(float contentHeight) {
            return ResolveMinOrMaxHeight(style.MaxHeight, contentHeight);
        }

//        [DebuggerStepThrough]
        protected float ResolveMinOrMaxWidth(UIMeasurement widthMeasurement) {
            switch (widthMeasurement.unit) {
                case UIUnit.Pixel:
                    return Mathf.Max(0, widthMeasurement.value);

                case UIUnit.Content:
                    return Mathf.Max(0, PaddingHorizontal + BorderHorizontal + (GetContentWidth() * widthMeasurement.value));

                case UIUnit.ParentSize:
                    if (parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }
                    return Mathf.Max(0, parent.allocatedWidth * widthMeasurement.value);

                case UIUnit.ViewportWidth:
                    return Mathf.Max(0, layoutSystem.ViewportRect.width * widthMeasurement.value);

                case UIUnit.ViewportHeight:
                    return Mathf.Max(0, layoutSystem.ViewportRect.height * widthMeasurement.value);

                case UIUnit.ParentContentArea:
                    if (parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }
                    return Mathf.Max(0, parent.allocatedWidth * widthMeasurement.value - (parent.style == null ? 0 : parent.PaddingHorizontal - parent.BorderHorizontal));

                case UIUnit.Em:
                    return Math.Max(0, style.FontAsset.asset.fontInfo.PointSize * widthMeasurement.value);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

//        [DebuggerStepThrough]
        protected float ResolveMinOrMaxHeight(UIMeasurement heightMeasurement, float width) {
            switch (heightMeasurement.unit) {
                case UIUnit.Pixel:
                    return Mathf.Max(0, heightMeasurement.value);

                case UIUnit.Content:
                    return Mathf.Max(0, GetContentHeight(width) * heightMeasurement.value);

                case UIUnit.ParentSize:
                    if (parent.style.PreferredHeight.IsContentBased) {
                        return 0f;
                    }
                    return Mathf.Max(0, parent.allocatedHeight * heightMeasurement.value);

                case UIUnit.ViewportWidth:
                    return Mathf.Max(0, layoutSystem.ViewportRect.width * heightMeasurement.value);

                case UIUnit.ViewportHeight:
                    return Mathf.Max(0, layoutSystem.ViewportRect.height * heightMeasurement.value);

                case UIUnit.ParentContentArea:
                    if (parent.style.PreferredHeight.IsContentBased) {
                        return 0f;
                    }
                    return Mathf.Max(0, parent.allocatedHeight * heightMeasurement.value - (parent.style == null ? 0 : parent.PaddingVertical - parent.BorderVertical));

                case UIUnit.Em:
                    return Mathf.Max(0, style.FontAsset.asset.fontInfo.PointSize * heightMeasurement.value);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public LayoutBoxSize GetHeights(float width) {
            float prfHeight = GetPreferredHeight(width);
            float minHeight = ResolveMinOrMaxHeight(style.MinHeight, width);
            float maxHeight = ResolveMinOrMaxHeight(style.MaxHeight, width);
            return new LayoutBoxSize(minHeight, maxHeight, prfHeight);
        }

        public LayoutBoxSize GetWidths() {
            float prfWidth = GetPreferredWidth();
            float minWidth = ResolveMinOrMaxWidth(style.MinWidth);
            float maxWidth = ResolveMinOrMaxWidth(style.MaxWidth);
            
            return new LayoutBoxSize(minWidth, maxWidth, prfWidth);
        }

        public struct LayoutBoxSize {

            public readonly float minSize;
            public readonly float maxSize;
            public readonly float prfSize;
            public readonly float clampedSize;

            public LayoutBoxSize(float minSize, float maxSize, float prfSize) {
                this.minSize = minSize;
                this.maxSize = maxSize;
                this.prfSize = prfSize;
                this.clampedSize = Mathf.Max(minSize, Mathf.Min(prfSize, maxSize));
            }

        }

    }

}