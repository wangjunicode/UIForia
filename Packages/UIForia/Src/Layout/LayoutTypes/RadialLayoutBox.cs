using System;
using System.Collections.Generic;
using UIForia.Elements;
using UnityEngine;

namespace UIForia.Layout.LayoutTypes {

    public enum RadialItemRotation {

        None,
        Natural,
        Vertical

    }

    public enum RadialSpacing {

        Uniform,
        Width,
        Height,
        DiagonalBoxSize

    }

    public enum RadialOffset {

        Left,
        Right,
        Center

    }

    public class RadialLayoutBox : LayoutBox {

        private List<float> widths;
        private List<float> heights;
        
        protected override float ComputeContentWidth() {
            return 0; // todo
        }

        protected override float ComputeContentHeight(float width) {
            return 0;  // todo 
        }

        protected override void OnChildrenChanged() { }

        public override void RunLayout() {

            float dist = ResolveFixedWidth(style.RadialLayoutRadius);

            float startAngle = style.RadialLayoutStartAngle;
            float maxAngle = style.RadialLayoutEndAngle;

            // todo support radial offsets
            // todo handle edge cases

            float step = ((maxAngle - startAngle)) / (children.Count - 1);
            Vector2 center = new Vector2(allocatedWidth * 0.5f, allocatedHeight * 0.5f);

            for (int i = 0; i < children.Count; i++) {

                float width = children[i].GetWidths().clampedSize;
                float height = children[i].GetHeights(width).clampedSize;

                float x = dist * Mathf.Cos(startAngle * Mathf.Deg2Rad);
                float y = dist * Mathf.Sin(startAngle * Mathf.Deg2Rad);

                Vector2 vPos = new Vector2(center.x + x, center.y + y);

                children[i].SetAllocatedRect(vPos.x - (width * 0.5f), vPos.y - (height * 0.5f), width, height);
                startAngle += step;
            }

            actualWidth = allocatedWidth;
            actualHeight = allocatedHeight;
        }

    }

}