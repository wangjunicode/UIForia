using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rendering;
using Src.Elements;
using Src.Systems;
using UnityEngine;

namespace Src.Layout {

    [DebuggerDisplay("{" + nameof(element) + "}")]
    public class LayoutNode : IHierarchical {

        // todo intrinsic min / max width (ie text min-width = width of longest word)

        public const int k_MeasurementResultCount = 4;
        public Dimensions rect;

        public UILayout layout;

        public string textContent;
        public float preferredTextWidth;

        public float contentStartOffsetX;
        public float contentEndOffsetX;
        public float contentStartOffsetY;
        public float contentEndOffsetY;
        public bool isTextElement;
        public bool isImageElement; // todo -- replace w/ intrinsic width & override

        public readonly UIElement element;
        public readonly List<LayoutNode> children;

        public VirtualScrollbar horizontalScrollbar;
        public VirtualScrollbar verticalScrollbar;

        private MeasureResult[] measureResults;
        private int currentMeasureResultIndex;

        public LayoutNode(UIElement element) {
            this.element = element;
            this.children = new List<LayoutNode>();
        }

        public bool isInFlow => element.style.layoutParameters.flow != LayoutFlowType.OutOfFlow;

        public float horizontalOffset => contentStartOffsetX + contentEndOffsetX;
        public float verticalOffset => contentStartOffsetY + contentEndOffsetY;

        public float intrinsicWidth;
        public float intrinsicHeight;

        public int UniqueId => element.id;
        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

        public void UpdateData(LayoutSystem layoutSystem) {
            UIStyleSet style = element.style;
            contentStartOffsetX = style.paddingLeft + style.marginLeft + style.borderLeft;
            contentEndOffsetX = style.paddingRight + style.marginRight + style.borderRight;

            contentStartOffsetY = style.paddingTop + style.marginTop + style.borderTop;
            contentEndOffsetY = style.paddingBottom + style.marginBottom + style.borderBottom;

            layout = layoutSystem.GetLayoutInstance(style.layoutType);

            rect = style.dimensions;

            UITextElement textElement = element as UITextElement;
            if (textElement != null) {
                isTextElement = true;
                SetTextContent(textElement.GetText());
            }

            UIImageElement imageElement = element as UIImageElement;
            if (imageElement != null) {
                isImageElement = true;
                if (imageElement.src.asset != null) {
                    intrinsicWidth = imageElement.src.asset.width;
                    intrinsicHeight = imageElement.src.asset.height;
                }
                else {
                    intrinsicWidth = 0;
                    intrinsicHeight = 0;
                }
            }
        }

        public float GetMinWidth(UIUnit parentUnit, float parentValue, float viewportValue) {
            LayoutConstraints constraints = element.style.constraints;
            switch (constraints.minWidth.unit) {
                case UIUnit.ParentContentArea:
                    return 0;

                case UIUnit.Pixel:
                    return constraints.minWidth.value;

                case UIUnit.Content:
                    return layout.GetContentWidth(this, parentValue - (contentStartOffsetX + contentEndOffsetX), viewportValue) * constraints.minWidth.value;

                case UIUnit.ParentSize:
                    return constraints.minWidth.value * parentValue;

                case UIUnit.View:
                    return constraints.minWidth.value * viewportValue;

                default:
                    return 0;
            }
        }

        public void SetTextContent(string text) {
            textContent = text;
            UpdateTextMeasurements();
        }

        public void UpdateTextMeasurements() {
            if (!isTextElement) return;

            currentMeasureResultIndex = 0;
            measureResults = measureResults ?? new MeasureResult[k_MeasurementResultCount];

            for (int i = 0; i < measureResults.Length; i++) {
                measureResults[i] = new MeasureResult();
            }

            if (layout != null) {
                preferredTextWidth = layout.GetTextWidth(textContent, element.style);
            }
        }

        public float GetTextHeight(float computedWidth) {
            int intWidth = (int) computedWidth;
            for (int i = 0; i < measureResults.Length; i++) {
                if (measureResults[i].width == intWidth) {
                    return measureResults[i].height;
                }
            }

            float height = layout.GetTextHeight(textContent, element.style, computedWidth);

            currentMeasureResultIndex = (currentMeasureResultIndex + 1) % measureResults.Length;
            measureResults[currentMeasureResultIndex] = new MeasureResult(intWidth, height);

            return height;
        }

