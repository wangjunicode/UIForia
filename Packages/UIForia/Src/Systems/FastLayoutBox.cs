using System;
using System.Diagnostics;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public enum AxisEdge {

        Min,
        Max

    }

    public struct Alignment {

        public float value;
        public bool isIn;
        public AxisEdge edge;
        public TransformUnit unit;

    }

    public enum Fit {

        Unset,
        None,
        Grow,
        Shrink,
        Fit

    }
    

    public abstract class FastLayoutBox {

        public LayoutRenderFlag flags;
        public FastLayoutBox relayoutBoundary;

        public FastLayoutBox parent;
        public FastLayoutBox firstChild;
        public FastLayoutBox nextSibling;
        public FastLayoutBox prevSibling;

        // optimized to use bits for units & holds resolved value 
        public OffsetBox paddingBox;
        public OffsetBox borderBox;
        public OffsetBox marginBox;

        // holds units in bit field
        public MeasurementSet widthMeasurement;
        public MeasurementSet heightMeasurement;

        public LayoutBehavior layoutBehavior;
        public ContainingBlock constraints;
        public FastLayoutSystem layoutSystem;

        public int traversalIndex;

        public bool sizedByParent;

        internal bool isInPool;

        public int depth;

        public Fit selfFitHorizontal;
        public Fit selfFitVertical;

        public Alignment selfAlignmentHorizontal;
        public Alignment selfAlignmentVertical;

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

        public Rect containingBox;
        public Rect localRect;

        public Size size;
        public Vector2 position;
        public int enabledFrame;

        public void ApplyLayout(in Size boxSize, in Rect rect, in Alignment horizontal, in Alignment vertical, Fit horizontalFit, Fit verticalFit) {
            // if our layout doesn't care about what size our parent asked us to be, don't layout

            if (containingBox != rect) {
                containingBox = rect;

                size = ApplyFitToSize(size, rect, horizontalFit, verticalFit);

                position = ApplyAlignment(rect, horizontal, vertical);

                // actual size comes from running layout
                // if we grow / shrink or fit after getting our allocation, we probably need to re-layout

                // layout needs to figure out our actual size, then apply aspect ratio, fit, then align, then do matrix math?
            }
        }

        private Vector2 ApplyAlignment(in Rect rect, Alignment horizontal, Alignment vertical) {
            Vector2 retn = default;

            switch (horizontal.unit) {
                case TransformUnit.Pixel:
                    if (horizontal.edge == AxisEdge.Min) {
                        retn.x = rect.x + horizontal.value;
                    }
                    else {
                        retn.x = rect.x + rect.width + horizontal.value;
                    }

                    break;

                case TransformUnit.ContentWidth:
                    break;

                case TransformUnit.ContentHeight:
                    break;
            }

            return retn;
        }

        private Size ApplyFitToSize(in Size size1, in Rect rect, Fit horizontalFit, Fit verticalFit) {
            horizontalFit = selfFitHorizontal == Fit.Unset ? horizontalFit : selfFitHorizontal;
            verticalFit = selfFitVertical == Fit.Unset ? verticalFit : selfFitVertical;

            switch (horizontalFit) {
                case Fit.Unset:
                case Fit.None:
                    break;

                case Fit.Grow:
                    if (size.width < rect.width) {
                        size.width = rect.width;
                    }

                    break;

                case Fit.Shrink:
                    if (size.width > rect.width) {
                        size.width = rect.width;
                    }

                    break;

                case Fit.Fit:
                    size.width = rect.width;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(horizontalFit), horizontalFit, null);
            }

            switch (verticalFit) {
                case Fit.Unset:
                case Fit.None:
                    break;

                case Fit.Grow:
                    if (size.height < rect.height) {
                        size.height = rect.height;
                    }

                    break;

                case Fit.Shrink:
                    if (size.height > rect.height) {
                        size.height = rect.height;
                    }

                    break;

                case Fit.Fit:
                    size.height = rect.height;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(verticalFit), verticalFit, null);
            }


            return size;
        }

        public float ResolveWidth(in ContainingBlock containingBlock, float value, UIMeasurementUnit unit) {
            switch (unit) {
                case UIMeasurementUnit.Content:
                    float minWidth = GetIntrinsicMinWidth();
                    float pref = GetIntrinsicMaxWidth();
                    return Mathf.Max(minWidth, Mathf.Min(containingBlock.width, pref));

                case UIMeasurementUnit.Pixel:
                    return value;

                case UIMeasurementUnit.Em:
                    return 0;

                case UIMeasurementUnit.Percentage:
                    // percentage of last resolved size (ie containing block)
                    return containingBlock.width * value;

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

        public float ResolveHeight(in ContainingBlock containingBlock, float value, UIMeasurementUnit unit) {
            switch (unit) {
                case UIMeasurementUnit.Content:
                    float minHeight = GetIntrinsicMinHeight();
                    float pref = GetIntrinsicMaxHeight();
                    return Mathf.Max(minHeight, Mathf.Min(containingBlock.height, pref));

                case UIMeasurementUnit.Pixel:
                    return value;

                case UIMeasurementUnit.Em:
                    return 0;

                case UIMeasurementUnit.Percentage:
                    // percentage of last resolved size (ie containing block)
                    return containingBlock.height * value;

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

        public Size GetSize(in ContainingBlock containingBlock) {
            float minWidth = ResolveWidth(containingBlock, widthMeasurement.minValue, widthMeasurement.minUnit);

            float maxWidth = ResolveWidth(containingBlock, widthMeasurement.maxValue, widthMeasurement.maxUnit);

            float prefWidth = ResolveWidth(containingBlock, widthMeasurement.prefValue, widthMeasurement.prefUnit);

            float minHeight = ResolveHeight(containingBlock, heightMeasurement.minValue, heightMeasurement.minUnit);

            float maxHeight = ResolveHeight(containingBlock, heightMeasurement.maxValue, heightMeasurement.maxUnit);

            float prefHeight = ResolveHeight(containingBlock, heightMeasurement.prefValue, heightMeasurement.prefUnit);

            // return preferred & clamp again later?

            return new Size();
        }

        public void Layout(ContainingBlock containingBlock, bool parentUsesSize = false) {
            FastLayoutBox relayoutBoundary;

            // also account for fixed size on both dimensions
            if (!parentUsesSize || sizedByParent) {
                relayoutBoundary = this;
            }
            else {
                relayoutBoundary = parent.relayoutBoundary;
            }

            if ((flags & LayoutRenderFlag.NeedsLayout) != 0 && this.constraints.Equals(containingBlock) && this.relayoutBoundary == relayoutBoundary) {
                return;
            }

            this.constraints = containingBlock;
            this.relayoutBoundary = relayoutBoundary;

            if (sizedByParent) {
                PerformResize();
            }

            PerformLayout();

            flags &= ~(LayoutRenderFlag.NeedsLayout);

            MarkNeedsPaint();
        }

        public void PerformResize() { }

        public abstract void PerformLayout();

        public void MarkNeedsPaint() { }

        public void MarkNeedsLayout() {
            if ((flags & LayoutRenderFlag.NeedsLayout) != 0) {
                return;
            }

            if (relayoutBoundary != this) {
                parent.MarkNeedsLayout();
            }
            else {
                flags |= LayoutRenderFlag.NeedsLayout;
                layoutSystem.nodesNeedingLayout.Add(this); // add to root list of nodes needing layout
                // renderSystem.RequireRender();
            }
        }

        public virtual void OnStyleChanged() { }

        public virtual void SetChildren(LightList<FastLayoutBox> container) {
            if (container.size == 0) {
                firstChild = null;
                return;
            }

            firstChild = container[0];
            for (int i = 0; i < container.size; i++) {
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
    }

}