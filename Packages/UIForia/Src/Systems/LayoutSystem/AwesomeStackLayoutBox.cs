using UIForia.Layout;
using UIForia.Util;

namespace UIForia.Systems {

    public class AwesomeStackLayoutBox : AwesomeLayoutBox {

        protected override float ComputeContentWidth() {
            AwesomeLayoutBox ptr = firstChild;
            float retn = 0f;
            while (ptr != null) {
                LayoutSize size = default;
                ptr.GetWidths(ref size);
                float clampedWidth = size.Clamped + size.marginStart + size.marginEnd;
                if (clampedWidth > retn) retn = clampedWidth;
                ptr = ptr.nextSibling;
            }

            return retn;
        }

        protected override float ComputeContentHeight() {
            AwesomeLayoutBox ptr = firstChild;
            float retn = 0f;
            while (ptr != null) {
                LayoutSize size = default;
                ptr.GetHeights(ref size);
                float clampedHeight = size.Clamped + size.marginStart + size.marginEnd;
                if (clampedHeight > retn) retn = clampedHeight;
                ptr = ptr.nextSibling;
            }

            return retn;
        }

        public override void OnChildrenChanged(LightList<AwesomeLayoutBox> childList) { }

        public override void RunLayoutHorizontal(int frameId) {
            AwesomeLayoutBox ptr = firstChild;

            float contentAreaWidth = finalWidth - (paddingBorderHorizontalStart + paddingBorderHorizontalEnd);

            float alignment = element.style.AlignItemsHorizontal;
            
            while (ptr != null) {
                LayoutSize size = default;
                ptr.GetWidths(ref size);
                float clampedWidth = size.Clamped;
                // todo - i think margin / alignment are not correct
                // todo distribute extra space (around and between are converted to center since thats what makes sense
                float x = 0;
                float originBase = x + size.marginStart;
                float originOffset = contentAreaWidth * alignment;
                float offset = clampedWidth * -alignment;
                float alignedPosition = originBase + originOffset + offset;
                ptr.ApplyLayoutHorizontal(x, alignedPosition, clampedWidth, contentAreaWidth, LayoutFit.None, frameId);
                ptr = ptr.nextSibling;
            }
        }

        public override void RunLayoutVertical(int frameId) {
              AwesomeLayoutBox ptr = firstChild;

            float contentAreaHeight = finalHeight - (paddingBorderVerticalStart + paddingBorderVerticalEnd);

            float alignment = element.style.AlignItemsVertical;
            
            while (ptr != null) {
                LayoutSize size = default;
                ptr.GetHeights(ref size);
                float clampedHeight = size.Clamped;
                // todo - i think margin / alignment are not correct
                // todo distribute extra space (around and between are converted to center since thats what makes sense
                float y = 0;
                float originBase = y + size.marginStart;
                float originOffset = contentAreaHeight * alignment;
                float offset = clampedHeight * -alignment;
                float alignedPosition = originBase + originOffset + offset;
                ptr.ApplyLayoutVertical(y, alignedPosition, clampedHeight, contentAreaHeight, LayoutFit.None, frameId);
                ptr = ptr.nextSibling;
            }
        }

    }

}