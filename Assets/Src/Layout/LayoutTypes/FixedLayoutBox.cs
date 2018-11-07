using System;
using UIForia.Rendering;
using UIForia.Systems;
using UnityEngine;

namespace UIForia.Layout.LayoutTypes {

    public class FixedLayoutBox : LayoutBox {

        public FixedLayoutBox(LayoutSystem layoutSystem, UIElement element)
            : base(layoutSystem, element) { }

        protected override float ComputeContentWidth() {
            float minX = 0;
            float maxX = 0;
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                float x = child.TransformX;

                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x + child.GetWidths().clampedSize);
            }

            return maxX - minX;
        }

        protected override float ComputeContentHeight(float width) {
            float minY = 0;
            float maxY = 0;
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                float y = child.TransformY;

                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y + child.GetHeights(child.GetWidths().clampedSize).clampedSize);
            }

            return Mathf.Max(0, maxY - minY);
        }

        public override void RunLayout() {
            float minX = 0;
            float maxX = 0;
            float minY = 0;
            float maxY = 0;

            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

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