using System;
using UIForia.Elements;

namespace UIForia.Layout {

    public class TranscludeLayoutBox : FastLayoutBox {

        public override float GetIntrinsicMinWidth() {
            throw new NotImplementedException("Should never call GetIntrinsicMinWidth on a transcluded layout box");
        }

        public override float GetIntrinsicMinHeight() {
            throw new NotImplementedException("Should never call GetIntrinsicMinHeight on a transcluded layout box");
        }

        public override float GetIntrinsicPreferredWidth() {
            throw new NotImplementedException("Should never call GetIntrinsicMaxWidth on a transcluded layout box");
        }

        public override float GetIntrinsicPreferredHeight() {
            throw new NotImplementedException("Should never call GetIntrinsicMaxHeight on a transcluded layout box");
        }

        public override float ComputeContentWidth(BlockSize blockWidth) {
            throw new NotImplementedException();
        }

        public override float ComputeContentHeight(float width, BlockSize blockWidth, BlockSize blockHeight) {
            throw new NotImplementedException();
        }

        protected override void PerformLayout() {
            throw new NotImplementedException("Should never call PerformLayout on a transcluded layout box");
        }

    }

}