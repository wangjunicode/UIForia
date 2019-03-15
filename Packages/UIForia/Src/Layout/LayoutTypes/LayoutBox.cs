using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace UIForia.Layout.LayoutTypes {

    public abstract class LayoutBox {

        public float localX;
        public float localY;

        public float allocatedWidth;
        public float allocatedHeight;

        public float actualWidth;
        public float actualHeight;

        public UIElement element;
        public UIStyleSet style;

        public LayoutBox parent;
        public List<LayoutBox> children;

        public Scrollbar horizontalScrollbar;
        public Scrollbar verticalScrollbar;

        protected UIView view;

#if DEBUG
        public int layoutCalls;
        public int contentSizeCacheHits;
#endif

        // todo compress w/ flags
        public bool markedForLayout;
        protected float cachedPreferredWidth;

        // todo -- stop looking up style properties, cache everything locally so we dont' have to look into the Style object
        // Padding, Margin, Border, Anchors, AnchorTarget, TransformPosition, TransformPivot, Pref/Min/Max Width + Height
        
        private static readonly Dictionary<int, WidthCache> s_HeightForWidthCache = new Dictionary<int, WidthCache>();

        /*
         * Todo -- When layout happens can probably be optimized a bit
         * Figure out if parent needs to re-layout instead of assuming it does when child properties change
         * Don't always re-calculate preferred width
         */
        protected LayoutBox(UIElement element) {
            this.element = element;
            this.style = element?.style;
            this.children = ListPool<LayoutBox>.Get();
            this.cachedPreferredWidth = -1;
            Debug.Assert(element != null, nameof(this.element) + " != null");
            this.view = element.view;
        }

        public abstract void RunLayout();

        public float TransformX => ResolveTransform(style.TransformPositionX);
        public float TransformY => ResolveTransform(style.TransformPositionY);

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

        public float AnchorLeft => ResolveAnchorLeft();
        public float AnchorRight => ResolveAnchorRight();
        public float AnchorTop => ResolveAnchorTop();
        public float AnchorBottom => ResolveAnchorBottom();

        public float PaddingBorderHorizontal =>
            ResolveFixedWidth(style.PaddingLeft) +
            ResolveFixedWidth(style.PaddingRight) +
            ResolveFixedWidth(style.BorderRight) +
            ResolveFixedWidth(style.BorderLeft);

        public float PaddingBorderVertical =>
            ResolveFixedHeight(style.PaddingTop) +
            ResolveFixedHeight(style.PaddingBottom) +
            ResolveFixedHeight(style.BorderBottom) +
            ResolveFixedHeight(style.BorderTop);

        public Rect ContentRect {
            get {
                float x = PaddingLeft + BorderLeft;
                float y = PaddingTop + BorderTop;
                float width = allocatedWidth - x - PaddingRight - BorderRight;
                float height = allocatedHeight - y - PaddingBottom - BorderBottom;
                return new Rect(x, y, Mathf.Max(0, width), Mathf.Max(0, height));
            }
        }

        public Vector2 Pivot => new Vector2(
            ResolveFixedWidth(style.TransformPivotX),
            ResolveFixedHeight(style.TransformPivotY)
        );

        public float GetMarginTop(float width) {
            return ResolveMarginVertical(width, style.MarginTop);
        }

        public float GetMarginBottom(float width) {
            return ResolveMarginVertical(width, style.MarginBottom);
        }

        public float GetMarginLeft() {
            return ResolveMarginHorizontal(style.MarginLeft);
        }

        public float GetMarginRight() {
            return ResolveMarginHorizontal(style.MarginRight);
        }

        public virtual void OnInitialize() { }

        public void SetParent(LayoutBox parent) {
            this.parent?.OnChildRemoved(this);
            this.parent = parent;
            if (element.isEnabled && style.LayoutBehavior != LayoutBehavior.Ignored) {
                this.parent?.OnChildAdded(this);
            }
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

        protected void OnChildRemoved(LayoutBox child) {
            if (!children.Remove(child)) {
                return;
            }

            if ((child.style.LayoutBehavior & LayoutBehavior.Ignored) == 0) {
                RequestContentSizeChangeLayout();
            }
        }
 
        protected void AdoptChildren(LayoutBox box) {
            for (int i = 0; i < box.children.Count; i++) {
                OnChildAdded(box.children[i]);
            }

            RequestContentSizeChangeLayout();
        }

        public void RequestContentSizeChangeLayout() {
            // not 100% sure this is safe
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
                    return width.value * view.ScaleFactor;

                case UIFixedUnit.Percent:
                    return allocatedWidth * width.value;

                case UIFixedUnit.ViewportHeight:
                    return view.Viewport.height * width.value;

                case UIFixedUnit.ViewportWidth:
                    return view.Viewport.width * width.value;

                case UIFixedUnit.Em:
                    return style.EmSize * width.value * view.ScaleFactor;

                case UIFixedUnit.LineHeight:
                    return style.LineHeightSize * width.value;

                default:
                    return 0;
            }
        }

        [DebuggerStepThrough]
        protected float ResolveFixedHeight(UIFixedLength height) {
            switch (height.unit) {
                case UIFixedUnit.Pixel:
                    return height.value * view.ScaleFactor;

                case UIFixedUnit.Percent:
                    return allocatedHeight * height.value;

                case UIFixedUnit.ViewportHeight:
                    return view.Viewport.height * height.value;

                case UIFixedUnit.ViewportWidth:
                    return view.Viewport.width * height.value;

                case UIFixedUnit.Em:
                    return style.EmSize * height.value * view.ScaleFactor;

                case UIFixedUnit.LineHeight:
                    return style.LineHeightSize * height.value;

                default:
                    return 0;
            }
        }

        protected float ResolveMarginVertical(float width, UIMeasurement margin) {
            AnchorTarget anchorTarget;
            switch (margin.unit) {
                case UIMeasurementUnit.Pixel:
                    return margin.value * view.ScaleFactor;

                case UIMeasurementUnit.Content:
                    return GetContentHeight(width) * margin.value;

                case UIMeasurementUnit.ParentSize:
                    if (parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }

                    return parent.allocatedHeight * margin.value;

                case UIMeasurementUnit.ViewportWidth:
                    return view.Viewport.width * margin.value;

                case UIMeasurementUnit.ViewportHeight:
                    return view.Viewport.height * margin.value;

                case UIMeasurementUnit.ParentContentArea:
                    if (parent.style.PreferredHeight.IsContentBased) {
                        return 0f;
                    }

                    return parent.allocatedHeight * margin.value -
                           (parent.style == null ? 0 : parent.PaddingBorderVertical);

                case UIMeasurementUnit.Em:
                    return style.EmSize * margin.value * view.ScaleFactor;

                case UIMeasurementUnit.AnchorWidth:
                    anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredHeight.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(margin.value);

                case UIMeasurementUnit.AnchorHeight:
                    anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredHeight.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorHeight(margin.value);

                case UIMeasurementUnit.Unset:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public virtual void OnStylePropertyChanged(LightList<StyleProperty> property) {
            
        }

        public virtual void OnChildStylePropertyChanged(LayoutBox child, LightList<StyleProperty> property) {

        }

        protected static int FindLayoutSiblingIndex(UIElement element) {
            // if parent is not in layout
            // we want to replace it
            // so find parent's sibling index
            // spin through laid out children until finding target
            // use parent index + child index
            if (element.parent == null) return 0;

            int idx = 0;
            for (int i = 0; i < element.parent.children.Count; i++) {
                UIElement sibling = element.parent.children[i];
                if (sibling == element) {
                    break;
                }

                if ((sibling.style.LayoutBehavior & LayoutBehavior.Ignored) == 0) {
                    idx++;
                }
            }

            return idx;
        }

        public void InvalidatePreferredSizeCache() {
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

        public float GetPreferredWidth() {
            AnchorTarget anchorTarget;
            UIMeasurement widthMeasurement = style.PreferredWidth;
            switch (widthMeasurement.unit) {
                case UIMeasurementUnit.Pixel:
                    return view.ScaleFactor * Mathf.Max(0, widthMeasurement.value);

                case UIMeasurementUnit.Content:
                    return Mathf.Max(0, PaddingBorderHorizontal + (GetContentWidth() * widthMeasurement.value));

                case UIMeasurementUnit.ParentSize:
                    if (parent == null) return view.Viewport.width;
                    if (parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedWidth * widthMeasurement.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, view.Viewport.width * widthMeasurement.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, view.Viewport.height * widthMeasurement.value);

                case UIMeasurementUnit.ParentContentArea:
                    if (parent == null) return view.Viewport.width;
                    if (parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedWidth - parent.PaddingBorderHorizontal) * widthMeasurement.value;

                case UIMeasurementUnit.Em:
                    return Math.Max(0, style.EmSize * widthMeasurement.value) * view.ScaleFactor;

                case UIMeasurementUnit.AnchorWidth:
                    anchorTarget = style.AnchorTarget;
                    if (parent == null) return view.Viewport.width;
                    if (parent.style.PreferredWidth.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(widthMeasurement.value);

                case UIMeasurementUnit.AnchorHeight:
                    anchorTarget = style.AnchorTarget;
                    if (parent == null) return view.Viewport.height;
                    if (parent.style.PreferredWidth.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorHeight(widthMeasurement.value);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected float ResolveAnchorWidth(float widthMeasurement) {
            return Mathf.Max(ResolveAnchorRight() - ResolveAnchorLeft(), 0) * widthMeasurement;
        }

        protected float ResolveAnchorHeight(float heightMeasurement) {
            return Mathf.Max(ResolveAnchorBottom() - ResolveAnchorTop(), 0) * heightMeasurement;
        }

        protected float ResolveAnchorValue(float width, UIFixedLength anchor) {
            switch (anchor.unit) {
                case UIFixedUnit.Unset:
                    return 0;

                case UIFixedUnit.Pixel:
                    return anchor.value;

                case UIFixedUnit.Percent:
                    return width * anchor.value;

                case UIFixedUnit.Em:
                    return style.EmSize * anchor.value * view.ScaleFactor;

                case UIFixedUnit.LineHeight:
                    return 0;

                case UIFixedUnit.ViewportWidth:
                    return view.Viewport.width * anchor.value;

                case UIFixedUnit.ViewportHeight:
                    return view.Viewport.height * anchor.value;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected float ResolveAnchorRight() {
            UIFixedLength anchor = style.AnchorRight;
            switch (style.AnchorTarget) {
                case AnchorTarget.Unset:
                case AnchorTarget.Parent:
                    if (parent == null) {
                        return view.Viewport.xMax - ResolveAnchorValue(view.Viewport.width, anchor);
                    }

                    // needs to be allocated width not actualWidth because we might not know the actual width yet
                    return parent.element.layoutResult.screenPosition.x + parent.allocatedWidth - ResolveAnchorValue(view.Viewport.width, anchor);

                case AnchorTarget.ParentContentArea:
                    if (parent == null) {
                        return view.Viewport.x + ResolveAnchorValue(view.Viewport.width, anchor);
                    }

                    Rect parentContentRect = parent.ContentRect;
                    return parentContentRect.x +
                           parentContentRect.width - ResolveAnchorValue(parentContentRect.width, anchor);

                case AnchorTarget.Viewport:
                    return view.Viewport.x +
                           view.Viewport.width - ResolveAnchorValue(view.Viewport.width, anchor);

                case AnchorTarget.Screen:
                    if (parent == null) {
                        return -view.Viewport.x + ResolveAnchorValue(Screen.width, anchor);
                    }

                    return -parent.element.layoutResult.screenPosition.x +
                           Screen.width - ResolveAnchorValue(Screen.width, anchor);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected float ResolveAnchorLeft() {
            UIFixedLength anchor = style.AnchorLeft;
            switch (style.AnchorTarget) {
                case AnchorTarget.Unset:
                case AnchorTarget.Parent:
                    if (parent == null) {
                        return view.Viewport.xMax + ResolveAnchorValue(view.Viewport.width, anchor);
                    }

                    return parent.element.layoutResult.screenPosition.x + ResolveAnchorValue(parent.actualWidth, anchor);

                case AnchorTarget.ParentContentArea:
                    if (parent == null) {
                        return view.Viewport.xMax + ResolveAnchorValue(view.Viewport.width, anchor);
                    }

                    Rect parentContentRect = parent.ContentRect;
                    return parentContentRect.xMax + ResolveAnchorValue(parentContentRect.width, anchor);

                case AnchorTarget.Viewport:
                    return view.Viewport.xMax + ResolveAnchorValue(view.Viewport.width, anchor);

                case AnchorTarget.Screen:
                    if (parent == null) {
                        return -(view.Viewport.xMax) + ResolveAnchorValue(Screen.width, anchor);
                    }

                    return -parent.element.layoutResult.screenPosition.x + Screen.width + ResolveAnchorValue(Screen.width, anchor.value);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected float ResolveAnchorTop() {
            UIFixedLength anchor = style.AnchorTop;
            switch (style.AnchorTarget) {
                case AnchorTarget.Unset:
                case AnchorTarget.Parent:
                    if (parent == null) {
                        return view.Viewport.y + ResolveAnchorValue(view.Viewport.height, anchor);
                    }

                    return ResolveAnchorValue(parent.actualWidth, anchor);

                case AnchorTarget.ParentContentArea:
                    if (parent == null) {
                        return view.Viewport.y + ResolveAnchorValue(view.Viewport.height, anchor);
                    }

                    Rect parentContentRect = parent.ContentRect;
                    return parentContentRect.y + ResolveAnchorValue(parentContentRect.height, anchor);

                case AnchorTarget.Viewport:
                    return view.Viewport.y + ResolveAnchorValue(view.Viewport.height, anchor);

                case AnchorTarget.Screen:
                    if (parent == null) {
                        return -view.Viewport.y + ResolveAnchorValue(Screen.height, anchor);
                    }

                    // todo -- need screen position here but might not have it correctly if parent moved this frame. maybe flag for later adjustment in layout?
                    return -parent.element.layoutResult.screenPosition.y + ResolveAnchorValue(Screen.height, anchor.value);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected float ResolveAnchorBottom() {
            UIFixedLength anchor = style.AnchorBottom;
            switch (style.AnchorTarget) {
                case AnchorTarget.Unset:
                case AnchorTarget.Parent:
                    if (parent == null) {
                        return view.Viewport.yMax + ResolveAnchorValue(view.Viewport.height, anchor) - actualHeight;
                    }

                    return parent.element.layoutResult.screenPosition.y + parent.actualHeight - ResolveAnchorValue(parent.actualHeight, anchor) - actualHeight;

                case AnchorTarget.ParentContentArea:
                    if (parent == null) {
                        return view.Viewport.yMax + ResolveAnchorValue(view.Viewport.height, anchor) - actualHeight;
                    }

                    Rect parentContentRect = parent.ContentRect;
                    return parentContentRect.yMax + ResolveAnchorValue(parentContentRect.height, anchor) - actualHeight;

                case AnchorTarget.Viewport:
                    return view.Viewport.yMax + ResolveAnchorValue(view.Viewport.height, anchor) - actualHeight;

                case AnchorTarget.Screen:
                    if (parent == null) {
                        return -(view.Viewport.yMax) + ResolveAnchorValue(Screen.height, anchor) - actualHeight;
                    }

                    return -parent.element.layoutResult.screenPosition.y + Screen.height + ResolveAnchorValue(Screen.height, anchor.value) - actualHeight;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float GetPreferredHeight(float contentWidth) {
            AnchorTarget anchorTarget;
            UIMeasurement height = style.PreferredHeight;
            switch (height.unit) {
                case UIMeasurementUnit.Pixel:
                    return Mathf.Max(0, height.value * view.ScaleFactor);

                case UIMeasurementUnit.Content:
                    float contentHeight = GetCachedHeightForWidth(contentWidth);
                    if (contentHeight == -1) {
                        float cachedWidth = allocatedWidth;
                        contentHeight = ComputeContentHeight(contentWidth);
                        SetCachedHeightForWidth(contentWidth, contentHeight);
                        allocatedWidth = cachedWidth;
                    }

                    return Mathf.Max(0, PaddingBorderVertical + (contentHeight * height.value));

                case UIMeasurementUnit.ParentSize:
                    if (parent == null) return view.Viewport.height;
                    if (parent.style.PreferredHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedHeight * height.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, view.Viewport.width * height.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, view.Viewport.height * height.value);

                case UIMeasurementUnit.ParentContentArea:
                    if (parent == null) return view.Viewport.height;
                    if (parent.style.PreferredHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0,
                        parent.allocatedHeight * height.value -
                        (parent.style == null ? 0 : parent.PaddingBorderVertical));

                case UIMeasurementUnit.Em:
                    return Mathf.Max(0, style.EmSize * height.value);

                case UIMeasurementUnit.AnchorWidth:
                    anchorTarget = style.AnchorTarget;
                    if (parent == null) return view.Viewport.width;
                    if (parent.style.PreferredHeight.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(height.value);

                case UIMeasurementUnit.AnchorHeight:
                    anchorTarget = style.AnchorTarget;
                    if (parent == null) return view.Viewport.height;
                    if (parent.style.PreferredHeight.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorHeight(height.value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public float ResolveMarginHorizontal(UIMeasurement margin) {
            AnchorTarget anchorTarget;

            switch (margin.unit) {
                case UIMeasurementUnit.Pixel:
                    return margin.value * view.ScaleFactor;

                case UIMeasurementUnit.Em:
                    return style.EmSize * margin.value * view.ScaleFactor;

                case UIMeasurementUnit.Content:
                    return GetContentWidth() * margin.value;

                case UIMeasurementUnit.ParentSize:
                    if (parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }

                    return parent.allocatedHeight * margin.value;

                case UIMeasurementUnit.ViewportWidth:
                    return view.Viewport.width * margin.value;

                case UIMeasurementUnit.ViewportHeight:
                    return view.Viewport.height * margin.value;

                case UIMeasurementUnit.ParentContentArea:
                    if (parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }

                    return parent.allocatedWidth * margin.value -
                           (parent.style == null ? 0 : parent.PaddingBorderHorizontal);


                case UIMeasurementUnit.AnchorWidth:
                    anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredWidth.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(margin.value);

                case UIMeasurementUnit.AnchorHeight:
                    anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredWidth.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorHeight(margin.value);

                case UIMeasurementUnit.Unset:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Note: Only call this AFTER layout has run
        public float ResolveTransform(TransformOffset transformOffset) {
            switch (transformOffset.unit) {
                case TransformUnit.Unset:
                    return 0;

                case TransformUnit.Pixel:
                    return transformOffset.value;
                case TransformUnit.Em:
                    return style.EmSize * transformOffset.value * view.ScaleFactor;

                case TransformUnit.ActualWidth:
                    return transformOffset.value * actualWidth;

                case TransformUnit.ActualHeight:
                    return transformOffset.value * actualHeight;

                case TransformUnit.AllocatedWidth:
                    return transformOffset.value * allocatedWidth;

                case TransformUnit.AllocatedHeight:
                    return transformOffset.value * allocatedHeight;

                case TransformUnit.ContentWidth:
                    return GetContentWidth() * transformOffset.value;

                case TransformUnit.ContentHeight:
                    // should this be allocatedWidth instead?
                    return GetContentHeight(actualWidth) * transformOffset.value;

                case TransformUnit.ContentAreaWidth:
                    float width = allocatedWidth - PaddingLeft + BorderLeft - PaddingRight - BorderRight;
                    return Mathf.Max(0, width) * transformOffset.value;

                case TransformUnit.ContentAreaHeight:
                    float height = allocatedHeight - PaddingTop + PaddingBottom - PaddingBottom - BorderBottom;
                    return Mathf.Max(0, height) * transformOffset.value;

                case TransformUnit.ViewportWidth:
                    return view.Viewport.width * transformOffset.value;

                case TransformUnit.ViewportHeight:
                    return view.Viewport.height * transformOffset.value;

                case TransformUnit.AnchorWidth: {
                    AnchorTarget anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredWidth.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(transformOffset.value);
                }
                case TransformUnit.AnchorHeight: {
                    AnchorTarget anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredHeight.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorHeight(transformOffset.value);
                }
                case TransformUnit.ParentWidth:
                    if (parent == null) return 0;
                    return parent.actualWidth * transformOffset.value;

                case TransformUnit.ParentHeight:
                    if (parent == null) return 0;
                    return parent.actualHeight * transformOffset.value;

                case TransformUnit.ParentContentAreaWidth:
                    if (parent == null) return 0;
                    return parent.ContentRect.width * transformOffset.value;

                case TransformUnit.ParentContentAreaHeight:
                    if (parent == null) return 0;
                    return parent.ContentRect.height * transformOffset.value;

                case TransformUnit.ScreenWidth:
                    return Screen.width * transformOffset.value;

                case TransformUnit.ScreenHeight:
                    return Screen.height * transformOffset.value;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [DebuggerStepThrough]
        protected float ResolveMinOrMaxWidth(UIMeasurement widthMeasurement) {
            AnchorTarget anchorTarget;
            switch (widthMeasurement.unit) {
                case UIMeasurementUnit.Pixel:
                    return Mathf.Max(0, widthMeasurement.value);

                case UIMeasurementUnit.Content:
                    return Mathf.Max(0, PaddingBorderHorizontal + (GetContentWidth() * widthMeasurement.value));

                case UIMeasurementUnit.ParentSize:
                    if (parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedWidth * widthMeasurement.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, view.Viewport.width * widthMeasurement.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, view.Viewport.height * widthMeasurement.value);

                case UIMeasurementUnit.ParentContentArea:
                    if (parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0,
                        parent.allocatedWidth * widthMeasurement.value - (parent.style == null
                            ? 0
                            : parent.PaddingHorizontal - parent.BorderHorizontal));

                case UIMeasurementUnit.Em:
                    return Math.Max(0, style.EmSize * widthMeasurement.value);

                case UIMeasurementUnit.AnchorWidth:
                    anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredWidth.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(widthMeasurement.value);

                case UIMeasurementUnit.AnchorHeight:
                    anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredWidth.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorHeight(widthMeasurement.value);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [DebuggerStepThrough]
        protected float ResolveMinOrMaxHeight(UIMeasurement heightMeasurement, float width) {
            AnchorTarget anchorTarget;
            switch (heightMeasurement.unit) {
                case UIMeasurementUnit.Pixel:
                    return Mathf.Max(0, heightMeasurement.value) * view.ScaleFactor;

                case UIMeasurementUnit.Content:
                    return Mathf.Max(0, PaddingBorderVertical + (GetContentHeight(width) * heightMeasurement.value));

                case UIMeasurementUnit.ParentSize:
                    if (parent.style.PreferredHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedHeight * heightMeasurement.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, view.Viewport.width * heightMeasurement.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, view.Viewport.height * heightMeasurement.value);

                case UIMeasurementUnit.ParentContentArea:
                    if (parent.style.PreferredHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0,
                        parent.allocatedHeight * heightMeasurement.value - (parent.style == null
                            ? 0
                            : parent.PaddingVertical - parent.BorderVertical));

                case UIMeasurementUnit.Em:
                    return Mathf.Max(0, style.EmSize * heightMeasurement.value);

                case UIMeasurementUnit.AnchorWidth:
                    anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredHeight.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(heightMeasurement.value);

                case UIMeasurementUnit.AnchorHeight:
                    anchorTarget = style.AnchorTarget;
                    if (parent.style.PreferredHeight.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorHeight(heightMeasurement.value);
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
            public readonly float clampedSize;

            public LayoutBoxSize(float minSize, float maxSize, float prfSize) {
                this.minSize = minSize;
                this.maxSize = maxSize;
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

        public OffsetRect GetMargin(float width) {
            return new OffsetRect(
                GetMarginTop(width),
                GetMarginRight(),
                GetMarginBottom(width),
                GetMarginLeft()
            );
        }

        public void SetChildren(LightList<LayoutBox> boxes) {
            children.Clear();
            for (int i = 0; i < boxes.Count; i++) {
                OnChildAdded(boxes[i]);
            }
        }

    }

}