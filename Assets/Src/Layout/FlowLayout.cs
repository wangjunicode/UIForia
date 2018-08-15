using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src.Layout {

    public class FlowLayout : UILayout {

        public override void Run(UIView view, Rect rect, UIElement element) {
            if (element.style.layoutDirection == LayoutDirection.Column) {
                RunLayoutVertical(view, rect, element);
            }
            else {
                RunLayoutHorizontal(view, rect, element);
            }
        }

        private void RunLayoutHorizontal(UIView view, Rect space, UIElement element) {
            
        }

        
        
        private void RunLayoutVertical(UIView view, Rect viewSpaceContentRect, UIElement element) {

            List<UIElement> children = element.children;
            float yOffset = 0;
            for (int i = 0; i < children.Count; i++) {
                UIElement child = children[i];
                UILayout childLayout = child.style.layout;
                float width = childLayout.GetLayoutWidth(child);
                float height = childLayout.GetLayoutHeight(child);

                Rect rect = new Rect();
                
                rect.x = 0f; // maybe just take rect.x from style
                rect.y = yOffset;
                rect.height = height;
                rect.width = width;

                yOffset -= height;
                view.SetLayoutRect(child, rect);
            }

        }

        public override float GetLayoutWidth(UIElement element) {
            UIMeasurement width = element.style.rectWidth;

            return width.value;
        }

        public override float GetLayoutHeight(UIElement element) {
            UIMeasurement height = element.style.rectHeight;
            switch (height.unit) {

                case UIUnit.Pixel:
                    return height.value;

                case UIUnit.Content: 
                    return 0f;

                case UIUnit.Parent:
                    if (element.parent == null) {
                        return 0f;
                    }
                    UIMeasurement parentHeight = element.parent.style.rectHeight;
                    if (parentHeight.unit == UIUnit.Content) {
                        return 0f;
                    }
                    return GetLayoutHeight(element.parent);
                
                case UIUnit.View:
                    return 0f;
            }

            return 0;
        }

    }

}