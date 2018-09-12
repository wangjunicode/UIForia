using UnityEngine;

namespace Src.Systems {

    public struct LayoutResult {

        public readonly Rect rect;
        public readonly Rect localRect;
        public readonly UIElement element;

        // todo fix local rect its just for demo
        public LayoutResult(UIElement element, Rect rect, Rect localRect) {
            this.element = element;
            this.rect = rect;
            this.localRect = localRect;
        }

    }

}