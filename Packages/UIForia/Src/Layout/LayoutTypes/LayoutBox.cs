using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

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

        internal UIFixedLength paddingTop;
        internal UIFixedLength paddingRight;
        internal UIFixedLength paddingBottom;
        internal UIFixedLength paddingLeft;

        internal UIFixedLength borderTop;
        internal UIFixedLength borderRight;
        internal UIFixedLength borderBottom;
        internal UIFixedLength borderLeft;

        internal UIFixedLength borderRadiusTopLeft;
        internal UIFixedLength borderRadiusTopRight;
        internal UIFixedLength borderRadiusBottomLeft;
        internal UIFixedLength borderRadiusBottomRight;

        internal UIMeasurement marginTop;
        internal UIMeasurement marginRight;
        internal UIMeasurement marginBottom;
        internal UIMeasurement marginLeft;

        internal float transformRotation;
        internal float transformScaleX;
        internal float transformScaleY;

        internal int zIndex;

        internal UIMeasurement prefWidth;
        internal UIMeasurement minWidth;
        internal UIMeasurement maxWidth;

        internal UIMeasurement prefHeight;
        internal UIMeasurement minHeight;
        internal UIMeasurement maxHeight;

        internal UIFixedLength transformPivotX;
        internal UIFixedLength transformPivotY;
        internal TransformOffset transformPositionX;
        internal TransformOffset transformPositionY;
        internal TransformBehavior transformBehaviorX;
        internal TransformBehavior transformBehaviorY;

        internal bool isInPool;
        internal LayoutBoxPool pool;
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
        protected LayoutBox() {
         
        }

        public abstract void RunLayout();

        public float TransformX => ResolveTransform(transformPositionX);
        public float TransformY => ResolveTransform(transformPositionY);

        public float PaddingHorizontal => ResolveFixedWidth(paddingLeft) + ResolveFixedWidth(paddingRight);
        public float BorderHorizontal => ResolveFixedWidth(borderLeft) + ResolveFixedWidth(borderRight);

        public float PaddingVertical => ResolveFixedHeight(paddingTop) + ResolveFixedHeight(paddingBottom);
        public float BorderVertical => ResolveFixedHeight(borderTop) + ResolveFixedHeight(borderBottom);

        public float PaddingLeft => ResolveFixedWidth(paddingLeft);
        public float BorderLeft => ResolveFixedWidth(borderLeft);

        public float PaddingTop => ResolveFixedHeight(paddingTop);
        public float BorderTop => ResolveFixedHeight(borderTop);

        public float PaddingBottom => ResolveFixedHeight(paddingBottom);
        public float PaddingRight => ResolveFixedWidth(paddingRight);

        public float BorderBottom => ResolveFixedHeight(borderBottom);
        public float BorderRight => ResolveFixedWidth(borderRight);

        public float BorderRadiusTopRight => ResolveFixedWidth(borderRadiusTopRight);
        public float BorderRadiusTopLeft => ResolveFixedWidth(borderRadiusTopLeft);
        public float BorderRadiusBottomRight => ResolveFixedWidth(borderRadiusBottomRight);
        public float BorderRadiusBottomLeft => ResolveFixedWidth(borderRadiusBottomLeft);

        public bool IsIgnored => (style.LayoutBehavior & LayoutBehavior.Ignored) != 0;

        public float AnchorLeft => ResolveAnchorLeft();
        public float AnchorRight => ResolveAnchorRight();
        public float AnchorTop => ResolveAnchorTop();
        public float AnchorBottom => ResolveAnchorBottom();

        public float PaddingBorderHorizontal =>
            ResolveFixedWidth(paddingLeft) +
            ResolveFixedWidth(paddingRight) +
            ResolveFixedWidth(borderRight) +
            ResolveFixedWidth(borderLeft);

        public float PaddingBorderVertical =>
            ResolveFixedHeight(paddingTop) +
            ResolveFixedHeight(paddingBottom) +
            ResolveFixedHeight(borderBottom) +
            ResolveFixedHeight(borderTop);

        public Rect ContentRect {
            get {
                float x = PaddingLeft + BorderLeft;
                float y = PaddingTop + BorderTop;
                float width = allocatedWidth - PaddingBorderHorizontal;
                float height = allocatedHeight - PaddingBorderVertical;
                return new Rect(x, y, Mathf.Max(0, width), Mathf.Max(0, height));
            }
        }

        public Vector2 Pivot => new Vector2(
            ResolveFixedWidth(transformPivotX),
            ResolveFixedHeight(transformPivotY)
        );

        public float GetMarginTop(float width) {
            return ResolveMarginVertical(width, marginTop);
        }

        public float GetMarginBottom(float width) {
            return ResolveMarginVertical(width, marginBottom);
        }

        public float GetMarginLeft() {
            return ResolveMarginHorizontal(marginLeft);
        }

        public float GetMarginRight() {
            return ResolveMarginHorizontal(marginRight);
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

        public void UpdateFromStyle() {
            paddingLeft = style.PaddingLeft;
            paddingTop = style.PaddingTop;
            paddingBottom = style.PaddingBottom;
            paddingRight = style.PaddingRight;
            borderLeft = style.BorderLeft;
            borderTop = style.BorderTop;
            borderBottom = style.BorderBottom;
            borderRight = style.BorderRight;
            marginLeft = style.MarginLeft;
            marginTop = style.MarginTop;
            marginBottom = style.MarginBottom;
            marginRight = style.MarginRight;
            borderRadiusTopLeft = style.BorderRadiusTopLeft;
            borderRadiusTopRight = style.BorderRadiusTopRight;
            borderRadiusBottomLeft = style.BorderRadiusBottomLeft;
            borderRadiusBottomRight = style.BorderRadiusBottomRight;
            transformPositionX = style.TransformPositionX;
            transformPositionY = style.TransformPositionY;
            transformBehaviorX = style.TransformBehaviorX;
            transformBehaviorY = style.TransformBehaviorY;
            transformPivotX = style.TransformPivotX;
            transformPivotY = style.TransformPivotY;
            transformScaleX = style.TransformScaleX;
            transformScaleY = style.TransformScaleY;
            prefWidth = style.PreferredWidth;
            minWidth = style.MinWidth;
            maxWidth = style.MaxWidth;
            prefHeight = style.PreferredHeight;
            minHeight = style.MinHeight;
            maxHeight = style.MaxHeight;
            zIndex = style.ZIndex;
        }

        public void ReplaceChild(LayoutBox toReplace, LayoutBox newChild) {
            newChild.parent = this;
            newChild.UpdateChildren();
            newChild.allocatedWidth = toReplace.allocatedWidth;
            newChild.allocatedHeight = toReplace.allocatedHeight;
            UpdateChildren();
        }

        public void UpdateChildren() {
            InvalidatePreferredSizeCache();
            RequestContentSizeChangeLayout();
            OnChildrenChanged();
        }

        public void RequestContentSizeChangeLayout() {
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
                    return style.GetResolvedFontSize() * width.value;

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
                    return style.GetResolvedFontSize() * height.value;

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
                    if (parent.prefWidth.IsContentBased) {
                        return 0f;
                    }

                    return parent.allocatedHeight * margin.value;

                case UIMeasurementUnit.ViewportWidth:
                    return view.Viewport.width * margin.value;

                case UIMeasurementUnit.ViewportHeight:
                    return view.Viewport.height * margin.value;

                case UIMeasurementUnit.ParentContentArea:
                    if (parent.prefHeight.IsContentBased) {
                        return 0f;
                    }

                    return parent.allocatedHeight * margin.value -
                           (parent.style == null ? 0 : parent.PaddingBorderVertical);

                case UIMeasurementUnit.Em:
                    return style.GetResolvedFontSize() * margin.value;

                case UIMeasurementUnit.AnchorWidth:
                    anchorTarget = style.AnchorTarget;
                    if (parent.prefHeight.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(margin.value);

                case UIMeasurementUnit.AnchorHeight:
                    anchorTarget = style.AnchorTarget;
                    if (parent.prefHeight.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorHeight(margin.value);

                case UIMeasurementUnit.Unset:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public virtual void OnStylePropertyChanged(LightList<StyleProperty> property) { }

        public virtual void OnChildStylePropertyChanged(LayoutBox child, LightList<StyleProperty> property) { }

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

        protected abstract float ComputeContentWidth();

        protected abstract float ComputeContentHeight(float width);

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
            UIMeasurement widthMeasurement = prefWidth;
            switch (widthMeasurement.unit) {
                case UIMeasurementUnit.Pixel:
                    return view.ScaleFactor * Mathf.Max(0, widthMeasurement.value);

                case UIMeasurementUnit.Content:
                    return Mathf.Max(0, PaddingBorderHorizontal + (GetContentWidth() * widthMeasurement.value));

                case UIMeasurementUnit.ParentSize:
                    if (parent == null) return view.Viewport.width;
                    if (parent.prefWidth.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedWidth * widthMeasurement.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, view.Viewport.width * widthMeasurement.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, view.Viewport.height * widthMeasurement.value);

                case UIMeasurementUnit.ParentContentArea:
                    if (parent == null) return view.Viewport.width;
                    if (parent.prefWidth.IsContentBased) {
                        // todo there are cases where this is not true
                        // if we hit the paradox -> size = own content size
                        // ie parent is layout that can grow and parent is growing
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedWidth - parent.PaddingBorderHorizontal) * widthMeasurement.value;

                case UIMeasurementUnit.Em:
                    return Math.Max(0, style.GetResolvedFontSize() * widthMeasurement.value);


                case UIMeasurementUnit.AnchorWidth:
                    anchorTarget = style.AnchorTarget;
                    if (parent == null) return view.Viewport.width;
                    if (parent.prefWidth.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(widthMeasurement.value);

                case UIMeasurementUnit.AnchorHeight:
                    anchorTarget = style.AnchorTarget;
                    if (parent == null) return view.Viewport.height;
                    if (parent.prefWidth.IsContentBased && anchorTarget == AnchorTarget.Parent ||
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
                    return style.GetResolvedFontSize() * anchor.value;

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
                    return parent.actualWidth - ResolveAnchorValue(parent.actualWidth, anchor);

                case AnchorTarget.ParentContentArea:
                    if (parent == null) {
                        return view.Viewport.x + ResolveAnchorValue(view.Viewport.width, anchor);
                    }

                    LayoutResult parentResult = parent.element.layoutResult;
                    float offset = parentResult.padding.right + parentResult.border.right + parentResult.border.left + parentResult.padding.left;
                    return parent.actualWidth - (ResolveAnchorValue(parent.actualWidth - offset, anchor) + parentResult.padding.right + parentResult.border.right);

                case AnchorTarget.Viewport:
                    return view.Viewport.x +
                           view.Viewport.width - ResolveAnchorValue(view.Viewport.width, anchor);

                case AnchorTarget.Screen:
                    float parentX = parent?.element.layoutResult.screenPosition.x ?? 0;
                    return Screen.width - parentX - ResolveAnchorValue(Screen.width, anchor);

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
                        return view.Viewport.xMin + ResolveAnchorValue(view.Viewport.width, anchor);
                    }

                    return ResolveAnchorValue(parent.actualWidth, anchor);

                case AnchorTarget.ParentContentArea:
                    if (parent == null) {
                        return view.Viewport.xMax + ResolveAnchorValue(view.Viewport.width, anchor);
                    }

                    LayoutResult parentResult = parent.element.layoutResult;
                    float offset = parentResult.padding.left + parentResult.border.left + parentResult.padding.right + parentResult.border.right;
                    return ResolveAnchorValue(parent.actualWidth - offset, anchor) + parentResult.padding.left + parentResult.border.left;

                case AnchorTarget.Viewport:
                    return view.Viewport.xMax + ResolveAnchorValue(view.Viewport.width, anchor);

                case AnchorTarget.Screen:
                    float parentX = parent?.element.layoutResult.screenPosition.x ?? 0;
                    return ResolveAnchorValue(Screen.width, anchor) - parentX;

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

                    return ResolveAnchorValue(parent.actualHeight, anchor);

                case AnchorTarget.ParentContentArea:
                    if (parent == null) {
                        return view.Viewport.y + ResolveAnchorValue(view.Viewport.height, anchor);
                    }

                    LayoutResult parentResult = parent.element.layoutResult;
                    float offset = parentResult.padding.top + parentResult.border.top + parentResult.border.bottom + parentResult.padding.bottom;
                    return ResolveAnchorValue(parent.actualHeight - offset, anchor) + parentResult.padding.top + parentResult.border.top;

                case AnchorTarget.Viewport:
                    return view.Viewport.y + ResolveAnchorValue(view.Viewport.height, anchor);

                case AnchorTarget.Screen:
                    float parentY = parent?.element.layoutResult.screenPosition.y ?? 0;
                    return ResolveAnchorValue(Screen.height, anchor) - parentY;

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
                        return view.Viewport.yMax + ResolveAnchorValue(view.Viewport.height, anchor);
                    }

                    return parent.actualHeight - ResolveAnchorValue(parent.actualHeight, anchor);

                case AnchorTarget.ParentContentArea:
                    if (parent == null) {
                        return view.Viewport.yMax + ResolveAnchorValue(view.Viewport.height, anchor);
                    }

                    LayoutResult parentResult = parent.element.layoutResult;
                    float offset = parentResult.padding.top + parentResult.border.top + parentResult.border.bottom + parentResult.padding.bottom;
                    return parent.actualHeight - (ResolveAnchorValue(parent.actualHeight - offset, anchor) + parentResult.border.bottom + parentResult.padding.bottom);

                case AnchorTarget.Viewport:
                    return view.Viewport.yMax + ResolveAnchorValue(view.Viewport.height, anchor);

                case AnchorTarget.Screen:
                    float parentY = parent?.element.layoutResult.screenPosition.y ?? 0;
                    return Screen.height - parentY - ResolveAnchorValue(Screen.height, anchor);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float GetPreferredHeight(float contentWidth) {
            AnchorTarget anchorTarget;
            UIMeasurement height = prefHeight;
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
                    if (parent.prefHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedHeight * height.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, view.Viewport.width * height.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, view.Viewport.height * height.value);

                case UIMeasurementUnit.ParentContentArea:
                    if (parent == null) return view.Viewport.height;
                    if (parent.prefHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0,
                        parent.allocatedHeight * height.value -
                        (parent.style == null ? 0 : parent.PaddingBorderVertical));

                case UIMeasurementUnit.Em:
                    return Mathf.Max(0, style.GetResolvedFontSize() * height.value);

                case UIMeasurementUnit.AnchorWidth:
                    anchorTarget = style.AnchorTarget;
                    if (parent == null) return view.Viewport.width;
                    if (parent.prefHeight.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(height.value);

                case UIMeasurementUnit.AnchorHeight:
                    anchorTarget = style.AnchorTarget;
                    if (parent == null) return view.Viewport.height;
                    if (parent.prefHeight.IsContentBased && anchorTarget == AnchorTarget.Parent ||
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
                    return style.GetResolvedFontSize() * margin.value;

                case UIMeasurementUnit.Content:
                    return GetContentWidth() * margin.value;

                case UIMeasurementUnit.ParentSize:
                    if (parent.prefWidth.IsContentBased) {
                        return 0f;
                    }

                    return parent.allocatedHeight * margin.value;

                case UIMeasurementUnit.ViewportWidth:
                    return view.Viewport.width * margin.value;

                case UIMeasurementUnit.ViewportHeight:
                    return view.Viewport.height * margin.value;

                case UIMeasurementUnit.ParentContentArea:
                    if (parent.prefWidth.IsContentBased) {
                        return 0f;
                    }

                    return parent.allocatedWidth * margin.value -
                           (parent.style == null ? 0 : parent.PaddingBorderHorizontal);


                case UIMeasurementUnit.AnchorWidth:
                    anchorTarget = style.AnchorTarget;
                    if (parent.prefWidth.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(margin.value);

                case UIMeasurementUnit.AnchorHeight:
                    anchorTarget = style.AnchorTarget;
                    if (parent.prefWidth.IsContentBased && anchorTarget == AnchorTarget.Parent ||
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
                    return style.GetResolvedFontSize() * transformOffset.value;

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
                    if (parent.prefWidth.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(transformOffset.value);
                }

                case TransformUnit.AnchorHeight: {
                    AnchorTarget anchorTarget = style.AnchorTarget;
                    if (parent.prefHeight.IsContentBased && anchorTarget == AnchorTarget.Parent ||
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

     //   [DebuggerStepThrough]
        protected float ResolveMinOrMaxWidth(UIMeasurement widthMeasurement) {
            AnchorTarget anchorTarget;
            switch (widthMeasurement.unit) {
                case UIMeasurementUnit.Pixel:
                    return Mathf.Max(0, widthMeasurement.value);

                case UIMeasurementUnit.Content:
                    return Mathf.Max(0, PaddingBorderHorizontal + (GetContentWidth() * widthMeasurement.value));

                case UIMeasurementUnit.ParentSize:
                    if (parent.prefWidth.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedWidth * widthMeasurement.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, view.Viewport.width * widthMeasurement.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, view.Viewport.height * widthMeasurement.value);

                case UIMeasurementUnit.ParentContentArea:
                    if (parent.prefWidth.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0,
                        parent.allocatedWidth * widthMeasurement.value - (parent.style == null
                            ? 0
                            : parent.PaddingHorizontal - parent.BorderHorizontal));

                case UIMeasurementUnit.Em:
                    return Math.Max(0, style.GetResolvedFontSize() * widthMeasurement.value);

                case UIMeasurementUnit.AnchorWidth:
                    anchorTarget = style.AnchorTarget;
                    if (parent.prefWidth.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(widthMeasurement.value);

                case UIMeasurementUnit.AnchorHeight:
                    anchorTarget = style.AnchorTarget;
                    if (parent.prefWidth.IsContentBased && anchorTarget == AnchorTarget.Parent ||
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
                    if (parent.prefHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, parent.allocatedHeight * heightMeasurement.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, view.Viewport.width * heightMeasurement.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, view.Viewport.height * heightMeasurement.value);

                case UIMeasurementUnit.ParentContentArea:
                    if (parent.prefHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0,
                        parent.allocatedHeight * heightMeasurement.value - (parent.style == null
                            ? 0
                            : parent.PaddingVertical - parent.BorderVertical));

                case UIMeasurementUnit.Em:
                    return Mathf.Max(0, style.GetResolvedFontSize() * heightMeasurement.value);

                case UIMeasurementUnit.AnchorWidth:
                    anchorTarget = style.AnchorTarget;
                    if (parent.prefHeight.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorWidth(heightMeasurement.value);

                case UIMeasurementUnit.AnchorHeight:
                    anchorTarget = style.AnchorTarget;
                    if (parent.prefHeight.IsContentBased && anchorTarget == AnchorTarget.Parent ||
                        anchorTarget == AnchorTarget.ParentContentArea) {
                        return 0f;
                    }

                    return ResolveAnchorHeight(heightMeasurement.value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public LayoutBoxSize GetHeights(float width) {
            float prf = GetPreferredHeight(width);
            float min = ResolveMinOrMaxHeight(minHeight, width);
            float max = ResolveMinOrMaxHeight(maxHeight, width);
            return new LayoutBoxSize(min, max, prf);
        }

        public LayoutBoxSize GetWidths() {
            float prf = GetPreferredWidth();
            float min = ResolveMinOrMaxWidth(minWidth);
            float max = ResolveMinOrMaxWidth(maxWidth);

            return new LayoutBoxSize(min, max, prf);
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

        protected abstract void OnChildrenChanged();

        public virtual void OnSpawn(UIElement element) {
            this.element = element;
            this.style = element.style;
            this.children = children ?? new List<LayoutBox>(4);
            this.cachedPreferredWidth = -1;
            this.view = element.View;
            this.markedForLayout = true;
            UpdateFromStyle();
        }

        public virtual void OnRelease() {
            this.element = null;
            this.style = null;
            this.children.Clear();
            this.cachedPreferredWidth = -1;
            this.view = null;
            this.markedForLayout = true;
        }

        internal void Release() {
            pool?.Release(this);
        }

    }

}