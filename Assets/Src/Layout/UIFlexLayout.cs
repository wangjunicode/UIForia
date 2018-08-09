using System.Collections.Generic;
using Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Layout {

    public class UIFlexLayout {
        
        private Rect rect;
        
        public List<UITransform> children;
        public MainAxisAlignment mainAxisAlignment;
        public CrossAxisAlignment crossAxisAlignment;
               
        public void DoLayout(UITransform transform, Rect rect) {
            
//            
//            this.rect = rect;
//            children = transform.children;
//            float remainingSpace = rect.width;
//            
//            for (int i = 0; i < children.Count; i++) {
//                UITransform flexItem = children[i];
//                if (!flexItem.isInFlow) {
//                    continue;
//                }
//                LayoutParameters layoutParams = flexItem.element.style.layoutParameters;
//                
//                flexItem.width = layoutParams.basisWidth;
//                remainingSpace -= flexItem.preferred;
//            }
//
//            if (remainingSpace > 0) {
//                GrowWidth(remainingSpace);
//            }
//            else if (remainingSpace < 0) {
//                ShrinkWidth(remainingSpace);
//            }
//            else {
//                AllocateFreeSpaceHorizontal(0);
//            }       
        }
//
//        private static float SafeDivide(float numerator, float denominator) {
//            return denominator == 0 ? 0 : numerator / denominator;
//        }
//
//        private void GrowWidth(float remainingSpace) {
//            int pieces = 0;
//            
//            for (int i = 0; i < children.Count; i++) {
//                pieces += children[i].growthFactor;
//            }
//            
//            bool didAllocate = pieces > 0;
//            while (didAllocate && remainingSpace > 0) {
//                didAllocate = false;
//                float pieceSize = SafeDivide(remainingSpace, pieces);
//                for (int i = 0; i < children.Count; i++) {
//                    FlexItem child = children[i];
//                    float maxWidth = child.max;
//                    int growthFactor = child.growthFactor;
//                    float outputWidth = child.width;
//                    if (growthFactor == 0 || outputWidth == maxWidth) {
//                        continue;
//                    }
//                    didAllocate = true;
//                    float startWidth = outputWidth;
//                    float growSize = growthFactor * pieceSize;
//                    float totalGrowWidth = startWidth + growSize;
//                    outputWidth = totalGrowWidth > maxWidth ? maxWidth : totalGrowWidth;
//                    remainingSpace -= outputWidth - startWidth;
//                    child.width = outputWidth;
//                }
//            }
//        }
//
//        private void ShrinkWidth(float remainingSpace) {
//            int pieces = 0;
//            for (int i = 0; i < children.Count; i++) {
//                pieces += children[i].shrinkFactor;
//            }
//
//            bool didAllocate = pieces > 0;
//
//            remainingSpace *= -1;
//
//            while (didAllocate && remainingSpace > 0) {
//                didAllocate = false;
//                float pieceSize = SafeDivide(remainingSpace, pieces);
//                for (int i = 0; i < children.Count; i++) {
//                    FlexItem item = children[i];
//                    float minWidth = item.min;
//                    int shrinkFactor = item.shrinkFactor;
//
//                    if (shrinkFactor == 0 || item.width == minWidth) {
//                        continue;
//                    }
//
//                    didAllocate = true;
//                    float startWidth = item.width;
//                    float shrinkSize = shrinkFactor * pieceSize;
//                    float totalShrinkWidth = startWidth - shrinkSize;
//                    item.width = totalShrinkWidth < minWidth ? minWidth : totalShrinkWidth;
//                    remainingSpace -= item.width - startWidth;
//                }
//            }
//            AllocateFreeSpaceHorizontal(0);
//        }
//
//        private void AllocateFreeSpaceHorizontal(float gutterSize) {
//            switch (mainAxisAlignment) {
//                case MainAxisAlignment.Start:
//                case MainAxisAlignment.Default:
//                    AlignMainAxis(gutterSize, 0, 0);
//                    break;
//                case MainAxisAlignment.Center:
//                    AlignMainAxis(gutterSize, 0, gutterSize * 0.5f);
//                    break;
//                case MainAxisAlignment.End:
//                    AlignMainAxis(gutterSize, 0, gutterSize);
//                    break;
//                case MainAxisAlignment.SpaceBetween: {
//                    int segments = children.Count - 1;
//                    AlignMainAxis(gutterSize, segments, 0);
//                    break;
//                }
//                case MainAxisAlignment.SpaceAround: {
//                    int segments = children.Count;
//                    AlignMainAxis(gutterSize, segments, (gutterSize / segments) * 0.5f);
//                    break;
//                }
//            }
//        }
//
//        private void AlignMainAxis(float space, int segmentCount, float offset) {
//            float x = offset;
//            float spacerSize = SafeDivide(space, segmentCount);
//            float height = rect.height;
//            for (int i = 0; i < children.Count; i++) {
//                FlexItem item = children[i];
//                item.y = AlignCrossAxis(item, height);
//                item.x = x;
//                x += item.width + spacerSize;
//            }
//        }
//
//        private float AlignCrossAxis(FlexItem item, float height) {
//            CrossAxisAlignment alignment = item.alignment;
//            if (alignment == CrossAxisAlignment.Default) {
//                alignment = crossAxisAlignment;
//            }
//            item.height = item.preferred; //todo wrong!
//            switch (alignment) {
//                case CrossAxisAlignment.Center:
//                    return (rect.height * 0.5f) - (item.height * 0.5f);
//                case CrossAxisAlignment.End:
//                    return (height - item.height);
//                case CrossAxisAlignment.Stretch:
//                    item.height = height;
//                    return 0;
//                default:
//                    return 0;
//            }
//        }   
    }

}