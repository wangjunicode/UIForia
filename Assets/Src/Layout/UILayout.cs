using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src.Layout {

    public abstract class UILayout {

        protected readonly ITextSizeCalculator textSizeCalculator;

        protected UILayout(ITextSizeCalculator textSizeCalculator) {
            this.textSizeCalculator = textSizeCalculator;
        }

        public abstract void Run(Rect viewport, LayoutDataSet size, Rect[] results);

        public virtual float GetContentWidth(LayoutData data, float contentSize, float viewportSize) {
            if ((data.element.flags & UIElementFlags.TextElement) != 0) {
                return data.textContentSize.x;
            }

            List<LayoutData> children = data.children;
            // todo include statically positioned things who's breadth exceeds max computed
            float output = 0;
            if (data.parameters.direction == LayoutDirection.Row) {
                // return sum of preferred sizes
                for (int i = 0; i < children.Count; i++) {
                    LayoutData child = children[i];
                    output += child.GetPreferredWidth(data.rect.width.unit, contentSize, viewportSize);
                }
            }
            else {

                for (int i = 0; i < children.Count; i++) {
                    LayoutData child = children[i];
                    output = Mathf.Max(output, child.GetPreferredWidth(data.rect.width.unit, contentSize, viewportSize));
                }

            }

            return output;
        }

        public float GetContentHeight(LayoutData data, float parentWidth, float contentSize, float viewportSize) {

            if ((data.element.flags & UIElementFlags.TextElement) != 0) {
                // todo -- add metrics per component about calc calls
                if (data.previousParentWidth != parentWidth) {
                    data.previousParentWidth = parentWidth;
                    data.textContentSize.y = textSizeCalculator.CalcTextHeight(data.textContent, data.element.style, parentWidth);
                }
                return data.textContentSize.y;
            }

            List<LayoutData> children = data.children;

            // todo include statically positioned things who's breadth exceeds max computed
            float output = 0;

            if (data.parameters.direction == LayoutDirection.Row) {
                for (int i = 0; i < children.Count; i++) {
                    output = Mathf.Max(output, children[i].GetPreferredHeight(data.rect.height.unit, parentWidth, contentSize, viewportSize));
                }
            }
            else {
                for (int i = 0; i < children.Count; i++) {
                    output += children[i].GetPreferredHeight(data.rect.height.unit, parentWidth, contentSize, viewportSize);
                }
            }

            return output;
        }

        public float GetPreferredWidth(LayoutData item, UIUnit parentUnit, float parentValue, float viewportValue) {
            float baseWidth = 0;

            switch (item.rect.width.unit) {
                case UIUnit.Auto:
                    baseWidth = parentValue;
                    break;

                case UIUnit.Pixel:
                    baseWidth = item.rect.width.value;
                    break;

                case UIUnit.Content:
                    baseWidth = item.layout.GetContentWidth(item, parentValue, viewportValue);
                    break;

                case UIUnit.Parent:
                    if (parentUnit == UIUnit.Content) return 0;
                    baseWidth = item.rect.width.value * parentValue;
                    break;

                case UIUnit.View:
                    baseWidth = item.rect.width.value * viewportValue;
                    break;

                default:
                    baseWidth = 0;
                    break;
            }

            return baseWidth;
        }

        public float GetPreferredHeight(LayoutData item, UIUnit parentUnit, float computedWidth, float parentValue, float viewportValue) {
            switch (item.rect.height.unit) {
                case UIUnit.Auto:
                    return item.ContentStartOffsetY
                           + item.layout.GetContentHeight(item, computedWidth, parentValue, viewportValue)
                           + item.ContentEndOffsetY;

                case UIUnit.Pixel:
                    return item.rect.height.value;

                case UIUnit.Content:
                    return (item.ContentStartOffsetY
                            + item.layout.GetContentHeight(item, computedWidth, parentValue, viewportValue)
                            + item.ContentEndOffsetY) * item.rect.height.value;

                case UIUnit.Parent:
                    if (parentUnit == UIUnit.Content) return 0;
                    return item.rect.height.value * parentValue;

                case UIUnit.View:
                    return item.rect.height.value * viewportValue;

                default:
                    return 0;
            }
        }

    }

}