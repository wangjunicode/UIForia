using System;
using System.Collections.Generic;
using Src.Systems;
using UnityEngine;

namespace Src.Layout.LayoutTypes {

    public class RadialLayoutBox : LayoutBox {

        public enum RadialItemRotation {

            None,
            Natural,
            Vertical

        }

        public enum RadialSpacing {

            Uniform,
            Width,
            Height,
            BoxSize

        }

        public enum RadialOffset {

            Left,
            Right,
            Center

        }

        public RadialLayoutBox(LayoutSystem layoutSystem, UIElement element) : base(layoutSystem, element) { }

        public override void RunLayout() {
            List<float> widths = new List<float>();
            List<float> heights = new List<float>();

            for (int i = 0; i < children.Count; i++) {
                widths.Add(children[i].GetWidths().clampedSize);
                heights.Add(children[i].GetHeights(widths[i]).clampedSize);
            }

            float offsetAngle = 0f;

            RadialOffset horizontalOffset = RadialOffset.Center;
            RadialOffset verticalOffset = RadialOffset.Center;

            for (int i = 0; i < children.Count; i++) {
                Vector3 vPos = new Vector3(Mathf.Cos(offsetAngle * Mathf.Deg2Rad), Mathf.Sin(offsetAngle * Mathf.Deg2Rad), 0) * 100f;

                switch (horizontalOffset) {
                    case RadialOffset.Left:
                        break;
                    case RadialOffset.Right:
                        break;
                    case RadialOffset.Center:
                        //vPos.x += (widths[i] * 0.5f);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (verticalOffset) {
                    case RadialOffset.Left:
                        break;
                    case RadialOffset.Right:
                        break;
                    case RadialOffset.Center:
                        vPos.y -= (heights[i] * 0.5f);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                children[i].SetAllocatedRect(vPos.x, vPos.y, widths[i], heights[i]);
                offsetAngle += 24f;
            }
        }

    }

}