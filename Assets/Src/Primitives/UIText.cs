using Rendering;
using Src;
using UnityEngine;

public class UIText : UIElementPrimitive {
    
    private TextPrimitive textRenderElement;
    
    [Prop] public string label;

    public void OnPropsChanged() {
        textRenderElement.Text = label;
    }

    // this should be done at instaniate time when the view is in scope
//    public void OnCreate() {
//        textRenderElement = view.CreateTextPrimitive(this);
//        textRenderElement.Text = label;
//    }

    public void ApplyFontSettings(TextStyle fontSettings) {
        textRenderElement.ApplyFontSettings(fontSettings);
    }
    
}