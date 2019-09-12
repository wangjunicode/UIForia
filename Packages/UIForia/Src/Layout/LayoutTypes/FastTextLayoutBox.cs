using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Layout {

    public class FastTextLayoutBox : FastLayoutBox {

        private TextInfo textInfo;

        public override void OnInitialize() {
            textInfo = ((UITextElement) element).textInfo;
        }

        public override float ComputeContentWidth(BlockSize blockWidth) {
            return textInfo.ComputeContentWidth(blockWidth.contentAreaSize);
        }

        public override float ComputeContentHeight(float width, BlockSize blockWidth, BlockSize blockHeight) {
            return textInfo.ComputeHeightForWidth(width, blockWidth, blockHeight);
        }

        public override float GetIntrinsicMinWidth() {
            return textInfo.GetIntrinsicMinWidth();
        }

        public override float GetIntrinsicMinHeight() {
            return textInfo.GetIntrinsicMinHeight();
        }

        protected override float ComputeIntrinsicPreferredWidth() {
            return textInfo.GetIntrinsicWidth();
        }

        public override float GetIntrinsicPreferredHeight() {
            return textInfo.GetIntrinsicHeight();
        }

        public override void OnStyleChanged(StructList<StyleProperty> changeList) {
            base.OnStyleChanged(changeList);
            MarkForLayout();
        }

        protected override void PerformLayout() {
            
            float topOffset = paddingBox.top - borderBox.top;
            float leftOffset = paddingBox.left + borderBox.left;

            // need a greedy size or some way to know
            // if text were multiple lines it would probably work
            contentSize = textInfo.Layout(new Vector2(leftOffset, topOffset), size.width - paddingBox.left - paddingBox.right - borderBox.right - borderBox.left);

//            actualWidth = size.width + PaddingHorizontal + BorderHorizontal;
//            actualHeight = size.height + resolvedPaddingBottom + resolvedBorderBottom;
        }

     
    }

}