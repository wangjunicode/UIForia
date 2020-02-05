using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;

[Template("Data/Layout/Tmp.xml")]
public class Tmp : UIElement {

    public string selectedValue;
    
    public List<ISelectOption<string>> options = new List<ISelectOption<string>>() {
        new SelectOption<string>("hello", "world"),
        new SelectOption<string>("hello", "world"),
        new SelectOption<string>("hello", "world"),
        new SelectOption<string>("hello", "world"),
        new SelectOption<string>("hello", "world")
    };

}