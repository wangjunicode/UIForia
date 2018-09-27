using System.Collections.Generic;
using Rendering;
using Src.Systems;
using UnityEngine;

namespace Src.Layout {

    public abstract class UILayout {

        protected readonly ITextSizeCalculator textSizeCalculator;

        protected UILayout(ITextSizeCalculator textSizeCalculator) {
            this.textSizeCalculator = textSizeCalculator;
        }
        
        public abstract List<Rect> Run(Rect viewport, LayoutNode layoutNode);

        public float GetTextWidth(string text, UIStyleSet style) {
            return textSizeCalculator.CalcTextWidth(text, style);
        }

        public float GetTextHeight(string text, UIStyleSet style, float width) {
            return textSizeCalculator.CalcTextHeight(text, style, width);
        }
        
        public virtual float GetContentWidth(LayoutNode node, float contentSize, float viewportSize) {

            if (node.isTextElement) {
                return node.preferredTextWidth;
            }

            // if node has intrinsic width 
            if (node.isImageElement) {
                
            }
            
            List<LayoutNode> children = node.children;
            
            // todo include statically positioned things who's breadth exceeds max computed
            
            float output = 0;
            if (node.element.style.layoutDirection == LayoutDirection.Row) {
                // return sum of preferred sizes
                for (int i = 0; i < children.Count; i++) {
                    LayoutNode child = children[i];
                    if (!child.isInFlow || child.element.isDisabled) continue;
                    output += child.GetPreferredWidth(node.rect.width.unit, contentSize, viewportSize);
                }
            }
            else {

                for (int i = 0; i < children.Count; i++) {
                    LayoutNode child = children[i];
                    if (!child.isInFlow || child.element.isDisabled) continue;
                    output = Mathf.Max(output, child.GetPreferredWidth(child.rect.width.unit, contentSize, viewportSize));
                }

            }

            return output;
        }

        public virtual float GetContentHeight(LayoutNode node, float adjustedWidth, float parentWidth, float viewportSize) {

            if (node.isTextElement) {
                return node.GetTextHeight(adjustedWidth);
            }

            List<LayoutNode> children = node.children;

            // todo include statically positioned things who's breadth exceeds max computed
            float output = 0;

            if (node.element.style.layoutDirection == LayoutDirection.Row) {
                for (int i = 0; i < children.Count; i++) {
                    if (!children[i].isInFlow || children[i].element.isDisabled) continue;

                    output = Mathf.Max(output, children[i].GetPreferredHeight(node.rect.height.unit, adjustedWidth, parentWidth, viewportSize));
                }
            }
            else {
                for (int i = 0; i < children.Count; i++) {
                    if (!children[i].isInFlow || children[i].element.isDisabled) continue;
                    output += children[i].GetPreferredHeight(node.rect.height.unit, adjustedWidth, parentWidth, viewportSize);
                }
            }

            return output;
        }

    }

}