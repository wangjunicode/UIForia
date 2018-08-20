using UnityEngine;

namespace Src.Systems {

    public struct LayoutResult {

        public readonly Rect rect;
        public readonly int elementId;

        public LayoutResult(int elementId, Rect rect) {
            this.elementId = elementId;
            this.rect = rect;
        }

    }

}