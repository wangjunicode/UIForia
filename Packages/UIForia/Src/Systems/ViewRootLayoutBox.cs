using UIForia.Layout;

namespace UIForia.Systems {

    public class ViewRootLayoutBox : FastLayoutBox {

        public override float GetIntrinsicMinWidth() {
            if (firstChild == null) {
                return 0;
            }

            return firstChild.GetIntrinsicMinWidth();
        }

        public override float GetIntrinsicMinHeight() {
            if (firstChild == null) {
                return 0;
            }

            return firstChild.GetIntrinsicMinHeight();
        }

        public override float GetIntrinsicPreferredWidth() {
            if (firstChild == null) {
                return 0;
            }

            return firstChild.GetIntrinsicPreferredWidth();
        }

        public override float GetIntrinsicPreferredHeight() {
            if (firstChild == null) {
                return 0;
            }

            return firstChild.GetIntrinsicPreferredHeight();
        }

        public override float ComputeContentWidth(BlockSize blockWidth) {
            blockWidth.size = element.View.Viewport.width;
            blockWidth.contentAreaSize = element.View.Viewport.width;
            return firstChild?.ComputeContentWidth(blockWidth) ?? 0;
        }

        public override float ComputeContentHeight(float width, BlockSize blockWidth, BlockSize blockHeight) {
            blockWidth.size = element.View.Viewport.width;
            blockWidth.contentAreaSize = element.View.Viewport.width;
            
            blockHeight.size = element.View.Viewport.height;
            blockHeight.contentAreaSize = element.View.Viewport.height;
            return firstChild?.ComputeContentHeight(width, blockWidth, blockHeight) ?? 0;
        }

        protected override void PerformLayout() {
            
            float width = element.View.Viewport.width;
            float height = element.View.Viewport.height;
            
            SizeConstraints targetSize = default;
            
            if (firstChild == null) return;

            BlockSize blockWidth = default;
            BlockSize blockHeight = default;
            blockWidth.size = element.View.Viewport.width;
            blockWidth.contentAreaSize = element.View.Viewport.width;
            
            blockHeight.size = element.View.Viewport.height;
            blockHeight.contentAreaSize = element.View.Viewport.height;
            
            firstChild.GetWidth(blockWidth, ref targetSize);
            firstChild.GetHeight(targetSize.prefWidth, blockWidth, blockHeight, ref targetSize);
            
            firstChild.ApplyHorizontalLayout(0, blockWidth, width, targetSize.prefWidth, default, Fit.Unset);
            firstChild.ApplyVerticalLayout(0, blockHeight, height, targetSize.prefHeight, default, Fit.Unset);
            
            // todo figure out how a view can take the size of it's contents
         
            
        }

    }

}