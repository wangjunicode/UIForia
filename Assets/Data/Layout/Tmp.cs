using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;

[Template("Data/Layout/Tmp.xml")]
public class Tmp : UIElement {

    public float Time => UnityEngine.Time.time;

    public RepeatableList<ISelectOption<string>> options = new RepeatableList<ISelectOption<string>>(new [] {
        new SelectOption<string>("hallo", "hallo"), 
        new SelectOption<string>("hallo", "hallo"), 
        new SelectOption<string>("hallo", "hallo"), 
        new SelectOption<string>("hallo", "hallo"), 
    });


}