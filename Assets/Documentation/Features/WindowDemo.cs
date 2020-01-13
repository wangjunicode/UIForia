using UI;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UnityEngine;

namespace Documentation.Features {

    [Template("Documentation/Features/WindowDemo.xml")]
    public class WindowDemo : UIElement {

        private int windowCount;
        
        public void OnButtonClick() {
            
            Vector2 position = layoutResult.screenPosition;
            
            windowCount++;
            UIView view = Application.CreateView<KlangWindow>("Window " + windowCount, new Size(Application.Width, Application.Height));
            view.focusOnMouseDown = true;

            ((KlangWindow) view.RootElement.GetChild(0)).onClose += () => {
                windowCount--;
                view.Destroy();
            };
        }
    }
}