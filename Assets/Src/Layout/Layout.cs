using Rendering;
using UnityEngine;

namespace Src.Layout {

    public abstract class UILayout {

        public abstract void Run(UIElement element);

    }

    public class UILayout_Auto : UILayout {

        public override void Run(UIElement element) {
                        
        }

    }

    public class FlexLayout : UILayout {

        private UITransform transform;
        
        public override void Run(UIElement element) {
                                         
        }

    }
}