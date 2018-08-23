using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rendering;
using Src.Systems;
using UnityEngine;

namespace Src.Layout {

    [DebuggerDisplay("{element}")]
    public class LayoutData : ISkipTreeTraversable {

        public LayoutRect rect;

        public LayoutParameters parameters;
        public LayoutConstraints constraints;

        public ContentBoxRect margin;
        public ContentBoxRect border;
        public ContentBoxRect padding;

        public Vector2 textContentSize;
        
        public LayoutData parent;
        public readonly UIElement element;
        public readonly List<LayoutData> children;

        public LayoutData(UIElement element) {
            this.element = element;
            this.children = new List<LayoutData>();

            constraints = LayoutConstraints.Unset;

            rect = new LayoutRect(0, 0, UIMeasurement.Parent100, UIMeasurement.Parent100);
        }

        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

        public UILayout layout {
            get {
                switch (parameters.type) {
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

        public bool isInFlow => parameters.flow != LayoutFlowType.OutOfFlow;

        public float GetMinWidth(UIUnit parentUnit, float parentValue, float viewportValue) {
            switch (constraints.minWidth.unit) {
                case UIUnit.Pixel:
                    return constraints.minWidth.value;

                case UIUnit.Content:
                    throw new NotImplementedException();
                    return layout.GetContentWidth(this, parentValue, viewportValue);

                case UIUnit.Parent:
                    throw new NotImplementedException();
                    if (parentUnit == UIUnit.Content) return 0;
                    return constraints.minWidth.value * parentValue;

                case UIUnit.View:
                    return constraints.minWidth.value * viewportValue;

                default:
                    return 0;
            }
        }

        public float GetMaxWidth(UIUnit parentUnit, float parentValue, float viewportValue) {
            switch (constraints.maxWidth.unit) {
                case UIUnit.Pixel:
                    return constraints.maxWidth.value;

                case UIUnit.Content:
                    throw new NotImplementedException();
                    return layout.GetContentWidth(this, parentValue, viewportValue);

                case UIUnit.Parent:
                    throw new NotImplementedException();
                    if (parentUnit == UIUnit.Content) return 0;
                    return constraints.maxWidth.value * parentValue;

                case UIUnit.View:
                    return constraints.maxWidth.value * viewportValue;

                default:
                    return 0;
            }
        }

        public float GetMinHeight(UIUnit parentUnit, float parentValue, float viewportValue) {
            switch (constraints.minHeight.unit) {
                case UIUnit.Pixel:
                    return constraints.minHeight.value;

                case UIUnit.Content:
                    throw new NotImplementedException();
                    return layout.GetContentWidth(this, parentValue, viewportValue);

                case UIUnit.Parent:
                    throw new NotImplementedException();
                    if (parentUnit == UIUnit.Content) return 0;
                    return constraints.minHeight.value * parentValue;

                case UIUnit.View:
                    return constraints.minHeight.value * viewportValue;

                default:
                    return 0;
            }
        }

        public float GetMaxHeight(UIUnit parentUnit, float parentValue, float viewportValue) {
            switch (constraints.maxHeight.unit) {
                case UIUnit.Pixel:
                    return constraints.maxHeight.value;

                case UIUnit.Content:
                    throw new NotImplementedException();
                    return layout.GetContentWidth(this, parentValue, viewportValue);

                case UIUnit.Parent:
                    throw new NotImplementedException();
                    if (parentUnit == UIUnit.Content) return 0;
                    return constraints.maxHeight.value * parentValue;

                case UIUnit.View:
                    return constraints.maxHeight.value * viewportValue;

                default:
                    return 0;
            }
        }

        public float GetPreferredWidth(UIUnit parentUnit, float parentValue, float viewportValue) {
            float baseWidth = 0;
            switch (rect.width.unit) {
                case UIUnit.Pixel:
                    baseWidth = rect.width.value;
                    break;
                case UIUnit.Content:
                    if ((element.flags & UIElementFlags.TextElement) != 0) {
                        
                    }
                    baseWidth = layout.GetContentWidth(this, parentValue, viewportValue);
                    break;
                case UIUnit.Parent:
                    if (parentUnit == UIUnit.Content) break;
                    baseWidth = rect.width.value * parentValue;
                    break;
                case UIUnit.View:
                    baseWidth = rect.width.value * viewportValue;
                    break;
                default:
                    baseWidth = 0;
                    break;
            }

            return baseWidth + padding.horizontal + margin.horizontal + border.horizontal;
        }

        public float GetPreferredHeight(UIUnit parentUnit, float parentValue, float viewportValue) {
            switch (rect.height.unit) {
                case UIUnit.Pixel:
                    return rect.height.value;

                case UIUnit.Content:
                    return layout.GetContentHeight(this, parentValue, viewportValue);

                case UIUnit.Parent:
                    if (parentUnit == UIUnit.Content) return 0;
                    return rect.height.value * parentValue;

                case UIUnit.View:
                    return rect.height.value * viewportValue;

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