        public float GetPreferredWidth(UIUnit parentUnit, float parentValue, float viewportValue) {
            float baseWidth;

            if (isTextElement) {
                return Math.Max(0, Mathf.Min(preferredTextWidth, parentValue));
            }

            if (isImageElement) return intrinsicWidth;
            
            switch (rect.width.unit) {
                case UIUnit.ParentContentArea:
                    baseWidth = parentValue;
                    break;

                case UIUnit.Pixel:
                    baseWidth = rect.width.value;
                    break;

                case UIUnit.Content:
                    baseWidth = layout.GetContentWidth(this, parentValue - (contentStartOffsetX + contentEndOffsetX), viewportValue);
                    break;

                case UIUnit.ParentSize:
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

            return baseWidth + (contentStartOffsetX + contentEndOffsetX);
        }

        public float GetPreferredHeight(UIUnit parentUnit, float computedWidth, float parentValue, float viewportValue) {
            if (isTextElement) {
                return GetTextHeight(computedWidth);
            }

            if (isImageElement) return intrinsicHeight;

            float baseHeight;

            switch (rect.height.unit) {
                case UIUnit.ParentContentArea: // fit parent content
                    // should be renamed & defined as nearest parent block
                    baseHeight = layout.GetContentHeight(this, computedWidth, parentValue - verticalOffset, viewportValue);
                    break;
                case UIUnit.Pixel:
                    baseHeight = rect.height.value;
                    break;
                case UIUnit.Content:
                    baseHeight = layout.GetContentHeight(this, computedWidth, parentValue - verticalOffset, viewportValue) * rect.height.value;
                    break;
                // idea: setting for filling parent + margin / padding or border
                case UIUnit.ParentSize: // fill parent extents, width + marginHorizontal + borderHorizontal + paddingHorizontal
                    if (parentUnit == UIUnit.Content) {
                        baseHeight = 0;
                    }
                    else {
                        baseHeight = rect.height.value * parentValue;
                    }

                    break;
                case UIUnit.View:
                    baseHeight = rect.height.value * viewportValue;
                    break;
                default:
                    baseHeight = 0;
                    break;
            }

            return baseHeight + verticalOffset;
        }

        public float GetMaxWidth(UIUnit parentUnit, float parentValue, float viewportValue) {
            LayoutConstraints constraints = element.style.constraints;
            switch (constraints.maxWidth.unit) {
                case UIUnit.Pixel:
                    return constraints.maxWidth.value;

                case UIUnit.Content:
                    throw new NotImplementedException();

                case UIUnit.ParentSize:
                    if (parentUnit == UIUnit.Content) return 0;
                    return constraints.maxWidth.value * parentValue;

                case UIUnit.View:
                    return constraints.maxWidth.value * viewportValue;

                default:
                    return 0;
            }
        }

        public float GetMinHeight(UIUnit parentUnit, float parentValue, float viewportValue) {
            LayoutConstraints constraints = element.style.constraints;
            switch (constraints.minHeight.unit) {
                case UIUnit.Pixel:
                    return constraints.minHeight.value;

                case UIUnit.Content:
                    throw new NotImplementedException();

                case UIUnit.ParentSize:
                    if (parentUnit == UIUnit.Content) return 0;
                    return constraints.minHeight.value * parentValue;

                case UIUnit.View:
                    return constraints.minHeight.value * viewportValue;

                default:
                    return 0;
            }
        }

        public float GetMaxHeight(UIUnit parentUnit, float parentValue, float viewportValue) {
            LayoutConstraints constraints = element.style.constraints;
            switch (constraints.maxHeight.unit) {
                case UIUnit.Pixel:
                    return constraints.maxHeight.value;

                case UIUnit.Content:
                    throw new NotImplementedException();

                case UIUnit.ParentSize:
                    throw new NotImplementedException();

                case UIUnit.View:
                    return constraints.maxHeight.value * viewportValue;

                default:
                    return 0;
            }
        }


        private struct MeasureResult {

            public readonly int width;
            public readonly float height;

            public MeasureResult(int width, float height) {
                this.width = width;
                this.height = height;
            }

        }

    }

}