using System.Collections.Generic;

namespace UIForia.Elements {

    public class ZIndexComparerAscending : IComparer<UIElement> {

        public int Compare(UIElement a, UIElement b) {

            int zIndexA = a.style.ZIndex;
            int zIndexB = b.style.ZIndex;

            if (zIndexA > zIndexB) {
                return -1;
            }

            if (zIndexB > zIndexA) {
                return 1;
            }

            if (a.depth != b.depth) {
                return a.depth > b.depth ? -1 : 1;
            }

            if (a.parent == b.parent) {
                return a.siblingIndex > b.siblingIndex ? -1 : 1;
            }

            if (a.parent == null) return -1;
            if (b.parent == null) return 1;

            return a.parent.siblingIndex > b.parent.siblingIndex ? -1 : 1;
        }

    }

}