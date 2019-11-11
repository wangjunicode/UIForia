using System;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public class AwesomeTextLayoutBox : AwesomeLayoutBox {

        private TextInfo textInfo;
        public Action onTextContentChanged;
        private bool textAlreadyDirty;
        
        protected override void OnInitialize() {
            onTextContentChanged = onTextContentChanged ?? HandleTextContentChanged;
            textInfo = ((UITextElement) element).textInfo;
            textInfo.onTextLayoutRequired += onTextContentChanged;
            textAlreadyDirty = true;
            flags |= (AwesomeLayoutBoxFlags.RequireLayoutHorizontal | AwesomeLayoutBoxFlags.RequireLayoutHorizontal);
        }

        private void HandleTextContentChanged() {
            flags |= (AwesomeLayoutBoxFlags.RequireLayoutHorizontal | AwesomeLayoutBoxFlags.RequireLayoutHorizontal);
            finalWidth = -1;
            finalHeight = -1;
            if (textAlreadyDirty) return;
            textAlreadyDirty = true;
            AwesomeLayoutBox ptr = parent;

            while (ptr != null) {

                // once we hit a block provider we can safely stop traversing since the provider's parent doesn't care about content size changing
                bool stop = (ptr.flags & AwesomeLayoutBoxFlags.WidthBlockProvider) != 0;
                // can't break out if already flagged for layout because parent of parent might not be and might be content sized
                ptr.flags |= AwesomeLayoutBoxFlags.RequireLayoutHorizontal;
                ptr.element.layoutHistory.AddLogEntry(LayoutDirection.Horizontal, -1, LayoutReason.DescendentStyleSizeChanged);
                if (stop) break;
                ptr = ptr.parent;
            }

            ptr = parent;
            
            while (ptr != null) {
                // once we hit a block provider we can safely stop traversing since the provider's parent doesn't care about content size changing
                bool stop = (ptr.flags & AwesomeLayoutBoxFlags.HeightBlockProvider) != 0;

                // can't break out if already flagged for layout because parent of parent might not be and might be content sized
                ptr.flags |= AwesomeLayoutBoxFlags.RequireLayoutVertical;
                ptr.element.layoutHistory.AddLogEntry(LayoutDirection.Vertical, -1, LayoutReason.DescendentStyleSizeChanged);
                if (stop) break;
                ptr = ptr.parent;
            }
        }

        protected override void OnDestroy() {
            textInfo.onTextLayoutRequired -= onTextContentChanged;
        }

        protected override float ComputeContentWidth() {
            // by definition when computing width in the width pass we only care about its natural width
            return textInfo.ComputeContentWidth(float.MaxValue);
        }

        protected override float ComputeContentHeight() {
            // todo -- might need to subtract padding / border from this value
            return textInfo.ComputeContentWidth(finalWidth);
        }

        public override void OnChildrenChanged(LightList<AwesomeLayoutBox> childList) {
            
        }

        public override void RunLayoutHorizontal(int frameId) {
            textAlreadyDirty = false;
            textInfo.ForceLayout(); // might not need this
            float topOffset = paddingBorderVerticalStart;
            float leftOffset = paddingBorderHorizontalStart;
            textInfo.Layout(new Vector2(leftOffset, topOffset), finalWidth - paddingBorderHorizontalStart - paddingBorderHorizontalEnd);
        }

        public override void RunLayoutVertical(int frameId) {
            
        }

    }

}