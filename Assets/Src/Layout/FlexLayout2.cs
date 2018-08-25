using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src.Layout {

    public enum DisplayType {

        Block,
        Inline,
        Flex

    }
    
    public class LayoutElement {

        public UIElement uiElement;
        public LayoutElement parent;
        public LayoutElement[] children;

    }

    public class FlexLayout2 : UILayout {

        public FlexLayout2(ITextSizeCalculator textSizeCalculator) : base(textSizeCalculator) { }

        public override void Run(Rect viewport, LayoutDataSet size, Rect[] results) {
            
        }

        public void RunWidthPass(Rect viewport, LayoutData current, Rect[] results) {

            List<LayoutData> children = current.children;

            // Phase 1, determine our width

            switch (current.element.style.rectWidth.unit) {
                    case UIUnit.Auto: // ie: fill-container-content-area
                            // width is the width of the last fixed(block) parent
                        break;
                    case UIUnit.Content:
                            // depends on the layout type and direction
                            // row -> basically need to run layout, we need to sum the widths of each in flow child
                            //        this will depend on what the layout needs to do, something like a grid or radial
                            //        layout would need to compute this differently
                            // col -> take the max of all child widths 
                        break;
                    case UIUnit.Parent: // fill-container-total-area
                            // width is width of the direct parent
                            // if parent is sized based on content, width is 0 and log warning in inspector
                        break;
                    case UIUnit.Pixel:
                            // width is pixel
                        break;
                    case UIUnit.View:
                            // width is view unit
                        break;
            }
            
            // last block parent
            // recurse into children
            // where width is auto
            // width = width of last fixed parent
            // where width is content
            // get content size, passing along last block parent
            // where width is parent
            // if parent is content -> width = 0
            // else width = parent width
            // where width is pixel
            // width = value

            //

        }
    }

}