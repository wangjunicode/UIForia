using System.Collections.Generic;
using UnityEngine;

public class UIElement {
    public Style style;
    public ContentBox contentBox;
    public UIElement[] children;
    public PropertyBinding[] bindings;
    public TemplateContext templateContext;
    public readonly GameObject gameObject;
    public readonly List<ObservedProperty> dirtyProperties;
    
    public UIElement() {
        gameObject = new GameObject();
        gameObject.AddComponent<RectTransform>();
        dirtyProperties = new List<ObservedProperty>();
    }

    public virtual void Initialize(List<object> props) {
       
    }

    public virtual void OnPropsChanged(List<object> props) {
        
    }

    public virtual void OnShown() { }

    public virtual void OnHidden() { }

    public virtual void OnDestroyed() { }

    public UIElement[] Children {
        set {
            if (children == null) children = value;
        }
    }

    public void MarkDirty() {
        for (int i = 0; i < dirtyProperties.Count; i++) {
            ObservedProperty dirtyProperty = dirtyProperties[i];
            List<TemplateContext.Binding> b = templateContext.GetBoundElements(dirtyProperty.name);
            for (int j = 0; j < b.Count; j++) {
//                b.__inputProperties.Add(dirtyProperty);
//                b.properties[""].Value = dirtyProperty.Value;
            }
        }
    }

}