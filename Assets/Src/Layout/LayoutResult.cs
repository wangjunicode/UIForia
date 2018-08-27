using UnityEngine;

namespace Src.Systems {

    public struct LayoutResult {

        public readonly Rect rect;
        public readonly Rect localRect;
        public readonly int elementId;

        // todo fix local rect its just for demo
        public LayoutResult(int elementId, Rect rect, Rect localRect) {
            this.elementId = elementId;
            this.rect = rect;
            this.localRect = localRect;
        }

    }

}