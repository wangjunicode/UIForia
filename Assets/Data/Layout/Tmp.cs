using UIForia.Attributes;
using UIForia.Elements;

[Template("Data/Layout/Tmp.xml")]
public class Tmp : UIElement {

    public float Time => UnityEngine.Time.time;

}