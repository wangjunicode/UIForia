using System;
using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src.Layout {

    public class FlowLayout : UILayout {

        public override void Run(Rect viewport, LayoutData layoutData) {

            float x = 0;
            float y = 0;
            float outputWidth = 0;
            
            if (layoutData.relativeToWidth == UIUnit.Fixed) {
                outputWidth = layoutData.width;
            }
            else if (layoutData.relativeToWidth == UIUnit.View) {
                outputWidth = viewport.width * layoutData.width;
            }
            else if (layoutData.relativeToWidth == UIUnit.Content) {
//                if width is content relative
//                if direction is row
//                for each child
//                if in flow
//                    assume linear layout
//                    x = 0
//                foreach child x += width
//                foreach aboslute position thing
//                if thing.topRight.x > widthSoFar
//                widthSoFar = thinig.topRight.x
//                width = widthSoFar.clamp(min, max);               
//                else 
//                width = max width(children) || absoulte things that overflow
//                outputWidth = layoutData
                // ComputePreferredContentWidth();

                float o = 0;
                for (int i = 0; i < layoutData.children.Count; i++) {
                    o += layoutData.children[i].GetFixedWidth();
                }
                
            }
            else if (layoutData.relativeToWidth == UIUnit.Parent) {
                outputWidth = layoutData.parent.width * layoutData.width;
            }
            
            // wrap occurs if wrap is enabled and we can't fit into our parent-allocated width
            
            for (int i = 0; i < layoutData.children.Count; i++) {
                LayoutData child = layoutData.children[i];
                if (layoutData.layoutDirection == LayoutDirection.Row) {
                    child.x = x;
                    x += child.GetWidth(this);
                }
            }
            // if layoutdata.renderData.unityTransform != null -> apply
        }

//            retn.relativeToWidth = element.style.rectWidth.unit;
//            retn.relativeToHeight = element.style.rectHeight.unit;
//
//            if ((retn.relativeToWidth & UIUnit.Fixed) != 0) {
//                retn.width = GetFixedWidth(viewport, element.style.rectWidth, retn.parent);
//            }
//
//            if ((retn.relativeToHeight & UIUnit.Fixed) != 0) {
//                retn.height = GetFixedHeight(viewport, element.style.rectHeight, retn.parent);
//            }
//
//            List<UIElement> children = null; //element.children;
//
//            float minX = 0;
//            float minY = 0;
//            float maxX = 0;
//            float maxY = 0;
//
//            float xOffset = 0;
//            float yOffset = 0;
//
//            for (int i = 0; i < children.Count; i++) {
//                UIElement child = children[i];
//                UILayout childLayout = child.style.layout;
//
//                LayoutData childLayoutData = childLayout.Run(viewport, retn, child);
//
//                if (retn.relativeToWidth == UIUnit.Content) {
//                    minX = Math.Min(minX, childLayoutData.x);
//                    maxX = Math.Max(maxX, childLayoutData.x);
//                }
//
//                if (retn.relativeToHeight == UIUnit.Content) {
//                    minY = Math.Min(minY, childLayoutData.y);
//                    maxY = Math.Max(maxY, childLayoutData.y);
//                }
//
//                if (!childLayoutData.isInFlow) continue;
//
//                if (element.style.layoutDirection == LayoutDirection.Column) {
//                    childLayoutData.y = yOffset;
//                    yOffset -= childLayoutData.height;
//                }
//                else {
//                    childLayoutData.x = xOffset;
//                    xOffset += childLayoutData.width;
//                }
//
//            }
//
//            // first pass - get child layout data
//            // second pass - do layout with real values
//            
//            // todo -- set height / width if content relative
//
//            retn.isInFlow = !element.style.ignoreLayout;
//
//            return retn;
//
//        }

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