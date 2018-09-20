using System;
using System.Collections.Generic;
using Rendering;
using Src.Systems;
using Src.Util;
using UnityEngine;

namespace Src.Layout {

    public class FixedLayout : UILayout {

        public FixedLayout(ITextSizeCalculator textSizeCalculator) : base(textSizeCalculator) { }

        public override List<Rect> Run(Rect viewport, LayoutNode currentNode) {
            Rect screenRect = currentNode.element.layoutResult.ScreenRect;
            
            float contentAreaWidth = screenRect.xMax - screenRect.x - currentNode.contentEndOffsetX - currentNode.contentStartOffsetX;
            float contentAreaHeight = screenRect.yMax - screenRect.y - currentNode.contentEndOffsetY - currentNode.contentStartOffsetY;

            List<LayoutNode> children = currentNode.children;
            List<Rect> retn = ListPool<Rect>.Get();
            
            for (int i = 0; i < children.Count; i++) {
                LayoutNode child = children[i];
                Rect rect = new Rect();
                if (child.element.isDisabled) {
                    retn.Add(rect);
                    continue;
                }

                float x = GetPixelValue(child.element.style.positionX, contentAreaWidth, viewport.width);
                float y = GetPixelValue(child.element.style.positionY, contentAreaHeight, viewport.height);
                
                float width = child.GetPreferredWidth(currentNode.rect.width.unit, contentAreaWidth, viewport.width);

                // todo -- is this weird?
                if (child.rect.width.unit == UIUnit.Auto) {
                    width = contentAreaWidth - x;
                }
                
                float height = child.GetPreferredHeight(currentNode.rect.height.unit, width, contentAreaHeight, viewport.height);
                retn.Add(new Rect(x, y, width, height));
            }

            return retn;
        }

        public override float GetContentWidth(LayoutNode node, float contentSize, float viewportSize) {
            List<LayoutNode> children = node.children;
            float minX = 0;
            float maxX = 0;
            for (int i = 0; i < children.Count; i++) {
                LayoutNode child = children[i];

                if (child.element.isDisabled) continue;

                float x = GetPixelValue(child.element.style.positionX, contentSize, viewportSize);
                float width = child.GetPreferredWidth(node.element.style.dimensions.width.unit, contentSize, viewportSize);
                
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

                float y = GetPixelValue(child.element.style.positionY, adjustedWidth, viewportSize);
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y + child.GetPreferredHeight(node.element.style.dimensions.height.unit, adjustedWidth, parentWidth, viewportSize));
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