using UI;
using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace Documentation.Features {

    [Template("Documentation/Features/WindowDemo.xml")]
    public class WindowDemo : UIElement {

        private int windowCount;
        
        public void OnButtonClick() {
            
            Vector2 position = layoutResult.screenPosition;
            
            windowCount++;
            UIView view = Application.CreateView("Window " + windowCount, new Rect(position.x, position.y, Application.Width, Application.Height), typeof(KlangWindow));
            view.focusOnMouseDown = true;

            ((KlangWindow) view.RootElement.GetChild(0)).onClose += () => {
                windowCount--;
                view.Destroy();
            };
        }
    }
}