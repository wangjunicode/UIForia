using System.Collections.Generic;
using Rendering;
using Src;
using Src.Layout;
using UnityEngine;


public class UIElement {

    public readonly int id;
    public UIElement[] children;
    public TemplateContext providedContext;
    public TemplateContext referenceContext;
    public UIElementTemplate originTemplate;
    public ObservedProperty[] observedProperties;
    public UILayout layout;
    public UIStyle style;
    
    public readonly UIView view;
    
    public UIElement(UIView view) {
        id = UIView.NextElementId;
        this.view = view;
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
    
    public override string ToString() {
        string retn = string.Empty;
        retn += "<" + GetType().Name + ">";
        return retn;
    }

}