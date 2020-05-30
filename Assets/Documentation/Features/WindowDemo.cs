using UI;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;
using UIForia.Rendering;
using UnityEngine;

namespace Documentation.Features {

    [Template("Features/WindowDemo.xml")]
    public class WindowDemo : UIElement {

        private int windowCount;
        
        public void OnButtonClick() {
            
            Vector2 position = layoutResult.screenPosition;
            
            windowCount++;
            UIView view = application.CreateView<KlangWindow>("Window " + windowCount, new Size(application.Width, application.Height));
            view.focusOnMouseDown = true;

            ((KlangWindow) view.RootElement.FindChildAt(0)).onClose += () => {
                windowCount--;
                view.Destroy();
            };
        }
    }
}