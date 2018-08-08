using JetBrains.Annotations;
using Src;

[UsedImplicitly]
[Template("Templates/UIPanel.xml")]
public class UIPanel : UIElement {

    public void OnCreate() {
        view.CreateImagePrimitive(this);
    }

}