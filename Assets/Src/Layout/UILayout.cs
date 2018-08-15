
using UnityEngine;

namespace Src.Layout {

    public abstract class UILayout {

        public abstract LayoutData Run(Rect viewport, LayoutData parentLayoutData, UIElement element);

        public static readonly FlowLayout Flow = new FlowLayout();
        
    }

}