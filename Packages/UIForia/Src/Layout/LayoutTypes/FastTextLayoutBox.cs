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

        protected override float ComputeContentWidth(float blockWidth) {
            return textInfo.ComputeContentWidth(blockWidth);
        }
        
        protected override float ComputeContentHeight(float width, float blockWidth, float blockHeight) {
            return textInfo.ComputeHeightForWidth(width, blockWidth, blockHeight);
        }

        public override float GetIntrinsicMinWidth() {
            return textInfo.GetIntrinsicMinWidth();
        }
        
        protected override float GetIntrinsicMinHeight() {
            return textInfo.GetIntrinsicMinHeight();
        }

        protected override float GetIntrinsicMaxWidth() {
            return textInfo.GetIntrinsicWidth();
        }
        
        protected override float GetIntrinsicMaxHeight() {
            return textInfo.GetIntrinsicHeight();
        }

        public override void OnStyleChanged(StructList<StyleProperty> changeList) {
            base.OnStyleChanged(changeList);
            MarkNeedsLayout();
        }

        protected override void PerformLayout() {
            
            float topOffset = paddingBox.top - borderBox.top;
            float leftOffset = paddingBox.left + borderBox.left;

            contentSize = textInfo.Layout(new Vector2(leftOffset, topOffset), size.width - paddingBox.left - paddingBox.right - borderBox.right - borderBox.left);

//            actualWidth = size.width + PaddingHorizontal + BorderHorizontal;
//            actualHeight = size.height + resolvedPaddingBottom + resolvedBorderBottom;
        }

     
    }

}