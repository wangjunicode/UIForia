using UnityEngine;
using UnityEngine.UI;

public class UIText : UIElementPrimitive {

    private Text textComponent;

    [Prop] public string label;

    public void OnPropsChanged() {
        textComponent.text = label;
    }

//    public override GameObject Create() {
//        GameObject go = new GameObject();
//        go.AddComponent<RectTransform>();
//        textComponent = go.AddComponent<Text>();
//        textComponent.text = label;
//        return go;
//    }

    public virtual void ApplyUpdates() {
        textComponent.text = label;
    }
    
}