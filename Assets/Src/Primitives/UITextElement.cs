using Rendering;
using Src;

public class UITextElement : UIElement {
    
    public TextPrimitive textRenderElement;
    
    [Prop] public string label;

    public void ApplyFontSettings(TextStyle fontSettings) {
        textRenderElement.Text = label;
        textRenderElement.ApplyFontSettings(fontSettings);
    }
    
}