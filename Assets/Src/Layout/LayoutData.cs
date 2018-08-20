using System;
using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src.Layout {

    public class LayoutData : ISkipTreeTraversable {

        public UIMeasurement staticX;
        public UIMeasurement staticY;

        public UIMeasurement preferredWidth;
        public UIMeasurement preferredHeight;

        public UIMeasurement minWidth;
        public UIMeasurement maxWidth;

        public UIMeasurement minHeight;
        public UIMeasurement maxHeight;

        // can compress into single int
        public int growthFactor;
        public int shrinkFactor;

        // can compress with flags
        public MainAxisAlignment mainAxisAlignment;
        public CrossAxisAlignment crossAxisAlignment;

        public int order; // can maybe compress

        public bool isInFlow; // can compress with flags

        public ContentBoxRect margin;
        public ContentBoxRect border;
        public ContentBoxRect padding;

        public LayoutType layoutType; // can compress with flags
        public LayoutData parent;
        public LayoutWrap wrapMode; // can compress with flags

        public readonly UIElement element; // can be an id / offset

        public readonly List<LayoutData> children; // can be computed
        public LayoutDirection layoutDirection; // can be compressed

        public LayoutData(UIElement element) {
            this.children = new List<LayoutData>();
            this.element = element;
            this.isInFlow = true;
        }

        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

        public UILayout layout {
            get {
                switch (layoutType) {
                    case LayoutType.Flex: return UILayout.Flex;
                    case LayoutType.Flow: return UILayout.Flow;
                    default: throw new NotImplementedException();
                }
            }
        }

        public float ContentStartOffsetX => margin.left + padding.left + border.left;
        public float ContentEndOffsetX => margin.right + padding.right + border.right;

        public float ContentStartOffsetY => margin.top + padding.top + border.top;
        public float ContentEndOffsetY => margin.bottom + padding.bottom + border.bottom;

        public float GetPreferredWidth(UIUnit parentUnit, float parentValue, float viewportValue) {
            switch (preferredWidth.unit) {
                case UIUnit.Pixel:
                    return preferredWidth.value;

                case UIUnit.Content:
                    return layout.GetContentWidth(this, parentValue, viewportValue);

                case UIUnit.Parent:
                    if (parentUnit == UIUnit.Content) return 0;
                    return preferredWidth.value * parentValue;

                case UIUnit.View:
                    return preferredWidth.value * viewportValue;

                default:
                    return 0;
            }
        }

        public float GetPreferredHeight(UIUnit parentUnit, float parentValue, float viewportValue) {
            switch (preferredHeight.unit) {
                case UIUnit.Pixel:
                    return preferredHeight.value;

                case UIUnit.Content:
                    return layout.GetContentHeight(this, parentValue, viewportValue);

                case UIUnit.Parent:
                    if (parentUnit == UIUnit.Content) return 0;
                    return preferredHeight.value * parentValue;

                case UIUnit.View:
                    return preferredHeight.value * viewportValue;

                default:
                    return 0;
            }
        }

        public void OnParentChanged(ISkipTreeTraversable newParent) {
            parent = (LayoutData) newParent;
        }

        void ISkipTreeTraversable.OnBeforeTraverse() {
            children.Clear();
        }

        void ISkipTreeTraversable.OnAfterTraverse() {
            parent?.children.Add(this);
        }

    }

}