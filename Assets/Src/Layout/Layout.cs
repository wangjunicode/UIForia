using Rendering;
using UnityEngine;

namespace Src.Layout {

    public abstract class UILayout {

        public abstract void Run(UIElement element);

    }

    public class UILayout_Auto : UILayout {

        public override void Run(UIElement element) {
            
            Rect rect = element.transform.GetLayoutRect();
            
            for (int i = 0; i < element.children.Length; i++) {
                UIElement child = element.children[i];
                child.transform.position = rect.position;
                child.transform.width = x
                
                rect = new Rect(rect) { y = rect.y + child.style.contentBox.totalHeight };
                
                
                
            }
            
        }

    }

    public class FlexLayout : UILayout {

        private UITransform transform;
        
        public override void Run(UIElement element) {
                                         
        }

    }
}