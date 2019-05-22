using System.Collections.Generic;
using UI;
using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace Documentation.Features {

    [Template("Documentation/Features/WindowDemo.xml")]
    public class WindowDemo : UIElement {

        public List<UIView> windowViews = new List<UIView>();

        public void OnButtonClick() {
            
            Vector2 position = layoutResult.screenPosition;
            
            UIView view = Application.CreateView("Window " + windowViews.Count, new Rect(position.x, position.y, 800, 600));
            
            KlangWindow window = Application.CreateElement<KlangWindow>();

            windowViews.Add(view);
            
            view.AddChild(window);

        }
        
    }

}