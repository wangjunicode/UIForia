using Rendering;
using UnityEngine;

namespace Src.Layout {

    public abstract class UILayout {

        public abstract void Run(UIView view, Rect rect, UIElement element);

        public abstract float GetLayoutHeight(UIElement element);

        public abstract float GetLayoutWidth(UIElement element);

        public static readonly FlowLayout Flow = new FlowLayout();
        
    }

}