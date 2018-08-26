using Rendering;
using UnityEngine;

namespace Src.Layout {

    public abstract class UILayout {

        protected readonly ITextSizeCalculator textSizeCalculator;

        protected UILayout(ITextSizeCalculator textSizeCalculator) {
            this.textSizeCalculator = textSizeCalculator;
        }

        public abstract void Run(Rect viewport, LayoutNode layoutNode, Rect[] results);

        // todo -- handle text x
        public virtual float GetContentWidth(LayoutNode node, float contentSize, float viewportSize) {
//            if ((data.element.flags & UIElementFlags.TextElement) != 0) {
//                return data.textContentSize.x;
//            }

            LayoutNode[] children = node.children;
            // todo include statically positioned things who's breadth exceeds max computed
            float output = 0;
            if (node.parameters.direction == LayoutDirection.Row) {
                // return sum of preferred sizes
                for (int i = 0; i < children.Length; i++) {
                    LayoutNode child = children[i];
                    output += child.GetPreferredWidth(node.rect.width.unit, contentSize, viewportSize);
                }
            }
            else {

                for (int i = 0; i < children.Length; i++) {
                    LayoutNode child = children[i];
                    output = Mathf.Max(output, child.GetPreferredWidth(child.rect.width.unit, contentSize, viewportSize));
                }

            }

            return output;
        }

        public float GetContentHeight(LayoutNode node, float parentWidth, float contentSize, float viewportSize) {

            if (node.isTextElement) {
                // todo -- add metrics per component about calc calls
                if (node.previousParentWidth != parentWidth) {
                    node.previousParentWidth = parentWidth;
                    node.textContentSize.y = textSizeCalculator.CalcTextHeight(node.textContent, node.style, parentWidth);
                }
                return node.textContentSize.y;
            }

            LayoutNode[] children = node.children;

            // todo include statically positioned things who's breadth exceeds max computed
            float output = 0;

            if (node.parameters.direction == LayoutDirection.Row) {
                for (int i = 0; i < children.Length; i++) {
                    output = Mathf.Max(output, children[i].GetPreferredHeight(node.rect.height.unit, parentWidth, contentSize, viewportSize));
                }
            }
            else {
                for (int i = 0; i < children.Length; i++) {
                    output += children[i].GetPreferredHeight(node.rect.height.unit, parentWidth, contentSize, viewportSize);
                }
            }

            return output;
        }

    }

}