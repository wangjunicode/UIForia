using System;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Layout {

    public class StackLayoutBox : FastLayoutBox {

        protected override void PerformLayout() {
            BlockSize blockWidth = containingBoxWidth;
            BlockSize blockHeight = containingBoxHeight;

            AdjustBlockSizes(ref blockWidth, ref blockHeight);

            FastLayoutBox ptr = firstChild;
            SizeConstraints sizeConstraints = default;
            OffsetRect margin = default;

            float contentAreaWidth = size.width - paddingBox.left - paddingBox.right - borderBox.left - borderBox.right;
            float contentAreaHeight = size.height - paddingBox.top - paddingBox.bottom - borderBox.top - borderBox.bottom;
            float topOffset = paddingBox.top + borderBox.top;
            float leftOffset = paddingBox.left + borderBox.left;

            float horizontalAlignment = element.style.StackLayoutAlignHorizontal;
            float verticalAlignment = element.style.StackLayoutAlignVertical;

            while (ptr != null) {
                ptr.GetWidth(blockWidth, ref sizeConstraints);
                ptr.GetMarginHorizontal(blockWidth, ref margin);
                float clampedWidth = Mathf.Max(sizeConstraints.minWidth, Mathf.Min(sizeConstraints.maxWidth, sizeConstraints.prefWidth));
                ptr.GetHeight(clampedWidth, blockWidth, blockHeight, ref sizeConstraints);
                ptr.GetMarginVertical(blockHeight, ref margin);
                float clampedHeight = Mathf.Max(sizeConstraints.minHeight, Mathf.Min(sizeConstraints.maxHeight, sizeConstraints.prefHeight));

                ptr.ApplyHorizontalLayout(leftOffset, blockWidth, contentAreaWidth, clampedWidth, horizontalAlignment, LayoutFit.None);
                ptr.ApplyVerticalLayout(topOffset, blockHeight, contentAreaHeight, clampedHeight, verticalAlignment, LayoutFit.None);

                ptr = ptr.nextSibling;
            }

            contentSize.width = contentAreaWidth;
            contentSize.height = contentAreaHeight;
        }

        public override float GetIntrinsicMinWidth() {
            throw new NotImplementedException();
        }

        public override float GetIntrinsicMinHeight() {
            float retn = float.MaxValue;

            if (firstChild == null) return 0;

            BlockSize blockWidth = containingBoxWidth;
            BlockSize blockHeight = containingBoxHeight;

            AdjustBlockSizes(ref blockWidth, ref blockHeight);

            FastLayoutBox ptr = firstChild;
            SizeConstraints sizeConstraints = default;
            OffsetRect margin = default;
            while (ptr != null) {
                ptr.GetWidth(blockWidth, ref sizeConstraints);
                ptr.GetMarginHorizontal(blockWidth, ref margin);
                float clampedWidth = Mathf.Max(sizeConstraints.minWidth, Mathf.Min(sizeConstraints.maxWidth, sizeConstraints.prefWidth)) + margin.left + margin.right;
                ptr.GetHeight(clampedWidth, blockWidth, blockHeight, ref sizeConstraints);
                ptr.GetMarginVertical(blockHeight, ref margin);
                float clampedHeight = Mathf.Max(sizeConstraints.minHeight, Mathf.Min(sizeConstraints.maxHeight, sizeConstraints.prefHeight)) + margin.top + margin.bottom;

                if (clampedHeight < retn) retn = clampedHeight;
                ptr = ptr.nextSibling;
            }

            return retn;
        }

        public override float GetIntrinsicPreferredWidth() {
            float retn = 0;

            FastLayoutBox ptr = firstChild;
            
            while (ptr != null) {
                float width = ptr.GetIntrinsicPreferredWidth();
                // ptr.GetMarginHorizontal();
                if (width > retn) retn = width;
                ptr = ptr.nextSibling;
            }

            return retn + paddingBox.left + paddingBox.right + borderBox.left + borderBox.right;
        }

        public override float GetIntrinsicPreferredHeight() {
            throw new NotImplementedException();
        }

        public override float ComputeContentWidth(BlockSize blockWidth) {
            float retn = 0;

            BlockSize blockHeight = default;

            if (prefHeight.unit == UIMeasurementUnit.Content) {
                blockHeight.size = containingBoxHeight.size;
                blockHeight.contentAreaSize = containingBoxHeight.contentAreaSize;
            }
            else {
                blockHeight.size = size.height;
                blockHeight.contentAreaSize = size.height - paddingBox.top - paddingBox.bottom - borderBox.top - borderBox.bottom;
            }

            AdjustBlockSizes(ref blockWidth, ref blockHeight);

            FastLayoutBox ptr = firstChild;
            SizeConstraints sizeConstraints = default;
            OffsetRect margin = default;

            while (ptr != null) {
                ptr.GetWidth(blockWidth, ref sizeConstraints);
                ptr.GetMarginHorizontal(blockWidth, ref margin);
                float clampedWidth = Mathf.Max(sizeConstraints.minWidth, Mathf.Min(sizeConstraints.maxWidth, sizeConstraints.prefWidth)) + margin.left + margin.right;
                if (clampedWidth > retn) retn = clampedWidth;
                ptr = ptr.nextSibling;
            }

            return retn;
        }

        public override float ComputeContentHeight(float width, BlockSize blockWidth, BlockSize blockHeight) {
            float retn = 0;

            AdjustBlockSizes(ref blockWidth, ref blockHeight);

            FastLayoutBox ptr = firstChild;
            SizeConstraints sizeConstraints = default;
            OffsetRect margin = default;
            while (ptr != null) {
                ptr.GetWidth(blockWidth, ref sizeConstraints);
                ptr.GetMarginHorizontal(blockWidth, ref margin);
                float clampedWidth = Mathf.Max(sizeConstraints.minWidth, Mathf.Min(sizeConstraints.maxWidth, sizeConstraints.prefWidth)) + margin.left + margin.right;
                ;
                ptr.GetHeight(clampedWidth, blockWidth, blockHeight, ref sizeConstraints);
                ptr.GetMarginVertical(blockHeight, ref margin);
                float clampedHeight = Mathf.Max(sizeConstraints.minHeight, Mathf.Min(sizeConstraints.maxHeight, sizeConstraints.prefHeight)) + margin.top + margin.bottom;

                if (clampedHeight > retn) retn = clampedHeight;
                ptr = ptr.nextSibling;
            }

            return retn;
        }

    }

}
