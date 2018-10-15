using Src;

[Template("Demos/ChatWindow/ChatWindow.xml")]
public class ChatWindow : UIElement {

    public override void OnReady() {
        FindById("child-to-animate").style.PlayAnimation(ChatWindow_Styles.AnimateTransform());
    }

}