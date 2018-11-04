using Src.Rendering;
using Src.Elements;
using Src.Systems;
using UnityEngine;

namespace Src.Layout.LayoutTypes {

    public class ScrollbarLayoutBox : LayoutBox {

        public ScrollbarLayoutBox(LayoutSystem layoutSystem, UIElement element) : base(layoutSystem, element) { }

        public override void RunLayout() {
            float minX = 0;
            float maxX = 0;
            float minY = 0;
            float maxY = 0;
            
            VirtualScrollbarHandle handle = ((VirtualScrollbar) element).handle;
            
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                if (child.element.isDisabled) continue;

                float x = child.TransformX;
                float y = child.TransformY;

                float width = child.GetWidths().clampedSize;
                LayoutBoxSize heights = child.GetHeights(width);

                child.SetAllocatedRect(x, y, width, heights.clampedSize);

                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x + width);
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y + heights.clampedSize);
            } 
        }

        public override void OnStylePropertyChanged(StyleProperty property) {
            switch (property.propertyId) {
                case StylePropertyId.TransformPositionX:
                case StylePropertyId.TransformPositionY:
                    RequestContentSizeChangeLayout();
                    break;
            }
        }
        
    }

}