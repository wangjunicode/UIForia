using System;
using System.Collections.Generic;
using System.Diagnostics;
using Src.Rendering;
using Src.Elements;
using Src.Systems;
using Src.Util;
using UIForia;
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

        protected LayoutSystem layoutSystem;

#if DEBUG
        public int layoutCalls;
        public int contentSizeCacheHits;
#endif

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
        protected LayoutBox(LayoutSystem layoutSystem, UIElement element) {
            this.element = element;
            this.layoutSystem = layoutSystem;
            this.style = element?.style?.computedStyle;
            this.children = ListPool<LayoutBox>.Get();
            this.cachedPreferredWidth = -1;
        }

        public abstract void RunLayout();

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

        public float PaddingBottom => ResolveFixedHeight(style.PaddingBottom);
        public float PaddingRight => ResolveFixedWidth(style.PaddingRight);

        public float BorderBottom => ResolveFixedHeight(style.BorderBottom);
        public float BorderRight => ResolveFixedWidth(style.BorderRight);

        public bool IsInitialized { get; set; }
        public bool IsIgnored => (style.LayoutBehavior & LayoutBehavior.Ignored) != 0;

        public float ContentOffsetLeft => ResolveFixedWidth(style.PaddingLeft) + ResolveFixedWidth(style.BorderLeft);
        public float ContentOffsetTop => ResolveFixedWidth(style.PaddingTop) + ResolveFixedWidth(style.BorderTop);

        public float AnchorLeft => ResolveHorizontalAnchor(style.AnchorLeft);
        public float AnchorRight => ResolveHorizontalAnchor(style.AnchorRight);
        public float AnchorTop => ResolveVerticalAnchor(style.AnchorTop);
        public float AnchorBottom => ResolveVerticalAnchor(style.AnchorBottom);

        public virtual void OnInitialize() { }

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

        protected void RequestContentSizeChangeLayout() {
            if (markedForLayout) {
                return;
            }

            markedForLayout = true;
            InvalidatePreferredSizeCache();
            LayoutBox ptr = parent;
            while (ptr != null) {
                // not 100% sure this is safe
                if (ptr.markedForLayout) {
                    return;
                }

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
            localX = x;
            if ((int) allocatedWidth != (int) width) {
                allocatedWidth = width;
                markedForLayout = true; // todo might not need it, delegate to virtual fn 
            }
        }

        public void SetAllocatedYAndHeight(float y, float height) {
            localY = y;
            if ((int) allocatedHeight != (int) height) {
                allocatedHeight = height;
                markedForLayout = true; // todo might not need it, delegate to virtual fn 
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
                    return style.EmSize * width.value;

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
                    return style.EmSize * height.value;

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
                case StylePropertyId.AnchorTop:
                case StylePropertyId.AnchorRight:
                case StylePropertyId.AnchorBottom:
                case StylePropertyId.AnchorLeft:
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
                case StylePropertyId.AnchorTop:
                case StylePropertyId.AnchorRight:
                case StylePropertyId.AnchorBottom:
                case StylePropertyId.AnchorLeft:
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
#if DEBUG
                    contentSizeCacheHits++;
#endif
                    return retn.height0;
                }

                if (retn.width1 == intWidth) {
#if DEBUG
                    contentSizeCacheHits++;
#endif
                    return retn.height1;
                }

                if (retn.width2 == intWidth) {
#if DEBUG
                    contentSizeCacheHits++;
#endif
                    return retn.height2;
                }

                return -1;
            }

            return -1;
        }

        protected virtual float ComputeContentWidth() {
            return 0f;
        }

        protected virtual float ComputeContentHeight(float width) {
            return 0f;
        }

        private float GetContentWidth() {
            // todo -- get some stats on this
            if (cachedPreferredWidth == -1) {
                cachedPreferredWidth = ComputeContentWidth();
            }
#if DEBUG
            else {
                contentSizeCacheHits++;
            }
#endif

            return cachedPreferredWidth;
        }

        private float GetContentHeight(float width) {
            // todo -- get some stats on this
            float cachedHeight = GetCachedHeightForWidth(width);
            if (cachedHeight == -1) {
                cachedHeight = ComputeContentHeight(width);
                SetCachedHeightForWidth(width, cachedHeight);
            }

            return cachedHeight;
        }

        public static float ResolveHorizontalMargin(UIMeasurement margin) {
            switch (margin.unit) {
                case UIMeasurementUnit.Pixel:
                    return margin.value;
                default:
                    throw new NotImplementedException();
            }
        }

        public float GetPreferredWidth() {
            AnchorTarget anchorTarget;
            UIMeasurement widthMeasurement = style.PreferredWidth;
            switch (widthMeasurement.unit) {
                case UIMeasurementUnit.Pixel:
                    return Mathf.Max(0, widthMeasurement.value) + ResolveHorizontalMargin(style.MarginLeft) + ResolveHorizontalMargin(style.MarginRight);

                case UIMeasurementUnit.Content:
                    cachedPreferredWidth = GetContentWidth();
                    return Mathf.Max(0, PaddingHorizontal + BorderHorizontal + (cachedPreferredWidth * widthMeasurement.value));

                case UIMeasurementUnit.ParentSize:
                    if (parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedWidth * widthMeasurement.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, layoutSystem.ViewportRect.width * widthMeasurement.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, layoutSystem.ViewportRect.height * widthMeasurement.value);

                case UIMeasurementUnit.ParentContentArea:
                    if (parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }
                    
                    return Mathf.Max(0, parent.allocatedWidth - parent.PaddingHorizontal - parent.BorderHorizontal) * widthMeasurement.value;

                case UIMeasurementUnit.Em:
                    return Math.Max(0, style.EmSize * widthMeasurement.value);

                case UIMeasurementUnit.AnchorWidth:
                    anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredWidth.IsContentBased && anchorTarget == AnchorTarget.Parent || anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(widthMeasurement);

                case UIMeasurementUnit.AnchorHeight:
                    anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredWidth.IsContentBased && anchorTarget == AnchorTarget.Parent || anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorHeight(widthMeasurement);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected float ResolveAnchorWidth(UIMeasurement widthMeasurement) {
            float left;
            float right;
            switch (style.AnchorTarget) {
                case AnchorTarget.Parent:
                    left = ResolveAnchor(parent.allocatedWidth, style.AnchorLeft);
                    right = ResolveAnchor(parent.allocatedWidth, style.AnchorRight);
                    return Mathf.Max(0, (right - left) * widthMeasurement.value);

                case AnchorTarget.ParentContentArea:
                    float contentArea = parent.allocatedWidth - parent.PaddingHorizontal - parent.BorderHorizontal;
                    left = ResolveAnchor(contentArea, style.AnchorLeft);
                    right = ResolveAnchor(contentArea, style.AnchorRight);
                    return Mathf.Max(0, (right - left) * widthMeasurement.value);

                case AnchorTarget.Screen:
                    left = ResolveAnchor(Screen.width, style.AnchorLeft);
                    right = Screen.width - ResolveAnchor(Screen.width, style.AnchorRight);
                    return Mathf.Max(0, (right - left) * widthMeasurement.value);

                case AnchorTarget.Viewport:
                    left = ResolveAnchor(layoutSystem.ViewportRect.width, style.AnchorLeft);
                    right = ResolveAnchor(layoutSystem.ViewportRect.width, style.AnchorRight);
                    return Mathf.Max(0, (right - left) * widthMeasurement.value);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected float ResolveAnchorHeight(UIMeasurement heightMeasurement) {
            float top;
            float bottom;

            switch (style.AnchorTarget) {
                case AnchorTarget.Parent:
                    top = ResolveAnchor(parent.allocatedHeight, style.AnchorTop);
                    bottom = parent.allocatedHeight - ResolveAnchor(parent.allocatedHeight, style.AnchorBottom);
                    return Mathf.Max(0, (bottom - top) * heightMeasurement.value);

                case AnchorTarget.ParentContentArea:
                    float contentArea = parent.allocatedHeight - parent.PaddingVertical - parent.BorderVertical;
                    top = ResolveAnchor(contentArea, style.AnchorTop);
                    bottom = contentArea - ResolveAnchor(contentArea, style.AnchorBottom);
                    return Mathf.Max(0, (bottom - top) * heightMeasurement.value);

                case AnchorTarget.Screen:
                    top = ResolveAnchor(Screen.height, style.AnchorTop);
                    bottom = Screen.height - ResolveAnchor(Screen.height, style.AnchorBottom);
                    return Mathf.Max(0, (bottom - top) * heightMeasurement.value);

                case AnchorTarget.Viewport:
                    top = ResolveAnchor(layoutSystem.ViewportRect.height, style.AnchorTop);
                    bottom = layoutSystem.ViewportRect.height - ResolveAnchor(layoutSystem.ViewportRect.height, style.AnchorBottom);
                    return Mathf.Max(0, (bottom - top) * heightMeasurement.value);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected float ResolveAnchor(float baseWidth, UIFixedLength anchor) {
            switch (anchor.unit) {
                case UIFixedUnit.Pixel:
                    return anchor.value;

                case UIFixedUnit.Percent:
                    return baseWidth * anchor.value;

                case UIFixedUnit.ViewportHeight:
                    return layoutSystem.ViewportRect.height * anchor.value;

                case UIFixedUnit.ViewportWidth:
                    return layoutSystem.ViewportRect.width * anchor.value;

                case UIFixedUnit.Em:
                    return style.EmSize * anchor.value;

                default:
                    throw new InvalidArgumentException();
            }
        }

        protected float ResolveHorizontalAnchor(UIFixedLength anchor) {
            switch (anchor.unit) {
                case UIFixedUnit.Pixel:
                    return anchor.value;

                case UIFixedUnit.Percent:
                    switch (style.AnchorTarget) {
                        // note -- not intended to be called by anything but the layout system
                        // which happens only for ignored layout behaviors after their parent is
                        // fully sized and laid out, meaning we don't need to return 0 for content
                        // sized parents
                        case AnchorTarget.Parent:
                            return parent.allocatedWidth * anchor.value;

                        case AnchorTarget.ParentContentArea:
                            float paddingLeft = parent.PaddingLeft;
                            float borderLeft = parent.BorderLeft;
                            float start = paddingLeft + borderLeft;
                            float end = parent.allocatedWidth - parent.PaddingHorizontal - parent.BorderHorizontal;
                            float range = end - start;
                            return start + (range * anchor.value);

                        case AnchorTarget.Screen:
                            return Screen.width * anchor.value;

                        case AnchorTarget.Viewport:
                            return layoutSystem.ViewportRect.width * anchor.value;

                        default:
                            throw new InvalidArgumentException();
                    }

                case UIFixedUnit.ViewportHeight:
                    return layoutSystem.ViewportRect.height * anchor.value;

                case UIFixedUnit.ViewportWidth:
                    return layoutSystem.ViewportRect.width * anchor.value;

                case UIFixedUnit.Em:
                    return style.EmSize * anchor.value;

                default:
                    throw new InvalidArgumentException();
            }
        }

        protected float ResolveVerticalAnchor(UIFixedLength anchor) {
            switch (anchor.unit) {
                case UIFixedUnit.Pixel:
                    return anchor.value;

                case UIFixedUnit.Percent:
                    switch (style.AnchorTarget) {
                        // note -- not intended to be called by anything but the layout system
                        // which happens only for ignored layout behaviors after their parent is
                        // fully sized and laid out, meaning we don't need to return 0 for content
                        // sized parents
                        case AnchorTarget.Parent:
                            return parent.allocatedHeight * anchor.value;

                        case AnchorTarget.ParentContentArea:
                            float paddingTop = parent.PaddingTop;
                            float borderTop = parent.BorderTop;
                            float start = paddingTop + borderTop;
                            float end = parent.allocatedHeight - parent.PaddingVertical - parent.BorderVertical;
                            float range = end - start;
                            return start + (range * anchor.value);

                        case AnchorTarget.Screen:
                            return Screen.height * anchor.value;

                        case AnchorTarget.Viewport:
                            return layoutSystem.ViewportRect.height * anchor.value;

                        default:
                            throw new InvalidArgumentException();
                    }

                case UIFixedUnit.ViewportHeight:
                    return layoutSystem.ViewportRect.height * anchor.value;

                case UIFixedUnit.ViewportWidth:
                    return layoutSystem.ViewportRect.width * anchor.value;

                case UIFixedUnit.Em:
                    return style.EmSize * anchor.value;

                default:
                    throw new InvalidArgumentException();
            }
        }

        public float GetPreferredHeight(float contentWidth) {
            AnchorTarget anchorTarget;
            UIMeasurement height = style.PreferredHeight;
            switch (height.unit) {
                case UIMeasurementUnit.Pixel:
                    return Mathf.Max(0, height.value);

                case UIMeasurementUnit.Content:
                    float contentHeight = GetCachedHeightForWidth(contentWidth);
                    if (contentHeight == -1) {
                        float cachedWidth = allocatedWidth;
                        contentHeight = ComputeContentHeight(contentWidth);
                        SetCachedHeightForWidth(contentWidth, contentHeight);
                        allocatedWidth = cachedWidth;
                    }
                    return PaddingVertical + BorderVertical + contentHeight * height.value;

                case UIMeasurementUnit.ParentSize:
                    if (parent.style.PreferredHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedHeight * height.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, layoutSystem.ViewportRect.width * height.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, layoutSystem.ViewportRect.height * height.value);

                case UIMeasurementUnit.ParentContentArea:
                    if (parent.style.PreferredHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedHeight * height.value - (parent.style == null ? 0 : parent.PaddingVertical - parent.BorderVertical));

                case UIMeasurementUnit.Em:
                    return Mathf.Max(0, style.EmSize * height.value);

                case UIMeasurementUnit.AnchorWidth:
                    anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredHeight.IsContentBased && anchorTarget == AnchorTarget.Parent || anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(height);

                case UIMeasurementUnit.AnchorHeight:
                    anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredHeight.IsContentBased && anchorTarget == AnchorTarget.Parent || anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorHeight(height);
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

        [DebuggerStepThrough]
        protected float ResolveMinOrMaxWidth(UIMeasurement widthMeasurement) {
            AnchorTarget anchorTarget;
            switch (widthMeasurement.unit) {
                case UIMeasurementUnit.Pixel:
                    return Mathf.Max(0, widthMeasurement.value);

                case UIMeasurementUnit.Content:
                    return Mathf.Max(0, PaddingHorizontal + BorderHorizontal + (GetContentWidth() * widthMeasurement.value));

                case UIMeasurementUnit.ParentSize:
                    if (parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedWidth * widthMeasurement.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, layoutSystem.ViewportRect.width * widthMeasurement.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, layoutSystem.ViewportRect.height * widthMeasurement.value);

                case UIMeasurementUnit.ParentContentArea:
                    if (parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedWidth * widthMeasurement.value - (parent.style == null ? 0 : parent.PaddingHorizontal - parent.BorderHorizontal));

                case UIMeasurementUnit.Em:
                    return Math.Max(0, style.EmSize * widthMeasurement.value);

                case UIMeasurementUnit.AnchorWidth:
                    anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredWidth.IsContentBased && anchorTarget == AnchorTarget.Parent || anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(widthMeasurement);

                case UIMeasurementUnit.AnchorHeight:
                    anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredWidth.IsContentBased && anchorTarget == AnchorTarget.Parent || anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorHeight(widthMeasurement);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [DebuggerStepThrough]
        protected float ResolveMinOrMaxHeight(UIMeasurement heightMeasurement, float width) {
            AnchorTarget anchorTarget;
            switch (heightMeasurement.unit) {
                case UIMeasurementUnit.Pixel:
                    return Mathf.Max(0, heightMeasurement.value);

                case UIMeasurementUnit.Content:
                    return Mathf.Max(0, GetContentHeight(width) * heightMeasurement.value);

                case UIMeasurementUnit.ParentSize:
                    if (parent.style.PreferredHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedHeight * heightMeasurement.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, layoutSystem.ViewportRect.width * heightMeasurement.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, layoutSystem.ViewportRect.height * heightMeasurement.value);

                case UIMeasurementUnit.ParentContentArea:
                    if (parent.style.PreferredHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedHeight * heightMeasurement.value - (parent.style == null ? 0 : parent.PaddingVertical - parent.BorderVertical));

                case UIMeasurementUnit.Em:
                    return Mathf.Max(0, style.EmSize * heightMeasurement.value);

                case UIMeasurementUnit.AnchorWidth:
                    anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredHeight.IsContentBased && anchorTarget == AnchorTarget.Parent || anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(heightMeasurement);

                case UIMeasurementUnit.AnchorHeight:
                    anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredHeight.IsContentBased && anchorTarget == AnchorTarget.Parent || anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorHeight(heightMeasurement);
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

        private struct WidthCache {

            public int next;

            public int width0;
            public int width1;
            public int width2;

            public float height0;
            public float height1;
            public float height2;

        }

    }

}