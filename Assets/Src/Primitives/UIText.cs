using Rendering;
using Src;
using UnityEngine;

public class UIText : UIElementPrimitive {

    // Text
    // RawImage
    // Image
    // Mask
    // Mask2D
    // InputField
    // TextArea
    
    private TextPrimitive textRenderElement;
    
    [Prop] public string label;

    public void OnPropsChanged() {
        textRenderElement.Text = label;
    }

    public void OnCreate() {
        textRenderElement = view.CreateTextPrimitive(this);
        textRenderElement.Text = label;
        view.MarkForRendering(this);
    }

    public void ApplyFontSettings(TextStyle fontSettings) {
        textRenderElement.ApplyFontSettings(fontSettings);
    }
    
}