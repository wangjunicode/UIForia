using System;
using System.Collections.Generic;
using Rendering;
using Src.Systems;
using Src.Util;

namespace Src.Layout.LayoutTypes {

    public abstract class LayoutBox {

        public float preferredWidth;
        public float preferredHeight;
        public float minWidth;
        public float minHeight;
        public float maxWidth;
        public float maxHeight;
        public float computedX;
        public float computedY;
        public float allocatedWidth;
        public float allocatedHeight;
        public float actualWidth;
        public float actualHeight;

        public UIStyle style;
        public UIElement element;

        public LayoutBox parent;
        
        private LayoutSystem2 layoutSystem;
        
        public List<LayoutBox> children;

        protected LayoutBox(LayoutSystem2 layoutSystem, UIElement element) {
            this.element = element;
            this.layoutSystem = layoutSystem;
            this.style = element?.style?.computedStyle;
            this.children = ListPool<LayoutBox>.Get();
        }

        public abstract void RunLayout();

        public virtual void OnContentRectChanged() { }

        public void OnFontSizeChanged(int fontSize) {
            // todo -- Em != font Size, rather size of 'M' in given font at given size 
            if (style.dimensions.width.unit == UIUnit.Em) {
                float newPrefWidth = fontSize * style.dimensions.width.value;
                if (newPrefWidth != preferredWidth) {
                    preferredWidth = newPrefWidth;
                    parent.OnChildPreferredWidthChanged(this);
                }
            }

            if (style.layoutConstraints.minWidth.unit == UIUnit.Em) {
                float newMinWidth = fontSize * style.layoutConstraints.minWidth.value;
                if (newMinWidth != preferredWidth) {
                    preferredWidth = newMinWidth;
                    parent.OnChildMinWidthChanged(this);
                }
            }

            if (style.layoutConstraints.maxWidth.unit == UIUnit.Em) {
                float newMaxWidth = fontSize * style.layoutConstraints.maxWidth.value;
                if (newMaxWidth != preferredWidth) {
                    preferredWidth = newMaxWidth;
                    parent.OnChildMaxWidthChanged(this);
                }
            }
        }

        protected float ResolveFixedWidth(UIMeasurement measurement) {
            switch (measurement.unit) {
                case UIUnit.Pixel:
                    return measurement.value;
                case UIUnit.ParentSize:
                    return measurement.value * parent.allocatedWidth;
                case UIUnit.View:
                    return measurement.value * layoutSystem.ViewportRect.width;
                case UIUnit.ParentContentArea:
                    // todo subtract parent content area
                    return (parent.allocatedWidth) * measurement.value;
                case UIUnit.Em:
                    return measurement.value * style.textStyle.fontSize;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected float ResolveFixedHeight(UIMeasurement measurement) {
            switch (measurement.unit) {
                case UIUnit.Pixel:
                    return measurement.value;
                case UIUnit.ParentSize:
                    return measurement.value * parent.allocatedHeight;
                case UIUnit.View:
                    return measurement.value * layoutSystem.ViewportRect.height;
                case UIUnit.ParentContentArea:
                    // todo subtract parent content area
                    return (parent.allocatedHeight) * measurement.value;
                case UIUnit.Em:
                    return measurement.value * style.textStyle.fontSize;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual void SetParent(LayoutBox parent) {
            this.parent = parent;
        }

       

        public virtual void OnChildMinWidthChanged(LayoutBox child) { }
        public virtual void OnChildMaxWidthChanged(LayoutBox child) { }
        public virtual void OnChildPreferredWidthChanged(LayoutBox child) { }

        public virtual void OnChildMinHeightChanged(LayoutBox child) { }
        public virtual void OnChildMaxHeightChanged(LayoutBox child) { }
        public virtual void OnChildPreferredHeightChanged(LayoutBox child) { }

        protected virtual float GetMinRequiredHeightForWidth(float width) {
            return 0f;
        }

        public virtual void OnDirectionChanged(LayoutDirection direction) { }

        public virtual void OnMainAxisAlignmentChanged(MainAxisAlignment newAlignment, MainAxisAlignment oldAlignment) { }

        public virtual void OnCrossAxisAlignmentChanged(CrossAxisAlignment newAlignment, CrossAxisAlignment oldAlignment) { }

        public virtual void OnWrapChanged(LayoutWrap newWrap, LayoutWrap oldWrap) { }

        public virtual void OnFlowStateChanged(LayoutFlowType flowType, LayoutFlowType oldFlow) { }

        public virtual void ReplaceChild(LayoutBox toReplace, LayoutBox newChild) {
            int index = children.IndexOf(toReplace);
            if (index == -1) {
                throw new Exception("Cannot replace child");
            }
            newChild.SetParent(this);
            children[index] = newChild;
            newChild.AdoptChildren(toReplace);
            layoutSystem.RequestLayout(this);
        }

        public virtual void AddChild(LayoutBox child) {
            children.Add(child);
            child.SetParent(this);
            layoutSystem.RequestLayout(this);
        }
        
        public virtual void RemoveChild(LayoutBox child) {
            if (!children.Remove(child)) {
                return;
            }
            child.SetParent(null);
            layoutSystem.RequestLayout(this);
        }

        protected virtual void AdoptChildren(LayoutBox box) {
            for (int i = 0; i < box.children.Count; i++) {
                children.Add(box.children[i]);
            }

            RequestLayout();
        }

        protected void RequestLayout() {
            layoutSystem.RequestLayout(this);
        }
    }

}