using System.Collections.Generic;
using UnityEngine;

public class UIView {

    public readonly List<UIElement> dirtyElements;
    public readonly List<TemplateContext> dirtyContexts;
    private readonly List<UIElement> renderQueue;
    
    public void FlushChanges() {

        for (int i = 0; i < dirtyElements.Count; i++) {
            UIElement element = dirtyElements[i];
            FlushDirtyProperties(element);
        }
        
    }

    public void UpdateRendering() {
        /*
         * for each dirty element
         *     if needs creation -> push to create queue
         *     if needs layout
         *     
         */
    
        RectTransform x;
    
        // hide
        // show
        // create
        // destroy
        // style
        // layout
    }

    public void UpdateLayout() { }

    private void FlushDirtyProperties(UIElement element) {
        TemplateContext context = element.templateContext;
        
        for (int i = 0; i < context.dirtyBindings.Count; i++) {
            string name = context.dirtyBindings[i];
            List<TemplateContext.Binding> dirtyBindings = context.GetBoundElements(name);
            // for each dirty element
            // update dirty prop values via reflection
            // invoke on props updated handle
            // send to render system for layout / styling / painting
            
        }
   
    }
    
}

public class UIElement {
    public Style style;
    public ContentBox contentBox;
    public UIElement[] children;
    public PropertyBinding[] bindings;
    public TemplateContext templateContext;
    public readonly GameObject gameObject;
    public readonly List<ObservedProperty> dirtyProperties;
    public readonly UIElement root;
    
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
        // root.MarkDirty(this)
        
       
    }

}