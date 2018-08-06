using System.Collections.Generic;
using Src;
using UnityEngine;

public class UIElement {

    public UIElement[] children;
    public TemplateContext providedContext;
    public TemplateContext referenceContext;
    public UIElementTemplate originTemplate;
    public ObservedProperty[] observedProperties;
    
    public UIElement() {
        
    }

    public virtual void Initialize() { }

    public virtual void OnPropsChanged(List<object> props) { }

    public virtual void OnShown() { }

    public virtual void OnHidden() { }

    public virtual void OnDestroyed() { }

    public UIElement[] Children {
        set {
            if (children == null) children = value;
        }
    }

    public ObservedProperty GetProperty(string bindingValue) {
        for (int i = 0; i < observedProperties.Length; i++) {
            if (observedProperties[i].name == bindingValue) {
                return observedProperties[i];
            }
        }
        return null;
    }


    public override string ToString() {
        string retn = string.Empty;
        retn += "<" + GetType().Name + ">";
        return retn;
    }

}