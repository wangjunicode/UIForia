using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;
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

    public class RadialLayoutBox : FastLayoutBox {

        private List<float> widths;
        private List<float> heights;

        protected override void PerformLayout() {
            UIStyleSet style = element.style;
            float dist = ResolveFixedWidth(style.RadialLayoutRadius);

            float startAngle = style.RadialLayoutStartAngle;
            float maxAngle = style.RadialLayoutEndAngle;

            // todo support radial offsets
            // todo handle edge cases

            var ptr = firstChild;
            BlockSize blockWidth = containingBoxWidth;
            BlockSize blockHeight = containingBoxHeight;
            AdjustBlockSizes(ref blockWidth, ref blockHeight);
            SizeConstraints sizeConstraints = default;
            OffsetRect margin = default;
            var childCount = 0;
            
            while (ptr != null) {

                if (ptr.element.isEnabled && ptr.element.style.Visibility != Visibility.Hidden) {
                    childCount++;
                }
                ptr = ptr.nextSibling;
            }

            float contentAreaWidth = size.width - paddingBox.left - paddingBox.right - borderBox.left - borderBox.right;
            float contentAreaHeight = size.height - paddingBox.top - paddingBox.bottom - borderBox.top - borderBox.bottom;
            float topOffset = paddingBox.top + borderBox.top;
            float leftOffset = paddingBox.left + borderBox.left;

            float horizontalAlignment = element.style.StackLayoutAlignHorizontal;
            float verticalAlignment = element.style.StackLayoutAlignVertical;

            float step = ((maxAngle - startAngle)) / childCount;
            Vector2 center = new Vector2(containingBoxWidth.contentAreaSize * 0.5f, containingBoxHeight.contentAreaSize * 0.5f);

            ptr = firstChild;
            while (ptr != null) {

                if (ptr.element.isDisabled || ptr.element.style.Visibility == Visibility.Hidden) {
                    continue;
                }

                ptr.GetWidth(blockWidth, ref sizeConstraints);
                ptr.GetMarginHorizontal(sizeConstraints.prefWidth, ref margin);
                float clampedWidth = Mathf.Max(sizeConstraints.minWidth, Mathf.Min(sizeConstraints.maxWidth, sizeConstraints.prefWidth));
                ptr.GetHeight(clampedWidth, blockWidth, blockHeight, ref sizeConstraints);
                ptr.GetMarginVertical(sizeConstraints.prefHeight, ref margin);
                float clampedHeight = Mathf.Max(sizeConstraints.minHeight, Mathf.Min(sizeConstraints.maxHeight, sizeConstraints.prefHeight));

                ptr.ApplyHorizontalLayout(leftOffset, blockWidth, contentAreaWidth, clampedWidth, horizontalAlignment, LayoutFit.None);
                ptr.ApplyVerticalLayout(topOffset, blockHeight, contentAreaHeight, clampedHeight, verticalAlignment, LayoutFit.None);
                
                float x = dist * Mathf.Cos(startAngle * Mathf.Deg2Rad);
                float y = dist * Mathf.Sin(startAngle * Mathf.Deg2Rad);

                Vector2 vPos = new Vector2(center.x + x, center.y + y);
                
                ptr.alignedPosition = new Vector2(vPos.x - (blockWidth.size * 0.5f), vPos.y - (blockHeight.size * 0.5f));
                startAngle += step;
                
                ptr = ptr.nextSibling;
            }

            contentSize.width = contentAreaWidth;
            contentSize.height = contentAreaHeight;
        }

        public override float GetIntrinsicMinWidth() {
            throw new NotImplementedException();
        }

        public override float GetIntrinsicMinHeight() {
            throw new NotImplementedException();
        }

        protected override float ComputeIntrinsicPreferredWidth() {
            throw new NotImplementedException();
        }

        public override float GetIntrinsicPreferredHeight() {
            throw new NotImplementedException();
        }

        public override float ComputeContentWidth(BlockSize blockWidth) {
            throw new NotImplementedException();
        }

        public override float ComputeContentHeight(float width, BlockSize blockWidth, BlockSize blockHeight) {
            throw new NotImplementedException();
        }
    }

}