using System;
using System.Collections.Generic;
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

        protected Size preferredContentSize;

        public VirtualScrollbar horizontalScrollbar;
        public VirtualScrollbar verticalScrollbar;

        private LayoutSystem2 layoutSystem;
        public bool markedForLayout;

        protected LayoutBox(LayoutSystem2 layoutSystem, UIElement element) {
            this.element = element;
            this.layoutSystem = layoutSystem;
            this.style = element?.style?.computedStyle;
            this.children = ListPool<LayoutBox>.Get();
            this.preferredContentSize = Size.Unset;
        }

        public abstract void RunLayout();
        protected abstract Size RunContentSizeLayout();

        public virtual float MinWidth => Mathf.Max(PaddingHorizontal + BorderHorizontal, ResolveWidth(style.MinWidth));
        public virtual float MaxWidth => Mathf.Max(PaddingHorizontal + BorderHorizontal, ResolveWidth(style.MaxWidth));
        public virtual float PreferredWidth => ResolveWidth(style.PreferredWidth);

        public virtual float MinHeight => Mathf.Max(PaddingVertical + BorderVertical, ResolveHeight(style.MinHeight));
        public virtual float MaxHeight => Mathf.Max(PaddingVertical + BorderVertical, ResolveHeight(style.MaxHeight));
        public virtual float PreferredHeight => ResolveHeight(style.PreferredHeight);

        // todo -- move to fixed units
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

        public virtual void SetParent(LayoutBox parent) {
            this.parent?.OnChildRemoved(this);
            this.parent = parent;
            this.parent?.OnChildAdded(this);
        }

        // need layout when
        /*
         * - Child Add / Remove / Move / Enable / Disable
         * - Allocated size changes && we give a shit
         * - Parent Allocated size changes & we give a shit -> handled automatically
         * - Child size changes from style
         * - Child constraint changes && affects output size
         * - Layout property changes
         */

        public virtual void ReplaceChild(LayoutBox toReplace, LayoutBox newChild) {
            int index = children.IndexOf(toReplace);
            if (index == -1) {
                throw new Exception("Cannot replace child");
            }

            newChild.SetParent(this);
            children[index] = newChild;
            newChild.AdoptChildren(toReplace);
        }

        public virtual void OnChildAdded(LayoutBox child) {
            children.Add(child);
            if (child.element.isEnabled) {
                RequestParentLayoutIfContentBased();
                RequestLayout();
            }
        }

        public virtual void OnChildRemoved(LayoutBox child) {
            if (!children.Remove(child)) {
                return;
            }

            if (child.element.isEnabled) {
                RequestParentLayoutIfContentBased();
                RequestLayout();
            }
        }

        protected virtual void AdoptChildren(LayoutBox box) {
            for (int i = 0; i < box.children.Count; i++) {
                children.Add(box.children[i]);
            }

            RequestLayout();
            RequestParentLayoutIfContentBased();
        }

        protected void RequestLayout() {
            if (markedForLayout) return;
            markedForLayout = true;
            layoutSystem.RequestLayout(this);
        }

        public virtual void OnChildSizeChanged() {
            RequestParentLayoutIfContentBased();
            RequestLayout();
        }

        protected bool IsContentSized {
            get {
                UIUnit units = style.PreferredWidth.unit
                               | style.PreferredHeight.unit
                               | style.MinWidth.unit
                               | style.MaxWidth.unit
                               | style.MinHeight.unit
                               | style.MaxHeight.unit;
                return (units & UIUnit.Content) != 0;
            }
        }

        public void OnSizeConstraintChanged() {
            UIUnit units = style.MinWidth.unit | style.MaxWidth.unit | style.MinHeight.unit | style.MaxHeight.unit;
            if ((units & UIUnit.Content) != 0) {
                preferredContentSize = Size.Unset;
            }

            parent.RequestLayout();
        }

        public virtual void SetAllocatedRect(float x, float y, float width, float height) {
            if (localX != x || localY != y) {
                localX = x;
                localY = y;
                layoutSystem.PositionChanged(this);
            }

            if (allocatedWidth != width || allocatedHeight != height) {
                allocatedWidth = width;
                allocatedHeight = height;
                layoutSystem.OnRectChanged(this);
                // todo -- right now this calls layout for all descendants which can probably be avoided if no children are parent sized
                RequestLayout();
            }
        }

        public void OnChildEnabled(LayoutBox child) {
            RequestParentLayoutIfContentBased();
            RequestLayout();
        }

        public void OnChildDisabled(LayoutBox child) {
            RequestParentLayoutIfContentBased();
            RequestLayout();
        }

        protected void RequestParentLayoutIfContentBased() {
            if (IsContentSized) {
                preferredContentSize = Size.Unset;
                LayoutBox ptr = parent;
                while (ptr != null && ptr.IsContentSized) {
                    ptr.preferredContentSize = Size.Unset;
                    ptr.parent?.RequestLayout();
                    ptr = ptr.parent;
                }
            }
        }

        protected float GetContentPreferredWidth() {
            if (!preferredContentSize.IsDefined()) {
                preferredContentSize = RunContentSizeLayout();
            }

            return preferredContentSize.width;
        }

        protected float GetContentPreferredHeight() {
            if (!preferredContentSize.IsDefined()) {
                preferredContentSize = RunContentSizeLayout();
            }

            return preferredContentSize.height;
        }

        protected float ResolveFixedWidth(UIFixedLength width) {
            switch (width.unit) {
                case UIFixedUnit.Pixel:
                    return width.value;
                case UIFixedUnit.Percent:
                    return allocatedWidth * width.value;
                default:
                    return 0;
            }
        }

        protected float ResolveFixedHeight(UIFixedLength height) {
            switch (height.unit) {
                case UIFixedUnit.Pixel:
                    return height.value;
                case UIFixedUnit.Percent:
                    return allocatedHeight * height.value;
                default:
                    return 0;
            }
        }

        protected virtual float ResolveWidth(UIMeasurement width) {
            switch (width.unit) {
                case UIUnit.Pixel:
                    return width.value;
                case UIUnit.Content:
                    return PaddingHorizontal + BorderHorizontal + (GetContentPreferredWidth() * width.value);
                case UIUnit.ParentSize:
                    return parent.allocatedWidth * width.value;
                case UIUnit.View:
                    return layoutSystem.ViewportRect.width * width.value;
                case UIUnit.ParentContentArea:
                    return parent.allocatedWidth * width.value - (parent.style == null ? 0 : parent.PaddingHorizontal - parent.BorderHorizontal);
                case UIUnit.Em:
                    return 0;
                case UIUnit.MinContent:
                    return 0;
                case UIUnit.MaxContent:
                    return 0;
                case UIUnit.FitContent:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual float ResolveHeight(UIMeasurement height) {
            switch (height.unit) {
                case UIUnit.Pixel:
                    return height.value;
                case UIUnit.Content:
                    return PaddingVertical + BorderVertical + (GetContentPreferredHeight() * height.value);
                case UIUnit.ParentSize:
                    return parent.allocatedHeight * height.value;
                case UIUnit.View:
                    return layoutSystem.ViewportRect.height * height.value;
                case UIUnit.ParentContentArea:
                    return parent.allocatedHeight * height.value - (parent.style == null ? 0 : parent.PaddingVertical - parent.BorderVertical);
                case UIUnit.Em:
                    return 0;
                case UIUnit.MinContent:
                    return 0;
                case UIUnit.MaxContent:
                    return 0;
                case UIUnit.FitContent:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual void OnStylePropertyChanged(StyleProperty property) { }

        public virtual void OnChildStylePropertyChanged(LayoutBox child, StyleProperty property) { }

    }

}