using Src;

[Template("Demos/ChatWindow/ChatWindow.xml")]
public class ChatWindow : UIElement {

    public bool runnning;
    public override void OnUpdate() {
        if (!runnning) {
            FindById("child-to-animate").style.PlayAnimation(ChatWindow_Styles.AnimateTransform());
            runnning = true;
        }
    }

}