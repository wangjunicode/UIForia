using Rendering;
using Src;

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

    public void OnInitialRender() {
        textRenderElement = view.CreateTextPrimitive(this);
        textRenderElement.Text = label;
        //apply font styles
        //textRenderElement.font = style.GetFont();view.FontTree.GetFontForElement(this);
    }

    public UIText(UIView view) : base(view) { }

}