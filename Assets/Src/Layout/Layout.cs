using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src.Layout {

    public abstract class UILayout {

        public abstract void Run(UIElement element);

    }

    public class UILayout_Auto : UILayout {

        public override void Run(UIElement element) {
                        
        }

        
        // layout for "free" layout type, ie use element rects not layout properties
        private void RunLayoutVertical(Rect viewSpaceContentRect, UIElement element) {
            
            List<UIElement> children = element.children;
            float yOffset = 0;
            for (int i = 0; i < children.Count; i++) {
                UIElement child = children[i];
                
//                if (child.style.ignoreLayout) continue;
                
                Rect rect = new Rect();
                rect.y = yOffset;
                float height = GetLayoutHeight(child);
                rect.height = height;
            }
            
        }

        private float GetLayoutHeight(UIElement element) {
            
//            // clamp to min / max height
//            // incorporate padding / margin / border
//            
//            if(element.style.rect.height)
//                if (element.style.height.unit == UIUnit.Parent) {
//                    if (currentElement.style.height.unit == UIUnit.Content) {
//                        return 0;
//                    }
//
//                    return element.parent.style.layout.GetLayoutHeight(element.parent);
//                }
//            else if (heightUnit == UIUnit.Pixel) {
//                    return heightUnit.Value;
//                }
//            else if (heightUnit == UIUnit.Content) {
//                    // do layout here?
//                    return element.style.layout.GetLayoutHeight(element).height;
//                }
//            
            // if element content based
            // if element is rect based (ie fixed)
                // return rect.height
            // element.style.layout.Run();
            // if element is parent based -> 0 or use content based
            // if element is viewport based -> fine
            // if element.style.layoutType == LayoutType.None

            return 0;
        }
        
    }

    public class FlexLayout : UILayout {

        private UITransform transform;
        
        public override void Run(UIElement element) {
                                         
        }

    }
}