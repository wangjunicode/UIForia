using UnityEngine;

namespace Src.Systems {

    public struct LayoutResult {

        public readonly Rect rect;
        public readonly UIElement element;

        public LayoutResult(UIElement element, Rect rect) {
            this.element = element;
            this.rect = rect;
        }

    }

}