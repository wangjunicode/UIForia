using System.Collections.Generic;
using UnityEngine;

namespace Src.Layout {

    public abstract class UILayout {

        public abstract void Run(Rect viewport, LayoutDataSet size, Rect[] results);

        public static readonly FlowLayout Flow = new FlowLayout();
        public static readonly FlexLayout Flex = new FlexLayout();
        public static readonly FixedLayout Fixed = new FixedLayout();

        public virtual float GetContentWidth(LayoutData data, float contentSize, float viewportSize) {
            List<LayoutData> children = data.children;
            // todo include statically positioned things who's breadth exceeds max computed
            float output = 0;
            // return sum of preferred sizes
            for (int i = 0; i < children.Count; i++) {
                LayoutData child = children[i];
                output += child.GetPreferredWidth(data.preferredWidth.unit, contentSize, viewportSize);
            }

            return output;
        }
        
        public virtual float GetContentHeight(LayoutData data, float contentSize, float viewportSize) {
            List<LayoutData> children = data.children;
            // todo include statically positioned things who's breadth exceeds max computed
            float output = 0;
            // return sum of preferred sizes
            for (int i = 0; i < children.Count; i++) {
                LayoutData child = children[i];
                output += child.GetPreferredHeight(data.preferredHeight.unit, contentSize, viewportSize);
            }

            return output;
        }

    }

}