using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UnityEngine;

namespace Documentation.Features {
    
    [Template("Documentation/Features/LayerDemo.xml")]
    public class LayerDemo : UIElement {
        public override void OnEnable() {
            UIView view = application.CreateView<LayerDemoView>("left", new Size(400, 400));
            view.RootElement.style.SetBackgroundColor(Color.red, StyleState.Normal);
            view.Depth = 10;
            
            view = application.CreateView<LayerDemoView>("right", new Size(400, 400));
            view.RootElement.style.SetBackgroundColor(Color.blue, StyleState.Normal);
            view.RootElement.style.SetTransformPositionX(600, StyleState.Normal);
            view.Depth = 10;
        }
    }

    [Template("Documentation/Features/LayerDemo.xml#view")]
    public class LayerDemoView : UIElement {
        
    }
}