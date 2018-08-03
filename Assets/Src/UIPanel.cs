using System.Collections.Generic;
using JetBrains.Annotations;

[UsedImplicitly]
public class UIPanel : UIElement {
    [Prop] public bool visible;
    
    public override void Initialize(List<object> props) {
        visible = true;
    }

    public void OnPropsChanged() {
        
    }

}