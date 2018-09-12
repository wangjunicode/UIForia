using System;
using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src.Layout {

    public class FixedLayout : UILayout {

        public FixedLayout(ITextSizeCalculator textSizeCalculator) : base(textSizeCalculator) { }

        public override void Run(Rect viewport, LayoutNode currentNode) {
            Rect size = currentNode.outputRect;
            float contentStartX = currentNode.contentStartOffsetX;
            float contentStartY = currentNode.contentStartOffsetY;
            float contentEndX = size.xMax - size.x - currentNode.contentEndOffsetX;
            float contentEndY = size.yMax - size.y - currentNode.contentEndOffsetY;
            float contentAreaWidth = contentEndX - contentStartX;
            float contentAreaHeight = contentEndY - contentStartY;

            List<LayoutNode> children = currentNode.children;

            for (int i = 0; i < children.Count; i++) {
                LayoutNode child = children[i];
                if (child.element.isDisabled) {
                    continue;
                }

                float x = GetPixelValue(child.style.positionX, contentAreaWidth, viewport.width);
                float y = GetPixelValue(child.style.positionY, contentAreaHeight, viewport.height);
                float width = child.GetPreferredWidth(currentNode.rect.width.unit, contentAreaWidth, viewport.width);

                if (child.rect.width.unit == UIUnit.Auto) {
                    width = contentAreaWidth - x;
                }
                
                float height = child.GetPreferredHeight(currentNode.rect.height.unit, width, contentAreaHeight, viewport.height);
                
                // todo -- this is weeeeird
                child.localPosition = new Vector2(
                    x - (currentNode.element.style.marginLeft),
                    y - (currentNode.element.style.marginTop)
                );

                child.outputRect = new Rect(x + size.x, y + size.y, width, height);
            }
        }

        public override float GetContentWidth(LayoutNode node, float contentSize, float viewportSize) {
            List<LayoutNode> children = node.children;
            float minX = 0;
            float maxX = 0;
            for (int i = 0; i < children.Count; i++) {
                LayoutNode child = children[i];

                if (child.element.isDisabled) continue;

                float x = GetPixelValue(child.style.positionX, contentSize, viewportSize);
                float width = child.GetPreferredWidth(node.style.dimensions.width.unit, contentSize, viewportSize);
                
                if (child.rect.width.unit == UIUnit.Auto) {
                    width = contentSize - x;
                }
                
                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x + width);
            }

            return Mathf.Max(0, maxX - minX);
        }

        public override float GetContentHeight(LayoutNode node, float adjustedWidth, float parentWidth, float viewportSize) {
            List<LayoutNode> children = node.children;
            float minY = 0;
            float maxY = 0;
            for (int i = 0; i < children.Count; i++) {
                LayoutNode child = children[i];

                if (child.element.isDisabled) continue;

                float y = GetPixelValue(child.style.positionY, adjustedWidth, viewportSize);
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y + child.GetPreferredWidth(node.style.dimensions.height.unit, adjustedWidth, viewportSize));
            }

            return Mathf.Max(0, maxY - minY);
        }
        
        private static float GetPixelValue(UIMeasurement x, float parentWidth, float viewportWidth) {
            switch (x.unit) {
                case UIUnit.Pixel:
                    return x.value;
                case UIUnit.Content:
                    return 0;
                case UIUnit.Parent:
                    return parentWidth * x.value;
                case UIUnit.View:
                    return viewportWidth * x.value;
                case UIUnit.Auto:
                    return 0;
                default:
                    return 0;
//                    throw new ArgumentOutOfRangeException();
            }
        }

        

    }

}