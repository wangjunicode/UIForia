using System;
using Src.Systems;
using UnityEngine;

namespace Src.Layout.LayoutTypes {

    public class FixedLayoutBox : LayoutBox {

        public FixedLayoutBox(LayoutSystem2 layoutSystem, UIElement element)
            : base(layoutSystem, element) { }

        protected override Size RunContentSizeLayout() {
            float minX = 0;
            float maxX = 0;
            float minY = 0;
            float maxY = 0;
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                if (child.element.isDisabled) continue;

                float x = child.TransformX;
                float y = child.TransformY;

                float width = Math.Max(child.MinWidth, Math.Min(child.PreferredWidth, child.MaxWidth));
                float height = Math.Max(child.MinHeight, Math.Min(child.PreferredHeight, child.MaxHeight));

                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x + width);
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y + height);
            }

            return new Size(Mathf.Max(0, maxX - minX), Mathf.Max(0, maxY - minY));
        }

        public override void RunWidthLayout() {
            float minX = 0;
            float maxX = 0;
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                float x = child.TransformX;

                float width = Math.Max(child.MinWidth, Math.Min(child.PreferredWidth, child.MaxWidth));

                child.SetAllocatedXAndWidth(x, width);

                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x + width);

            }

            actualWidth = Mathf.Max(0, maxX - minX);
        }

        public override void RunHeightLayout() {
            float minY = 0;
            float maxY = 0;
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                float y = child.TransformY;

                float height = Math.Max(child.MinHeight, Math.Min(child.GetPreferredHeightForWidth(child.allocatedWidth), child.MaxHeight));

                child.SetAllocatedYAndHeight(y, height);

                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y + height);

            }

            actualHeight = Mathf.Max(0, maxY - minY);
        }

    }

}