using System.Collections.Generic;
using JetBrains.Annotations;
using Src;

[UsedImplicitly]
[Template("Templates/UIPanel.xml")]
public class UIPanel : UIElement {
    [Prop] public bool visible;
    
    public void OnPropsChanged() {
        
    }

}