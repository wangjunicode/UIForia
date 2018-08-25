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

        public float previousParentWidth;

        public LayoutData parent;
        public UILayout layout;

        public readonly UIElement element;
        public readonly List<LayoutData> children;

        public string textContent;

        public readonly UIStyleSet style;
        public float horizontal;

        public LayoutData(UIElement element) {
            this.element = element;
            this.children = new List<LayoutData>();
            this.previousParentWidth = float.MinValue;
            this.constraints = LayoutConstraints.Unset;
            this.style = element.style;
        }

        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

        public bool isInFlow => parameters.flow != LayoutFlowType.OutOfFlow;

        public float ContentStartOffsetX => margin.left + padding.left + border.left;
        public float ContentEndOffsetX => margin.right + padding.right + border.right;

        public float ContentStartOffsetY => margin.top + padding.top + border.top;
        public float ContentEndOffsetY => margin.bottom + padding.bottom + border.bottom;

        public float GetPreferredWidth(UIUnit parentUnit, float parentValue, float viewportValue) {
            float baseWidth = 0;

            switch (rect.width.unit) {
                case UIUnit.Auto:
                    baseWidth = parentValue;
                    break;

                case UIUnit.Pixel:
                    baseWidth = rect.width.value;
                    break;

                case UIUnit.Content:
                    baseWidth = layout.GetContentWidth(this, parentValue, viewportValue);
                    break;

                case UIUnit.Parent:
                    if (parentUnit == UIUnit.Content) return 0;
                    baseWidth = rect.width.value * parentValue;
                    break;

                case UIUnit.View:
                    baseWidth = rect.width.value * viewportValue;
                    break;

                default:
                    baseWidth = 0;
                    break;
            }

            return baseWidth;
        }

        public float GetPreferredHeight(UIUnit parentUnit, float computedWidth, float parentValue, float viewportValue) {
            computedWidth = computedWidth - horizontal;
            switch (rect.height.unit) {
                case UIUnit.Auto: // fit parent content
                    // should be renamed & defined as nearest parent block
                    return ContentStartOffsetY
                           + layout.GetContentHeight(this, computedWidth, parentValue, viewportValue)
                           + ContentEndOffsetY;

                case UIUnit.Pixel:
                    return rect.height.value;

                case UIUnit.Content:
                    return (ContentStartOffsetY
                            + layout.GetContentHeight(this, computedWidth, parentValue, viewportValue)
                            + ContentEndOffsetY) * rect.height.value;

                // idea: setting for filling parent + margin / padding or border
                case UIUnit.Parent: // fill parent extents, width + marginHorizontal + borderHorizontal + paddingHorizontal
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

        public float GetMinWidth(UIUnit parentUnit, float parentValue, float viewportValue) {
            switch (constraints.minWidth.unit) {
                case UIUnit.Auto:
                    return 0;

                case UIUnit.Pixel:
                    return constraints.minWidth.value;

                case UIUnit.Content:
                    throw new NotImplementedException();

                case UIUnit.Parent:
                    throw new NotImplementedException();

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

                case UIUnit.Parent:
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

                case UIUnit.Parent:
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

                case UIUnit.Parent:
                    throw new NotImplementedException();

                case UIUnit.View:
                    return constraints.maxHeight.value * viewportValue;

                default:
                    return 0;
            }
        }

        public void UpdateFromStyle() {
            border = style.border;
            padding = style.padding;
            margin = style.margin;
            parameters = style.layout;
            constraints = style.constraints;
            rect = style.rect;
            horizontal = border.horizontal - padding.horizontal - margin.horizontal;
        }

        public void SetLayout(UILayout layout) {
            this.layout = layout;
        }

        public void SetMargin(ContentBoxRect margin) {
            this.margin = margin;
            horizontal = border.horizontal - padding.horizontal - margin.horizontal;
        }
        
        public void SetPadding(ContentBoxRect padding) {
            this.padding = padding;
            horizontal = border.horizontal - padding.horizontal - margin.horizontal;
        }
        
        public void SetBorder(ContentBoxRect border) {
            this.border = border;
            horizontal = border.horizontal - padding.horizontal - margin.horizontal;
        }
        
        public void SetTextContent(string text) {
            previousParentWidth = float.MinValue;
            textContent = text;
        }

    }

}