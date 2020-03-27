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
            // UIWindow window = application.CreateView<KlangWindow>("Window " + windowCount, new Size(application.Width, application.Height));
            // view.focusOnMouseDown = true;
            //
            // ((KlangWindow) view.RootElement.GetChild(0)).onClose += () => {
            //     windowCount--;
            //     view.Destroy();
            // };
        }
    }
}