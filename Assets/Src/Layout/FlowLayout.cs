using System;
using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src.Layout {

    public class FlowLayout : UILayout {

        public override LayoutData Run(Rect viewport, LayoutData parentLayoutData, UIElement element) {

            LayoutData retn = new LayoutData(parentLayoutData, element.id);

            retn.relativeToWidth = element.style.rectWidth.unit;
            retn.relativeToHeight = element.style.rectHeight.unit;

            if ((retn.relativeToWidth & UIUnit.Fixed) != 0) {
                retn.width = GetFixedWidth(viewport, element.style.rectWidth, retn.parent);
            }

            if ((retn.relativeToHeight & UIUnit.Fixed) != 0) {
                retn.height = GetFixedHeight(viewport, element.style.rectHeight, retn.parent);
            }

            List<UIElement> children = element.children;

            float minX = 0;
            float minY = 0;
            float maxX = 0;
            float maxY = 0;

            float xOffset = 0;
            float yOffset = 0;

            for (int i = 0; i < children.Count; i++) {
                UIElement child = children[i];
                UILayout childLayout = child.style.layout;

                LayoutData childLayoutData = childLayout.Run(viewport, retn, child);

                if (retn.relativeToWidth == UIUnit.Content) {
                    minX = Math.Min(minX, childLayoutData.x);
                    maxX = Math.Max(maxX, childLayoutData.x);
                }

                if (retn.relativeToHeight == UIUnit.Content) {
                    minY = Math.Min(minY, childLayoutData.y);
                    maxY = Math.Max(maxY, childLayoutData.y);
                }

                if (!childLayoutData.isInFlow) continue;

                if (element.style.layoutDirection == LayoutDirection.Column) {
                    childLayoutData.y = yOffset;
                    yOffset -= childLayoutData.height;
                }
                else {
                    childLayoutData.x = xOffset;
                    xOffset += childLayoutData.width;
                }

            }

            // todo -- set height / width if content relative

            retn.isInFlow = !element.style.ignoreLayout;

            return retn;

        }

        private float GetFixedWidth(Rect viewport, UIMeasurement width, LayoutData parent) {
            switch (width.unit) {

                case UIUnit.Pixel:
                    return width.value;

                case UIUnit.Content:
                    throw new Exception("Bad");

                case UIUnit.Parent:
                    if (parent == null) {
                        return 0f;
                    }
                    return parent.width * width.value;

                case UIUnit.View:
                    return viewport.width * width.value;
            }

            return 0;
        }

        private float GetFixedHeight(Rect viewport, UIMeasurement height, LayoutData parent) {
            switch (height.unit) {

                case UIUnit.Pixel:
                    return height.value;

                case UIUnit.Content:
                    throw new Exception("Bad");

                case UIUnit.Parent:
                    if (parent == null) {
                        return 0f;
                    }
                    return parent.height * height.value;

                case UIUnit.View:
                    return viewport.height * height.value;
            }

            return 0;
        }

    }

}