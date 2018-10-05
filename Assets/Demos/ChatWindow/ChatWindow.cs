using Src;

[Template("Demos/ChatWindow/ChatWindow.xml")]
public class ChatWindow : UIElement {


    public ClippedPanel.ClippedCorner clippedCorner = ClippedPanel.ClippedCorner.BottomLeft;
    
    public void ChangeCorner() {
        
        if (clippedCorner == ClippedPanel.ClippedCorner.BottomRight) {
            clippedCorner = ClippedPanel.ClippedCorner.BottomLeft;
        }
        else if (clippedCorner == ClippedPanel.ClippedCorner.BottomLeft) {
            clippedCorner = ClippedPanel.ClippedCorner.BottomRight;
        }
        
    }

}