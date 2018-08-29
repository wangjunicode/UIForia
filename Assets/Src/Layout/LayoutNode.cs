using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rendering;
using Src.Systems;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Src.Layout {

    [DebuggerDisplay("{element}")]
    public class LayoutNode : IHierarchical {

        public Rect outputRect;
        public LayoutRect rect;
        public UILayout layout;
        public List<LayoutNode> children;
        public LayoutParameters parameters;
        public LayoutConstraints constraints;

        public string textContent;
        public Vector2 textContentSize;
        public float previousParentWidth;

        public float contentStartOffsetX;
        public float contentEndOffsetX;
        public float contentStartOffsetY;
        public float contentEndOffsetY;
        public bool isTextElement;
        public readonly UIStyleSet style;
        public readonly UIElement element;
        public Vector2 localPosition;

        public LayoutNode(UIElement element) {
            this.element = element;
            this.children = new List<LayoutNode>();
            this.style = element.style;
        }

        public bool isInFlow => parameters.flow != LayoutFlowType.OutOfFlow;

        public void UpdateData(LayoutSystem layoutSystem) {
            previousParentWidth = float.MinValue;
            textContentSize = Vector2.zero;

            contentStartOffsetX = style.paddingLeft + style.marginLeft + style.borderLeft;
            contentEndOffsetX = style.paddingRight + style.marginRight + style.borderRight;

            contentStartOffsetY = style.paddingTop + style.marginTop + style.borderTop;
            contentEndOffsetY = style.paddingBottom + style.marginBottom + style.borderBottom;

            parameters = style.layoutParameters;
            layout = layoutSystem.GetLayoutInstance(parameters.type);

            constraints = style.constraints;
            rect = style.rect;
            
            UITextElement textElement = element as UITextElement;
            if (textElement != null) {
                isTextElement = true;
                textContent = textElement.GetText();
            }
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

        public void SetTextContent(string text) {
            previousParentWidth = float.MinValue;
            textContent = text;
        }

        public float GetPreferredWidth(UIUnit parentUnit, float parentValue, float viewportValue) {
            float baseWidth;
            if (isTextElement) {
                // weirdly the text size is calculated differently when the mouse is down
                float width = new IMGUITextSizeCalculator().CalcTextWidth(textContent, style);
                if (Mathf.Abs(width - textContentSize.x) > 3f) {
                    textContentSize.x = width;
                }

                return textContentSize.x;
            }
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

            return baseWidth;// +  (contentStartOffsetX + contentEndOffsetX);
        }

        public float GetPreferredHeight(UIUnit parentUnit, float computedWidth, float parentValue, float viewportValue) {
            computedWidth = computedWidth - (contentEndOffsetX + contentStartOffsetX);
            switch (rect.height.unit) {
                case UIUnit.Auto: // fit parent content
                    // should be renamed & defined as nearest parent block
                    return layout.GetContentHeight(this, computedWidth, parentValue, viewportValue);

                case UIUnit.Pixel:
                    return rect.height.value;

                case UIUnit.Content:
                    return layout.GetContentHeight(this, computedWidth, parentValue, viewportValue) * rect.height.value;

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

        public int UniqueId => element.id;
        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

    }

}