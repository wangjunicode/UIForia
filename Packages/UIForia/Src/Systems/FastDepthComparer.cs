using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Layout;

namespace UIForia.Systems {

    public class FastDepthComparer : IComparer<FastLayoutBox> {

        public int Compare(FastLayoutBox a, FastLayoutBox b) {
            
            // if (a.layer != b.layer) {
            //     return a.layer > b.layer ? -1 : 1;
            // }

            if (a.element.View.Depth != b.element.View.Depth) {
                return a.element.View.Depth - b.element.View.Depth;
            }

            if (a.zIndex != b.zIndex) {
                return a.zIndex - b.zIndex;
            }

            // SiblingRenderOrder

            if (a.parent == b.parent) {
                return a.element.siblingIndex - b.element.siblingIndex;
            }

            UIElement left = a.element;
            UIElement right = b.element;
            if (a.element.depth > b.element.depth) {
                for (int i = 0; i < a.element.depth - b.element.depth; i++) {
                    left = left.parent;
                    if (left == b.element) {
                        return 1;
                    }
                }
            }
            else if (a.element.depth < b.element.depth) {
                for (int i = 0; i < b.element.depth - a.element.depth; i++) {
                    right = right.parent;
                    if (right == a.element) {
                        return -1;
                    }
                }
            }

            bool loop = true;
            while (loop) {
                if (left.parent == right.parent) {
                    return left.siblingIndex - right.siblingIndex;
                }
                left = left.parent;
                right = right.parent;
            }
            
            // Depth

            // traversal index -- break ties between things at the same depth
            
            return a.traversalIndex - b.traversalIndex;

        }

    }

}