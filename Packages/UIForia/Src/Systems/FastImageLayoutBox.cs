using UIForia.Elements;

namespace UIForia.Layout {

    public class FastImageLayoutBox : FastLayoutBox {

        private UIImageElement imageElement;
        
        public override void OnInitialize() {
            imageElement = (UIImageElement) element;
        }

        public override float GetIntrinsicMinWidth() {
            if (imageElement.texture == null) {
                return 0;
            }
            return imageElement.texture.width;
        }

        public override float GetIntrinsicMinHeight() {
            if (imageElement.texture == null) {
                return 0;
            }

            return imageElement.texture.height;
        }

        protected override float ComputeIntrinsicPreferredWidth() {
            if (imageElement.texture == null) {
                return 0;
            }
            return imageElement.texture.width;
        }

        public override float GetIntrinsicPreferredHeight() {
            if (imageElement.texture == null) {
                return 0;
            }

            return imageElement.texture.height;
        }

        public override float ComputeContentWidth(BlockSize blockWidth) {
            if (imageElement.texture == null) {
                return 0;
            }
            return imageElement.texture.width;
        }

        public override float ComputeContentHeight(float width, BlockSize blockWidth, BlockSize blockHeight) {
            // todo -- if preserving aspect ratio do that
            if (imageElement.texture == null) {
                return 0;
            }

            return imageElement.texture.height;
        }

        protected override void PerformLayout() {
        }

    }

}