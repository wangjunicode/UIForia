using System;
using UIForia.Util;

namespace UIForia.Layout {

    public class TranscludeLayoutBox : FastLayoutBox {

        public override float GetIntrinsicMinWidth() {
            throw new NotImplementedException("Should never call GetIntrinsicMinWidth on a transcluded layout box");
        }

        public override float GetIntrinsicMinHeight() {
            throw new NotImplementedException("Should never call GetIntrinsicMinHeight on a transcluded layout box");
        }

        protected override float ComputeIntrinsicPreferredWidth() {
            throw new NotImplementedException("Should never call GetIntrinsicMaxWidth on a transcluded layout box");
        }

        public override float GetIntrinsicPreferredHeight() {
            throw new NotImplementedException("Should never call GetIntrinsicMaxHeight on a transcluded layout box");
        }

        public override float ComputeContentWidth(BlockSize blockWidth) {
            throw new NotImplementedException("Should never call ComputeContentWidth on a transcluded layout box");
        }

        public override float ComputeContentHeight(float width, BlockSize blockWidth, BlockSize blockHeight) {
            throw new NotImplementedException("Should never call ComputeContentHeight on a transcluded layout box");
        }

        protected override void PerformLayout() {
            // this only ever gets called if the element was enabled this frame and not part of the regular update code
        }

        public override void AddChild(FastLayoutBox child) {
            parent.AddChild(child);
        }

        public override void SetChildren(LightList<FastLayoutBox> container) {
            for (int i = 0; i < container.size; i++) {
                parent.AddChild(container[i]);
            }
        }

        public override void RemoveChild(FastLayoutBox child) {
            parent.RemoveChild(child);
        }
        
    }

}