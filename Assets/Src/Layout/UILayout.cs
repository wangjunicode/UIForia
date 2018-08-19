
using UnityEngine;

namespace Src.Layout {

    public abstract class UILayout {

        public abstract void Run(Rect viewport, LayoutData data);

        public static readonly FlowLayout Flow = new FlowLayout();
        
    }

}